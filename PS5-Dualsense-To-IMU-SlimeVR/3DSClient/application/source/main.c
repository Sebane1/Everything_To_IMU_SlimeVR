#include <3ds.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <unistd.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <arpa/inet.h>
#include <3ds/services/apt.h>

#define SERVER_PORT 9305
#define CONFIG_PATH "3ds/IMU_Over_UDP/imu_stream.cfg"
#define DEFAULT_SERVER_IP "10.0.0.21"

typedef struct {
    s16 ax, ay, az;
    s16 gx, gy, gz;
} ImuPacket;

// Reads IP from config or returns default
void getServerIp(char* ipBuffer, size_t bufferSize) {
    FILE* file = fopen(CONFIG_PATH, "r");
    if (file) {
        fgets(ipBuffer, bufferSize, file);
        // Remove newline character if present
        ipBuffer[strcspn(ipBuffer, "\r\n")] = 0;
        fclose(file);
    }
    else {
        strncpy(ipBuffer, DEFAULT_SERVER_IP, bufferSize);
    }
}

int main() {
    aptSetSleepAllowed(false);
    gfxInitDefault();
    consoleInit(GFX_TOP, NULL);

    u32* soc_buffer = (u32*)aligned_alloc(0x1000, 0x100000);
    if (!soc_buffer) {
        printf("aligned_alloc failed\n");
        gfxExit();
        return 1;
    }

    if (socInit(soc_buffer, 0x100000) < 0) {
        printf("socInit failed\n");
        free(soc_buffer);
        gfxExit();
        return 1;
    }

    char ipAddress[64];
    getServerIp(ipAddress, sizeof(ipAddress));
    printf("Using server IP: %s\n", ipAddress);

    int sock = socket(AF_INET, SOCK_DGRAM, 0);
    if (sock < 0) {
        printf("Socket creation failed\n");
        socExit();
        free(soc_buffer);
        gfxExit();
        return 1;
    }

    struct sockaddr_in serverAddr;
    memset(&serverAddr, 0, sizeof(serverAddr));
    serverAddr.sin_family = AF_INET;
    serverAddr.sin_port = htons(SERVER_PORT);
    inet_pton(AF_INET, ipAddress, &serverAddr.sin_addr);

    HIDUSER_EnableAccelerometer();
    HIDUSER_EnableGyroscope();

    printf("Streaming IMU data... Press START to exit\n");

    int frame = 0;
    while (aptMainLoop()) {
        hidScanInput();
        if (hidKeysDown() & KEY_START) break;

        accelVector accel = { 0 };
        angularRate gyro = { 0 };

        hidAccelRead(&accel);
        hidGyroRead(&gyro);

        ImuPacket packet = {
            .ax = accel.x,
            .ay = accel.y,
            .az = accel.z,
            .gx = gyro.x,
            .gy = gyro.y,
            .gz = gyro.z,
        };

        sendto(sock, &packet, sizeof(packet), 0,
            (struct sockaddr*)&serverAddr, sizeof(serverAddr));

        if (++frame % 50 == 0) {
            printf("\x1b[1;1H"); // Top-left
            printf("Streaming IMU data... Press START to exit\n");
            printf("Accel: %6d %6d %6d\n", accel.x, accel.y, accel.z);
            printf("Gyro : %6d %6d %6d\n", gyro.x, gyro.y, gyro.z);
        }

        svcSleepThread(10000000); // 10ms
    }

    close(sock);
    socExit();
    free(soc_buffer);
    gfxExit();
    return 0;
}