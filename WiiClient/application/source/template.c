#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <malloc.h>
#include <ogcsys.h>
#include <gccore.h>
#include <network.h>
#include <debug.h>
#include <errno.h>
#include <wiiuse/wpad.h>
#include <math.h>

typedef struct {
	float x;
	float y;
	float z;
} Vector;

typedef struct {
	float w;
	float x;
	float y;
	float z;
} Quaternion;

// ----- CONFIG -----
#define PATH "/"
#define BUFLEN 512
#define MOTIONPLUS_DELAY_FRAMES 60
#define DEFAULT_SERVER_IP "10.0.0.21"
#define DEFAULT_SERVER_PORT 9909

static char server_ip[32] = DEFAULT_SERVER_IP;
static int server_port = DEFAULT_SERVER_PORT;
// -------------------

static int persistent_sock = -1;
static void* xfb = NULL;
static GXRModeObj* rmode = NULL;

// Function declarations
uint32_t to_little_endian_u32(uint32_t val);
uint32_t count_connected_wiimotes();
void send_http_post_binary(uint8_t* payload, int payload_len);
void float_to_little_endian(float val, uint8_t* out);
Vector normalize_vector(float x, float y, float z);
Quaternion quat_from_gravity(float x, float y, float z, float cx, float cy, float cz, float scale);
Vector quaternion_to_euler(Quaternion q);
void get_app_directory(char* out_path, size_t out_size, char* argv0);
void load_config(const char* app_dir);

int main(int argc, char** argv) {
	VIDEO_Init();
	WPAD_Init();

	rmode = VIDEO_GetPreferredMode(NULL);
	xfb = MEM_K0_TO_K1(SYS_AllocateFramebuffer(rmode));

	console_init(xfb, 20, 20, rmode->fbWidth, rmode->xfbHeight, rmode->fbWidth * VI_DISPLAY_PIX_SZ);

	VIDEO_Configure(rmode);
	VIDEO_SetNextFramebuffer(xfb);
	VIDEO_SetBlack(false);
	VIDEO_Flush();
	VIDEO_WaitVSync();
	if (rmode->viTVMode & VI_NON_INTERLACE) VIDEO_WaitVSync();

	if (net_init() < 0) {
		printf("net_init failed!\n");
	}

	printf("\x1b[2;0H");
	printf("Wiimote IMU Forwarder!\n");

	char app_dir[256];
	get_app_directory(app_dir, sizeof(app_dir), argv[0]);
	load_config(app_dir);

	char localip[16] = { 0 };
	char gateway[16] = { 0 };
	char netmask[16] = { 0 };

	if (if_config(localip, gateway, netmask, true, 20) < 0) {
		printf("DHCP config failed\n");
		if (if_config("10.0.0.110", "10.0.0.1", "255.255.255.0", false, 2) < 0) {
			printf("Static IP config failed\n");
		}
	}
	else {
		printf("DHCP config success\n");
	}

	printf("IP: %s\n", localip);
	printf("Gateway: %s\n", gateway);
	printf("Netmask: %s\n", netmask);

	s16 wiimote_offset = 512;
	s16 nunchuck_offset = 512;
	bool formatSet[4] = { false, false, false, false };
	uint8_t buffer[37 * 4]; // 4 controllers max, each 37 bytes

	while (1) {
		WPAD_ScanPads();
		uint32_t connectedDevices = count_connected_wiimotes();
		int buffer_len = 0;

		for (uint32_t i = 0; i < connectedDevices; i++) {
			WPADData* wpad_data = WPAD_Data(i);
			if (!wpad_data) continue;

			if (!formatSet[i]) {
				WPAD_SetDataFormat(i, WPAD_FMT_BTNS_ACC);
				formatSet[i] = true;
			}

			u32 pressed = WPAD_ButtonsDown(i);
			if (pressed & WPAD_BUTTON_HOME) {
				if (persistent_sock >= 0) net_close(persistent_sock);
				exit(0);
			}

			s16 x = wpad_data->accel.x;
			s16 y = wpad_data->accel.y;
			s16 z = wpad_data->accel.z;
			u8 nunchuk_connected = 0;
			s16 nax = 0, nay = 0, naz = 0;

			if (wpad_data->exp.type == WPAD_EXP_NUNCHUK) {
				nunchuk_connected = 1;
				nax = wpad_data->exp.nunchuk.accel.x;
				nay = wpad_data->exp.nunchuk.accel.y;
				naz = wpad_data->exp.nunchuk.accel.z;
			}

			uint8_t* ptr = buffer + buffer_len;

			uint32_t id_le = to_little_endian_u32(i);
			memcpy(ptr, &id_le, 4); ptr += 4;
			Quaternion wm_quat = quat_from_gravity((float)x, (float)y, (float)z, wiimote_offset, wiimote_offset, wiimote_offset, 200.0f);
			float_to_little_endian(wm_quat.w, ptr); ptr += 4;
			float_to_little_endian(wm_quat.x, ptr); ptr += 4;
			float_to_little_endian(wm_quat.y, ptr); ptr += 4;
			float_to_little_endian(wm_quat.z, ptr); ptr += 4;

			if (nunchuk_connected) {
				Quaternion nc_quat = quat_from_gravity((float)nax, (float)nay, (float)naz, nunchuck_offset, nunchuck_offset, nunchuck_offset, 200.0f);
				float_to_little_endian(nc_quat.w, ptr); ptr += 4;
				float_to_little_endian(nc_quat.x, ptr); ptr += 4;
				float_to_little_endian(nc_quat.y, ptr); ptr += 4;
				float_to_little_endian(nc_quat.z, ptr); ptr += 4;
			}
			else {
				memset(ptr, 0, 16);
				ptr += 16;
			}

			*ptr = nunchuk_connected;
			ptr += 1;

			buffer_len += (ptr - (buffer + buffer_len));
		}

		if (buffer_len > 0) {
			send_http_post_binary(buffer, buffer_len);
		}
	}
}

void get_app_directory(char* out_path, size_t out_size, char* argv0) {
	if (!argv0 || argv0[0] == '\0') {
		strncpy(out_path, "sd:/apps/WiiImuForwarder", out_size - 1); // fallback default
		out_path[out_size - 1] = '\0';
		return;
	}
	strncpy(out_path, argv0, out_size - 1);
	out_path[out_size - 1] = '\0';
	char* last_slash = strrchr(out_path, '/');
	if (last_slash) {
		*last_slash = '\0'; // truncate to directory
	}
}

void load_config(const char* app_dir) {
	char config_path[256];
	snprintf(config_path, sizeof(config_path), "%s/config.txt", app_dir);

	FILE* f = fopen(config_path, "r");
	if (!f) {
		printf("No config.txt found, using defaults (%s:%d)\n", server_ip, server_port);
		return;
	}

	char line[128];
	while (fgets(line, sizeof(line), f)) {
		if (strncmp(line, "SERVER_IP=", 10) == 0) {
			strncpy(server_ip, line + 10, sizeof(server_ip) - 1);
			server_ip[sizeof(server_ip) - 1] = '\0'; // ensure null-terminated
			server_ip[strcspn(server_ip, "\r\n")] = 0; // strip newline
		}
		else if (strncmp(line, "SERVER_PORT=", 12) == 0) {
			server_port = atoi(line + 12);
		}
	}

	fclose(f);

	printf("Loaded config from: %s\n", config_path);
	printf("Server IP: %s\n", server_ip);
	printf("Server Port: %d\n", server_port);
}

int initialize_socket() {
	if (persistent_sock >= 0) {
		net_close(persistent_sock);
	}

	persistent_sock = net_socket(AF_INET, SOCK_STREAM, IPPROTO_IP);
	if (persistent_sock < 0) {
		printf("Failed to create socket\n");
		return -1;
	}

	struct sockaddr_in dest;
	memset(&dest, 0, sizeof(dest));
	dest.sin_family = AF_INET;
	dest.sin_port = htons(server_port);
	inet_aton(server_ip, &dest.sin_addr);

	if (net_connect(persistent_sock, (struct sockaddr*)&dest, sizeof(dest)) < 0) {
		printf("Failed to connect to server: errno=%d\n", errno);
		net_close(persistent_sock);
		persistent_sock = -1;
		return -1;
	}

	return 0;
}

void send_http_post_binary(uint8_t* payload, int payload_len) {
	if (persistent_sock < 0) {
		if (initialize_socket() != 0) {
			printf("Socket init failed\n");
			return;
		}
	}

	char request_header[256];
	char ip_string[64];
	snprintf(ip_string, sizeof(ip_string), "%s:%d", server_ip, server_port);

	int header_len = snprintf(request_header, sizeof(request_header),
		"POST %s HTTP/1.1\r\n"
		"Host: %s\r\n"
		"Content-Type: application/octet-stream\r\n"
		"Content-Length: %d\r\n"
		"Connection: keep-alive\r\n"
		"\r\n",
		PATH, ip_string, payload_len);

	int written = net_write(persistent_sock, request_header, header_len);
	if (written < 0) goto socket_error;

	written = net_write(persistent_sock, payload, payload_len);
	if (written < 0) goto socket_error;

	char response[128];
	int read_bytes = net_read(persistent_sock, response, sizeof(response));
	if (read_bytes <= 0) goto socket_error;

	return;

socket_error:
	printf("Socket error, resetting connection\n");
	net_close(persistent_sock);
	persistent_sock = -1;
}

Vector normalize_vector(float x, float y, float z) {
	float length = sqrtf(x * x + y * y + z * z);
	if (length == 0) return (Vector) { 0, 0, 0 };
	return (Vector) { x / length, y / length, z / length };
}

Quaternion quat_from_gravity(float x, float y, float z, float cx, float cy, float cz, float scale) {
	float sx = (x - cx) / scale;
	float sy = (y - cy) / scale;
	float sz = (z - cz) / scale;

	Vector gravity = normalize_vector(sx, sy, sz);
	Vector reference = { 0.0f, 0.0f, -1.0f };

	Vector axis = {
		gravity.y * reference.z - gravity.z * reference.y,
		gravity.z * reference.x - gravity.x * reference.z,
		gravity.x * reference.y - gravity.y * reference.x
	};

	float dot = gravity.x * reference.x + gravity.y * reference.y + gravity.z * reference.z;
	float angle = acosf(dot);
	axis = normalize_vector(axis.x, axis.y, axis.z);

	float half_angle = angle * 0.5f;
	float sin_half = sinf(half_angle);

	Quaternion q;
	q.w = cosf(half_angle);
	q.x = axis.x * sin_half;
	q.y = axis.y * sin_half;
	q.z = axis.z * sin_half;

	if (fabsf(dot - 1.0f) < 1e-5f) {
		q = (Quaternion){ 1.0f, 0.0f, 0.0f, 0.0f };
	}
	else if (fabsf(dot + 1.0f) < 1e-5f) {
		q = (Quaternion){ 0.0f, 1.0f, 0.0f, 0.0f };
	}

	return q;
}

uint32_t count_connected_wiimotes() {
	uint32_t count = 0;
	u32 type;
	for (int i = 0; i < WPAD_MAX_WIIMOTES; i++) {
		s32 status = WPAD_Probe(i, &type);
		if (status == WPAD_ERR_NONE) {
			count++;
		}
	}
	return count;
}

uint32_t to_little_endian_u32(uint32_t val) {
	return ((val & 0xFF000000) >> 24) |
		((val & 0x00FF0000) >> 8) |
		((val & 0x0000FF00) << 8) |
		((val & 0x000000FF) << 24);
}

void float_to_little_endian(float val, uint8_t* out) {
	union {
		float f;
		uint8_t b[4];
	} u;
	u.f = val;

	out[0] = u.b[3];
	out[1] = u.b[2];
	out[2] = u.b[1];
	out[3] = u.b[0];
}
