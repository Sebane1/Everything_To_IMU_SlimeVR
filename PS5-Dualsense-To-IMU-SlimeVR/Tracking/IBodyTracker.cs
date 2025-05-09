﻿using System.Numerics;

namespace Everything_To_IMU_SlimeVR.Tracking {
    public interface IBodyTracker {
        int Id { get; set; }
        string MacSpoof { get; set; }
        Vector3 Euler { get; set; }
        float LastHmdPositon { get; set; }
        bool SimulateThighs { get; set; }
        TrackerConfig.RotationReferenceType YawReferenceTypeValue { get; set; }
        string Debug { get; }

        void Rediscover();
        Vector3 GetCalibration();
    }
}