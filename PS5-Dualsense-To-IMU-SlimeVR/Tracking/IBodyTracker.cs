using System.Numerics;

namespace PS5_Dualsense_To_IMU_SlimeVR.Tracking {
    public interface IBodyTracker {
        int Id { get; set; }
        string MacSpoof { get; set; }
        Vector3 Euler { get; set; }
        float LastHmdPositon { get; set; }
    }
}