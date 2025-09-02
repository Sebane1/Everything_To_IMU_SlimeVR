// NuGet packages required:
// - NAudio (e.g., 2.x)
// This example uses NAudio.Dsp.FastFourierTransform
//
// What it does:
// - Captures desktop audio via WASAPI loopback
// - Computes FFT on short windows
// - Measures energy outside typical speech band (below 300 Hz and above 4 kHz by default)
// - Applies hysteresis thresholds to emit binary ON/OFF events
// - Uses stereo RMS comparison to pick LEFT / RIGHT / BOTH haptic targets
// - Calls into an IHaptics adapter that you can wire to your existing HapticsManager
//
// Usage example:
//   var monitor = new DesktopAudioHapticMonitor(new SlimeVrHapticsAdapter());
//   monitor.Start();
//   ...
//   monitor.Stop();
//
// Notes:
// - Keep FFT size small (e.g., 1024) for snappy response.
// - You can tune band edges & thresholds at the constructor.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Everything_To_IMU_SlimeVR.Osc;
using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;
using NAudio.Dsp;
using NAudio.Wave;
using Complex = NAudio.Dsp.Complex;

namespace Everything_To_IMU_SlimeVR.AudioHaptics {
    public enum HapticSide { Left, Right, Both }

    public interface IHaptics {
        void SetHaptics(HapticSide side, float intensity);
    }

    // Example adapter you can swap with your own implementation
    public class AudioHapticsAdapter : IHaptics {
        public void SetHaptics(HapticSide side, float intensity) {
            int durationMs = 200;

            // TODO: Replace with your real HapticsManager calls.
            // Below is illustrative only; wire to your existing nodes as you like.
            switch (side) {
                case HapticSide.Left:
                    HapticsManager.SetNodeVibration(HapticNodeBinding.LeftHand, durationMs, intensity);
                    HapticsManager.SetNodeVibration(HapticNodeBinding.LeftForeArm, durationMs, intensity);
                    HapticsManager.SetNodeVibration(HapticNodeBinding.LeftUpperArm, durationMs, intensity);
                    HapticsManager.SetNodeVibration(HapticNodeBinding.LeftThigh, durationMs, intensity);
                    HapticsManager.SetNodeVibration(HapticNodeBinding.LeftCalf, durationMs, intensity);
                    HapticsManager.SetNodeVibration(HapticNodeBinding.LeftShoulder, durationMs, intensity);
                    break;
                case HapticSide.Right:
                    HapticsManager.SetNodeVibration(HapticNodeBinding.RightHand, durationMs, intensity);
                    HapticsManager.SetNodeVibration(HapticNodeBinding.RightForeArm, durationMs, intensity);
                    HapticsManager.SetNodeVibration(HapticNodeBinding.RightUpperArm, durationMs, intensity);
                    HapticsManager.SetNodeVibration(HapticNodeBinding.RightThigh, durationMs, intensity);
                    HapticsManager.SetNodeVibration(HapticNodeBinding.RightCalf, durationMs, intensity);
                    HapticsManager.SetNodeVibration(HapticNodeBinding.RightShoulder, durationMs, intensity);
                    break;
                case HapticSide.Both:
                    HapticsManager.SetNodeVibration(HapticNodeBinding.Hips, durationMs, intensity);
                    HapticsManager.SetNodeVibration(HapticNodeBinding.Chest, durationMs, intensity);
                    HapticsManager.SetNodeVibration(HapticNodeBinding.ChestAndHips, durationMs, intensity);
                    HapticsManager.SetNodeVibration(HapticNodeBinding.ChestAndHipsBack, durationMs, intensity);
                    HapticsManager.SetNodeVibration(HapticNodeBinding.ChestAndHipsFront, durationMs, intensity);
                    HapticsManager.SetNodeVibration(HapticNodeBinding.ChestBack, durationMs, intensity);
                    HapticsManager.SetNodeVibration(HapticNodeBinding.ChestFront, durationMs, intensity);
                    break;
            }
        }
    }
    public class DefaultDeviceWatcher : IMMNotificationClient {
        public event Action<DeviceStateChangedEventArgs> DefaultDeviceChanged;

        public void OnDefaultDeviceChanged(DataFlow dataFlow, Role deviceRole, string defaultDeviceId) {
            if (dataFlow == DataFlow.Render && deviceRole == Role.Multimedia) {
                DefaultDeviceChanged?.Invoke(new DeviceStateChangedEventArgs(defaultDeviceId));
            }
        }

        public void OnDeviceAdded(string pwstrDeviceId) { }
        public void OnDeviceRemoved(string deviceId) { }
        public void OnDeviceStateChanged(string deviceId, DeviceState newState) { }
        public void OnPropertyValueChanged(string pwstrDeviceId, PropertyKey key) { }
    }

    public class DeviceStateChangedEventArgs : EventArgs {
        public string DeviceId { get; }
        public DeviceStateChangedEventArgs(string id) => DeviceId = id;
    }
    public class DesktopAudioHapticMonitor : IDisposable {
        // Config
        private readonly int _fftSize;
        private readonly int _fftPower;
        private readonly int _hopSize;
        private readonly float _lowBandMaxHz;
        private readonly float _highBandMinHz;
        private readonly float _onThresholdDb;
        private readonly float _offThresholdDb;
        private readonly float _stereoDbBias; // how many dB difference to decide Left vs Right

        // State
        private WasapiLoopbackCapture? _capture;
        private readonly object _bufferLock = new object();
        private float[] _ring; // interleaved stereo float samples
        private int _maximumDb;
        private MMDeviceEnumerator _enumerator;
        private DefaultDeviceWatcher _deviceWatcher;
        private int _ringWrite;
        private int _ringCount; // in samples (per-channel interleaved)

        private readonly IHaptics _haptics;
        private bool _isOn;
        private HapticSide _lastSide = HapticSide.Both;

        private Task? _worker;
        private CancellationTokenSource? _cts;
        private object _lock;

        public DesktopAudioHapticMonitor(
            IHaptics haptics,
            int fftSize = 1024,
            float lowBandMaxHz = 300f,
            float highBandMinHz = 4000f,
            float onThresholdDb = -28f,
            float offThresholdDb = -38f,
            float stereoDbBias = 3f // 3 dB means ~1.4x power
        ) {
            if ((fftSize & (fftSize - 1)) != 0) throw new ArgumentException("fftSize must be power of two");

            _haptics = haptics;
            _fftSize = fftSize;
            _fftPower = (int)Math.Round(Math.Log(fftSize, 2));
            _hopSize = fftSize; // process per event; you could use 50% overlap if you want
            _lowBandMaxHz = lowBandMaxHz;
            _highBandMinHz = highBandMinHz;
            _onThresholdDb = onThresholdDb;
            _offThresholdDb = offThresholdDb;
            _stereoDbBias = stereoDbBias;

            _ring = new float[fftSize * 20 * 2]; // ~20 frames of stereo
            _maximumDb = -23;
            CoInitializeEx(IntPtr.Zero, COINIT_APARTMENTTHREADED);

            _enumerator = new MMDeviceEnumerator();

            _deviceWatcher = new DefaultDeviceWatcher();

            // Listen for default device changes
            _deviceWatcher.DefaultDeviceChanged += (e) => {
                Console.WriteLine("Default device changed → restarting capture");
                RestartCapture();
            };
            _enumerator.RegisterEndpointNotificationCallback(_deviceWatcher);

        }
        float NormalizeIntensity(float value) {
            float min = _onThresholdDb;
            float max = _maximumDb;

            // Clamp the value first to avoid going outside the expected range
            value = Math.Clamp(value, min, max);

            // Map to 0.0f - 1.0f
            return (value - min) / (max - min);
        }

        float NormalizeIntensityInverted(float value) {
            float min = _onThresholdDb;
            float max = _maximumDb;

            value = Math.Clamp(value, min, max);

            // Normalized 0..1
            float t = (value - min) / (max - min);

            // Invert
            return 1.0f - t;
        }

        [DllImport("ole32.dll")]
        private static extern int CoInitializeEx(IntPtr pvReserved, uint dwCoInit);

        private const uint COINIT_APARTMENTTHREADED = 0x2; // STA

        private void RestartCapture() {

            _capture?.StopRecording();

            var device = _enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            _capture = new WasapiLoopbackCapture(device);
            _capture.DataAvailable += OnData;
            _capture.StartRecording();

        }

        public void Start() {
            if (_capture != null) return;
            _cts = new CancellationTokenSource();

            _capture = new WasapiLoopbackCapture();
            _capture.DataAvailable += OnData;
            _capture.StartRecording();

            _worker = Task.Run(() => WorkerLoop(_cts.Token));
        }

        public void Stop() {
            try {
                _cts?.Cancel();
                _worker?.Wait();
            } catch { }

            if (_capture != null) {
                try { _capture.StopRecording(); } catch { }
                _capture.Dispose();
                _capture = null;
            }
        }

        private void OnData(object? sender, WaveInEventArgs e) {
            if (Configuration.Instance.AudioHapticsActive) {
                // WASAPI loopback usually gives 32-bit float, stereo, but we guard anyway
                var format = _capture!.WaveFormat;
                int channels = format.Channels; // expect 2
                int bytesPerSample = format.BitsPerSample / 8;
                int frameCount = e.BytesRecorded / (bytesPerSample * channels);

                if (channels < 2) return; // we need stereo for direction; you can adapt for mono

                // Convert to float[] (interleaved stereo)
                lock (_bufferLock) {
                    EnsureRingCapacity(_ringCount + frameCount * channels);

                    if (format.Encoding == WaveFormatEncoding.IeeeFloat && bytesPerSample == 4) {
                        // Fast path: copy directly
                        int bytesToCopy = e.BytesRecorded;
                        int maxBytesAvailable = (_ring.Length - _ringWrite) * sizeof(float);

                        if (bytesToCopy > maxBytesAvailable) {
                            bytesToCopy = maxBytesAvailable;
                        }

                        Buffer.BlockCopy(e.Buffer, 0, _ring, _ringWrite * sizeof(float), bytesToCopy);

                        int samplesCopied = bytesToCopy / sizeof(float);

                        _ringWrite = (_ringWrite + samplesCopied) % _ring.Length;
                        _ringCount = Math.Min(_ringCount + samplesCopied, _ring.Length);
                    } else if (format.Encoding == WaveFormatEncoding.Pcm && bytesPerSample == 2) {
                        // 16-bit PCM → float
                        int offset = 0;
                        for (int i = 0; i < frameCount; i++) {
                            short s0 = BitConverter.ToInt16(e.Buffer, offset); offset += 2;
                            short s1 = BitConverter.ToInt16(e.Buffer, offset); offset += 2;
                            float l = s0 / 32768f;
                            float r = s1 / 32768f;
                            WriteToRing(l);
                            WriteToRing(r);
                        }
                    } else {
                        // Unsupported format
                    }
                }
            }
        }

        private void WorkerLoop(CancellationToken token) {
            float[] left = new float[_fftSize];
            float[] right = new float[_fftSize];
            float[] window = HannWindow(_fftSize);
            Complex[] fft = new Complex[_fftSize];

            var sampleRate = _capture!.WaveFormat.SampleRate;
            int binsLowMax = (int)Math.Floor(_lowBandMaxHz * _fftSize / (double)sampleRate);
            int binsHighMin = (int)Math.Ceiling(_highBandMinHz * _fftSize / (double)sampleRate);

            // Transient detection state
            double prevMonoRms = 0;
            double transientThreshold = 0.25; // Adjust to sensitivity

            while (!token.IsCancellationRequested) {
                if (!TryDequeueFrame(left, right)) {
                    Thread.Sleep(5);
                    continue;
                }

                // Windowing
                for (int i = 0; i < _fftSize; i++) {
                    left[i] *= window[i];
                    right[i] *= window[i];
                }

                // Mix to mono
                float[] mono = new float[_fftSize];
                for (int i = 0; i < _fftSize; i++) mono[i] = 0.5f * (left[i] + right[i]);

                // FFT spectral energy outside speech band
                for (int i = 0; i < _fftSize; i++) {
                    fft[i].X = mono[i];
                    fft[i].Y = 0f;
                }
                FastFourierTransform.FFT(true, _fftPower, fft);

                double lowSum = 0, highSum = 0;
                for (int bin = 1; bin <= binsLowMax && bin < _fftSize / 2; bin++) {
                    double re = fft[bin].X; double im = fft[bin].Y;
                    lowSum += re * re + im * im;
                }
                for (int bin = Math.Max(binsHighMin, 1); bin < _fftSize / 2; bin++) {
                    double re = fft[bin].X; double im = fft[bin].Y;
                    highSum += re * re + im * im;
                }
                double bandPower = lowSum + highSum;
                double db = 10.0 * Math.Log10(Math.Max(bandPower, 1e-12));

                // RMS for stereo
                double lRms = Rms(left);
                double rRms = Rms(right);
                double lDb = 20.0 * Math.Log10(Math.Max(lRms, 1e-9));
                double rDb = 20.0 * Math.Log10(Math.Max(rRms, 1e-9));

                HapticSide side = HapticSide.Both;
                if (lDb - rDb > _stereoDbBias) side = HapticSide.Left;
                else if (rDb - lDb > _stereoDbBias) side = HapticSide.Right;

                // Transient detection
                double monoRms = Rms(mono);
                bool isTransient = (monoRms - prevMonoRms) > transientThreshold;
                prevMonoRms = monoRms;

                bool trigger = db >= _onThresholdDb || db >= _onThresholdDb && isTransient;
                float intensity = NormalizeIntensity((float)db);
                // Hysteresis and transient handling
                if (!_isOn && trigger) {
                    _isOn = true;
                    _lastSide = side;
                    _haptics.SetHaptics(side, intensity);
                } else if (_isOn && db <= _offThresholdDb) {
                    _isOn = false;
                    _haptics.SetHaptics(_lastSide, intensity);
                } else if (_isOn && side != _lastSide) {
                    _lastSide = side;
                    _haptics.SetHaptics(side, intensity);
                }
            }
        }

        private bool TryDequeueFrame(float[] left, float[] right) {
            lock (_bufferLock) {
                int needed = _fftSize * 2; // stereo interleaved samples
                if (_ringCount < needed) return false;

                int readPos = (_ringWrite - _ringCount + _ring.Length) % _ring.Length;

                for (int i = 0; i < _fftSize; i++) {
                    // L
                    left[i] = _ring[readPos];
                    readPos = (readPos + 1) % _ring.Length;
                    // R
                    right[i] = _ring[readPos];
                    readPos = (readPos + 1) % _ring.Length;
                }

                _ringCount -= needed;
                return true;
            }
        }

        private void EnsureRingCapacity(int minSamples) {
            if (_ring.Length >= minSamples) return;
            int newSize = _ring.Length;
            while (newSize < minSamples) newSize *= 2;
            var newBuf = new float[newSize];

            // compact existing data
            int readPos = (_ringWrite - _ringCount + _ring.Length) % _ring.Length;
            for (int i = 0; i < _ringCount; i++) {
                newBuf[i] = _ring[readPos];
                readPos = (readPos + 1) % _ring.Length;
            }
            _ring = newBuf;
            _ringWrite = _ringCount;
        }

        private void WriteToRing(float sample) {
            _ring[_ringWrite] = sample;
            _ringWrite = (_ringWrite + 1) % _ring.Length;
            if (_ringCount < _ring.Length) _ringCount++;
            else // overwrite oldest
                ;
        }

        private static float[] HannWindow(int n) {
            var w = new float[n];
            for (int i = 0; i < n; i++) {
                w[i] = 0.5f * (1f - (float)Math.Cos((2.0 * Math.PI * i) / (n - 1)));
            }
            return w;
        }

        private static double Rms(float[] x) {
            double sum = 0;
            for (int i = 0; i < x.Length; i++) sum += x[i] * x[i];
            return Math.Sqrt(sum / Math.Max(1, x.Length));
        }

        public void Dispose() => Stop();
    }
}
