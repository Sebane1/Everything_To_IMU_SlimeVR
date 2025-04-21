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

#define SRV_IP "10.0.0.21"
#define BUFLEN 512
#define PORT 9909

static void* xfb = NULL;
static GXRModeObj* rmode = NULL;

int udp_sock = -1;
struct sockaddr_in dest_addr;

uint32_t to_little_endian_u32(uint32_t val);
u16 to_little_endian_u16(u16 val);
uint32_t count_connected_wiimotes();
void send_udp_packet(uint32_t id, u16 wm_ax, u16 wm_ay, u16 wm_az,
	u16 nc_ax, u16 nc_ay, u16 nc_az, u8 nunchuk_connected);

//---------------------------------------------------------------------------------
int main(int argc, char** argv) {
	//---------------------------------------------------------------------------------

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
	printf("Hello World!\n");

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

	printf("About to create socket...\n");
	printf("AF_INET=%d, SOCK_DGRAM=%d, IPPROTO_IP=%d\n", AF_INET, SOCK_DGRAM, IPPROTO_IP);

	if (udp_sock >= 0) {
		net_close(udp_sock);
		udp_sock = -1;
	}

	// Setup UDP socket
	udp_sock = net_socket(AF_INET, SOCK_DGRAM, IPPROTO_IP);

	if (udp_sock < 0) {
		printf("Socket creation failed: errno=%d\n", errno);
	}
	else {
		printf("Socket created successfully: %d\n", udp_sock);
	}

	struct sockaddr_in local_addr;
	memset(&local_addr, 0, sizeof(local_addr));
	local_addr.sin_family = AF_INET;
	local_addr.sin_port = htons(50000); // ephemeral port
	local_addr.sin_addr.s_addr = INADDR_ANY;

	if (net_bind(udp_sock, (struct sockaddr*)&local_addr, sizeof(local_addr)) < 0) {
		printf("bind failed: errno=%d (%s)\n", errno, strerror(errno));
	}
	else {
		printf("bind succeeded!\n");
	}


	memset(&dest_addr, 0, sizeof(dest_addr));
	dest_addr.sin_family = AF_INET;
	dest_addr.sin_port = htons(PORT);
	if (inet_aton(SRV_IP, &dest_addr.sin_addr) == 0) {
		printf("inet_aton failed\n");
	}

	bool setMotionPlus[4] = { false, false, false, false };
	int frame_counter = 0;

	while (1) {
		WPAD_ScanPads();
		uint32_t connectedDevices = count_connected_wiimotes();
		for (uint32_t i = 0; i < connectedDevices; i++) {
			WPADData* wpad_data = WPAD_Data(i);
			if (wpad_data) {
				u32 pressed = WPAD_ButtonsDown(i);

				if (pressed & WPAD_BUTTON_HOME) {
					if (udp_sock >= 0) {
						net_close(udp_sock);
						udp_sock = -1;
					}

					exit(0);
				}

				u16 x = wpad_data->accel.x;
				u16 y = wpad_data->accel.y;
				u16 z = wpad_data->accel.z;
				u8 nunchuk_connected = 0;
				u16 nax = 0, nay = 0, naz = 0;

				if (wpad_data->exp.type == WPAD_EXP_NUNCHUK) {
					nunchuk_connected = 1;
					nax = wpad_data->exp.nunchuk.accel.x;
					nay = wpad_data->exp.nunchuk.accel.y;
					naz = wpad_data->exp.nunchuk.accel.z;
				}

				printf("\x1b[%d;0H", 7 + i * 3);
				printf("Wiimote %u: X=%u Y=%u Z=%u   \n", i, x, y, z);

				printf("\x1b[%d;0H", 8 + i * 3);
				if (nunchuk_connected) {
					printf("Nunchuk %u: X=%u Y=%u Z=%u   \n", i, nax, nay, naz);
				}
				else {
					printf("Nunchuk %u: Not connected       \n", i);
				}

				if (!setMotionPlus[i]) {
					s32 value = WPAD_SetMotionPlus(i, 1);
					if (value == WPAD_ERR_NONE) {
						setMotionPlus[i] = true;
					}
				}

				// Throttle: only send every 2 frames
				frame_counter++;
				if (frame_counter % 10 == 0) {
					send_udp_packet(i, x, y, z, nax, nay, naz, nunchuk_connected);
				}
			}
		}
		VIDEO_WaitVSync();
	}
}

void send_udp_packet(uint32_t id, u16 wm_ax, u16 wm_ay, u16 wm_az,
	u16 nc_ax, u16 nc_ay, u16 nc_az, u8 nunchuk_connected) {

	if (udp_sock < 0) {
		printf("Socket not initialized!\n");
		return;
	}

	uint32_t id_le = to_little_endian_u32(id);
	u16 wm_ax_le = to_little_endian_u16(wm_ax);
	u16 wm_ay_le = to_little_endian_u16(wm_ay);
	u16 wm_az_le = to_little_endian_u16(wm_az);
	u16 nc_ax_le = to_little_endian_u16(nc_ax);
	u16 nc_ay_le = to_little_endian_u16(nc_ay);
	u16 nc_az_le = to_little_endian_u16(nc_az);

	char buffer[17];
	memcpy(&buffer[0], &id_le, 4);
	memcpy(&buffer[4], &wm_ax_le, 2);
	memcpy(&buffer[6], &wm_ay_le, 2);
	memcpy(&buffer[8], &wm_az_le, 2);
	memcpy(&buffer[10], &nc_ax_le, 2);
	memcpy(&buffer[12], &nc_ay_le, 2);
	memcpy(&buffer[14], &nc_az_le, 2);
	buffer[16] = nunchuk_connected;

	s32 result = net_sendto(udp_sock, buffer, sizeof(buffer), 0,
		(struct sockaddr*)&dest_addr, sizeof(dest_addr));

	if (result <= 0) {
		printf("Send failed (result=%d): errno=%d\n", result, errno);
		for (int i = 0; i < 6; i++) {
			VIDEO_WaitVSync(); // ~100ms total
		}
	}
	else {
		printf("Sent %d bytes\n", result);
	}
	printf("Dest IP: 0x%08x (%s), Port: %d\n", dest_addr.sin_addr.s_addr,
		inet_ntoa(dest_addr.sin_addr), ntohs(dest_addr.sin_port));
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

u16 to_little_endian_u16(u16 val) {
	return (val >> 8) | (val << 8);
}