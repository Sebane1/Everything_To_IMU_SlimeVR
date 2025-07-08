using System;
using System.Runtime.InteropServices;

public class VQFWrapper : IDisposable {
    private IntPtr handle;

    private const string DllName = "vqf"; // or "libvqf" on macOS/Linux if not renamed

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr VQF_Create(double gyrTs, double accTs, double magTs);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    private static extern void VQF_Destroy(IntPtr handle);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    private static extern void VQF_Update(IntPtr handle, double[] gyr, double[] acc, double[] mag);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    private static extern void VQF_GetQuat9D(IntPtr handle, double[] quatOut);
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    private static extern void VQF_GetQuat6D(IntPtr handle, double[] quatOut);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    private static extern void VQF_SetTauAcc(IntPtr handle, double tauAcc);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    private static extern void VQF_SetTauMag(IntPtr handle, double tauMag);
    public VQFWrapper(double gyrTs, double accTs = -1.0, double magTs = -1.0) {
        handle = VQF_Create(gyrTs, accTs, magTs);
    }
    public void SetTauAcc(double tauAcc) => VQF_SetTauAcc(handle, tauAcc);
    public void SetTauMag(double tauMag) => VQF_SetTauMag(handle, tauMag);

    public void Update(double[] gyro, double[] accel, double[]? mag = null) {
        if (gyro.Length != 3 || accel.Length != 3) {
            throw new ArgumentException("Gyro and accel must have 3 elements.");
        }
        VQF_Update(handle, gyro, accel, mag);
    }


    public double[] GetQuat9D() {
        double[] quat = new double[4];
        VQF_GetQuat9D(handle, quat);
        return quat;
    }

    public double[] GetQuat6D() {
        double[] quat = new double[4];
        VQF_GetQuat9D(handle, quat);
        return quat;
    }

    public void Dispose() {
        if (handle != IntPtr.Zero) {
            VQF_Destroy(handle);
            handle = IntPtr.Zero;
        }
        GC.SuppressFinalize(this);
    }

    ~VQFWrapper() => Dispose();
}
