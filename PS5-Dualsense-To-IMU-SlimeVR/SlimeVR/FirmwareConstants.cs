using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Everything_To_IMU_SlimeVR.SlimeVR {
    public static class FirmwareConstants {

        public enum BoardType {
            UNKNOWN = 0,
            SLIMEVR_LEGACY = 1,
            SLIMEVR_DEV = 2,
            NODEMCU = 3,
            CUSTOM = 4,
            WROOM32 = 5,
            WEMOSD1MINI = 6,
            TTGO_TBASE = 7,
            ESP01 = 8,
            SLIMEVR = 9,
            LOLIN_C3_MINI = 10,
            BEETLE32C32 = 11,
            ES32C3DEVKITM1 = 12,
            OWOTRACK = 13,
            WRANGLER = 14,
            MOCOPI = 15,
            WEMOSWROOM02 = 16,
            XIAO_ESP32C3 = 17,
            HARITORA = 18,
            DEV_RESERVED = 250
        }

        public enum ImuType {
            UNKNOWN = 0,
            MPU9250 = 1,
            MPU6500 = 2,
            BNO080 = 3,
            BNO085 = 4,
            BNO055 = 5,
            MPU6050 = 6,
            BNO086 = 7,
            BMI160 = 8,
            ICM20948 = 9,
            ICM42688 = 10,
            BMI270 = 11,
            LSM6DS3TRC = 12,
            LSM6DSV = 13,
            LSM6DSO = 14,
            LSM6DSR = 15,
            DEV_RESERVED = 250
        }
        public enum McuType {
            UNKNOWN = 0,
            ESP8266 = 1,
            ESP32 = 2,
            OWOTRACK_ANDROID = 3,
            WRANGLER = 4,
            OWOTRACK_IOS = 5,
            ESP32_C3 = 6,
            MOCOPI = 7,
            HARITORA = 8,
            DEV_RESERVED = 250
        }

        public enum TrackerDataType {
            ROTATION = 0,
            FLEX_RESISTANCE = 1,
            FLEX_ANGLE = 2
        }

        public enum TrackerPosition {
            NONE = 0,
            HEAD = 1,
            NECK = 2,
            UPPER_CHEST = 3,
            CHEST = 4,
            WAIST = 5,
            HIP = 6,
            LEFT_UPPER_LEG = 7,
            RIGHT_UPPER_LEG = 8,
            LEFT_LOWER_LEG = 9,
            RIGHT_LOWER_LEG = 10,
            LEFT_FOOT = 11,
            RIGHT_FOOT = 12,
            LEFT_LOWER_AR = 13,
            RIGHT_LOWER_AR = 14,
            LEFT_UPPER_ARM = 15,
            RIGHT_UPPER_ARM = 16,
            LEFT_HAND = 17,
            RIGHT_HAND = 18,
            LEFT_SHOULDER = 19,
            RIGHT_SHOULDER = 20,
            LEFT_THUMB_METACARPAL = 21,
            LEFT_THUMB_PROXIMAL = 22,
            LEFT_THUMB_DISTAL = 23,
            LEFT_INDEX_PROXIMAL = 24,
            LEFT_INDEX_INTERMEDIATE = 25,
            LEFT_INDEX_DISTAL = 26,
            LEFT_MIDDLE_PROXIMAL = 27,
            LEFT_MIDDLE_INTERMEDIATE = 28,
            LEFT_MIDDLE_DISTAL = 29,
            LEFT_RING_PROXIMAL = 30,
            LEFT_RING_INTERMEDIATE = 31,
            LEFT_RING_DISTAL = 32,
            LEFT_LITTLE_PROXIMAL = 33,
            LEFT_LITTLE_INTERMEDIATE = 34,
            LEFT_LITTLE_DISTAL = 35,
            RIGHT_THUMB_METACARPAL = 36,
            RIGHT_THUMB_PROXIMAL = 37,
            RIGHT_THUMB_DISTAL = 38,
            RIGHT_INDEX_PROXIMAL = 39,
            RIGHT_INDEX_INTERMEDIATE = 40,
            RIGHT_INDEX_DISTAL = 41,
            RIGHT_MIDDLE_PROXIMAL = 42,
            RIGHT_MIDDLE_INTERMEDIATE = 43,
            RIGHT_MIDDLE_DISTAL = 44,
            RIGHT_RING_PROXIMAL = 45,
            RIGHT_RING_INTERMEDIATE = 46,
            RIGHT_RING_DISTAL = 47,
            RIGHT_LITTLE_PROXIMAL = 48,
            RIGHT_LITTLE_INTERMEDIATE = 49,
            RIGHT_LITTLE_DISTAL = 50
        }
        public class UDPPackets {
            public static int HEARTBEAT = 0;
            public static int ROTATION = 1;
            public static int GYRO = 2;
            public static int HANDSHAKE = 3;
            public static int ACCELERATION = 4;
            public static int MAG = 5;
            public static int RAW_CALIBRATION_DATA = 6;
            public static int CALIBRATION_FINISHED = 7;
            public static int CONFIG = 8;
            public static int RAW_MAGENTOMETER = 9;
            public static int PING_PONG = 10;
            public static int SERIAL = 11;
            public static int BATTERY_LEVEL = 12;
            public static int TAP = 13;
            public static int RESET_REASON = 14;
            public static int SENSOR_INFO = 15;
            public static int ROTATION_2 = 16;
            public static int ROTATION_DATA = 17;
            public static int MAGENTOMETER_ACCURACY = 18;

            public static int FLEX_DATA_PACKET = 26;
            public static int BUTTON_PUSHED = 60;
            public static int SEND_MAG_STATUS = 61;
            public static int CHANGE_MAG_STATUS = 62;


            public static int RECIEVE_HEARTBEAT = 1;
            public static int RECIEVE_VIBRATE = 2;
            public static int RECIEVE_HANDSHAKE = 3;
            public static int RECIEVE_COMMAND = 4;
        }

    }
}
