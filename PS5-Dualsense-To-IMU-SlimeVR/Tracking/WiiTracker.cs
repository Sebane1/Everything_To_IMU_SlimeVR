using Newtonsoft.Json.Linq;
using SlimeImuProtocol.SlimeVR;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Numerics;
using SlimeImuProtocol.Utility;
using static Everything_To_IMU_SlimeVR.TrackerConfig;
using static Everything_To_IMU_SlimeVR.Tracking.ForwardedWiimoteManager;
using System.Threading.Tasks;

namespace Everything_To_IMU_SlimeVR.Tracking {
	public class WiiTracker : IDisposable, IBodyTracker {
		private string _debug;
		private int _index;
		private string _wiimoteClient;
		private int _id;
		private string _wiimoteId;
		private string _firmwareId;
		private bool _nunchuck;
		private ConcurrentDictionary<string, WiimoteInfo> _motionStateList;
		private string macSpoof;
		private byte[] _macAddressBytes;
		private UDPHandler udpHandler;
		private Vector3 _wiimoteRotationCalibration;
		private Vector3 _nunchuckRotationCalibration;
		private float _calibratedHeight;
		private bool _ready;
		private bool _disconnected;
		private string _lastDualSenseId;
		private bool _useWaistTrackerForYaw;
		private FalseThighTracker _falseThighTracker;
		private float _lastEulerPositon;
		private Vector3 _acceleration;
		private bool _waitForRelease;
		private string _rememberedStringId;
		private RotationReferenceType _yawReferenceTypeValue = RotationReferenceType.WaistRotation;
		private RotationReferenceType _extensionYawReferenceTypeValue = RotationReferenceType.WaistRotation;
		Stopwatch buttonPressTimer = new Stopwatch();
		WiiTracker _connectedWiimote;
		private HapticNodeBinding _hapticNodeBinding;
		private bool isAlreadyVibrating;
		private bool identifying;
		private bool _isAlreadyUpdating;
		private float _minAccelDelta;
		private bool _hasTrackedFirstAccel;
		private Vector3Short _previousAccelValue;
		private Vector3Short _previousWiimoteAccelValue;
		private Vector3Short _previousNunchuckAccelValue;
		private DateTime _hapticEndTime;

		public bool SupportsHaptics => true;
		public bool SupportsIMU => true;

		public event EventHandler<string> OnTrackerError;

		public WiiTracker(string id) {
			Initialize(id);
		}
		public async void Initialize(string id) {
			await Task.Run(async () => {
				try {
					var split = id.Split(":");
					_wiimoteClient = split[0];
					_id = int.Parse(split[1]);
					_wiimoteId = id;
					_rememberedStringId = _wiimoteClient + ":" + _id.ToString();
					macSpoof = HashUtility.CalculateMD5Hash(_rememberedStringId + "Wiimote_Tracker");
					_macAddressBytes = new byte[] { (byte)macSpoof[0], (byte)macSpoof[1], (byte)macSpoof[2], (byte)macSpoof[3], (byte)macSpoof[4], (byte)macSpoof[5] };
					_firmwareId = "Wiimote_Tracker" + _rememberedStringId;
					_motionStateList = ForwardedWiimoteManager.Wiimotes;
					udpHandler = new UDPHandler(_firmwareId, _macAddressBytes,
				 FirmwareConstants.BoardType.UNKNOWN, FirmwareConstants.ImuType.UNKNOWN, FirmwareConstants.McuType.UNKNOWN, FirmwareConstants.MagnetometerStatus.NOT_SUPPORTED, 2);
					udpHandler.Active = true;
					Recalibrate();
					ForwardedWiimoteManager.NewPacketReceived += NewPacketReceived;
					_ready = true;
				} catch (Exception e) {
					OnTrackerError?.Invoke(this, e.Message);
				}
			});
		}

		private async void NewPacketReceived(object reference, string ip) {
			if (_wiimoteClient == ip) {
				await Update();
			}
		}

		public async Task<bool> Update() {
			var accelerationMultiplier = 1f;
			var accelerationNunchuckMultiplier = 1f;
			if (_ready) {
				try {
					var value = _motionStateList[_wiimoteId];
					if (value.ButtonUp) {
						if (!_waitForRelease) {
							_waitForRelease = true;
							//udpHandler.SendButton();
						}
					} else if (!_isAlreadyUpdating) {
						_isAlreadyUpdating = true;
						var hmdHeight = OpenVRReader.GetHMDHeight();
						bool isClamped = !_falseThighTracker.IsClamped;
						var trackerRotation = OpenVRReader.GetTrackerRotation(YawReferenceTypeValue);
						float trackerEuler = trackerRotation.GetYawFromQuaternion();

						_lastEulerPositon = -trackerEuler;
						if (_waitForRelease) {
							_waitForRelease = false;
						}
						var wiimoteRotation = value.MotionPlusSupport != 0 ? value.WiimoteFusedOrientation : value.WiimoteGravityOrientation;
						var eulerUncalibrated = wiimoteRotation.QuaternionToEuler();
						var euler = eulerUncalibrated;
						if (GenericTrackerManager.DebugOpen) {
							_debug =
							$"Device Id: {macSpoof}\r\n" +
							$"Euler Rotation Uncalibrated:\r\n" +
							$"X:{eulerUncalibrated.X}, Y:{eulerUncalibrated.Y}, Z:{eulerUncalibrated.Z}" +
							$"\r\nEuler Rotation:\r\n" +
							$"X:{euler.X}, Y:{euler.Y}, Z:{euler.Z}" +
							$"\r\nAcceleration:\r\n" +
							$"X:{value.WiimoteAccelX}, Y:{value.WiimoteAccelY}, Z:{value.WiimoteAccelZ}\r\n" +
							$"Gyro:\r\n" +
							(value.MotionPlusSupport != 0 ? $"X:{value.WiimoteGyroX}, Y:{value.WiimoteGyroY}, Z:{value.WiimoteGyroZ}\r\n" : "No Motion Plus Support\r\n") +
							$"Yaw Reference Rotation:\r\n" +
							$"Y:{trackerEuler}\r\n"
							+ _falseThighTracker.Debug;
						}
						float finalX = -euler.X;
						float finalY = euler.Y;
						float finalZ = 0;

						await udpHandler.SetSensorBattery(Math.Clamp(value.BatteryLevel / 100f, 0f, 1f), 3.7f);
						var shortVector = new Vector3Short(value.WiimoteAccelZ, value.WiimoteAccelY, value.WiimoteAccelZ);
						await udpHandler.SetSensorAcceleration(new Vector3(
	(value.WiimoteAccelX / 512f) * accelerationMultiplier,
	(value.WiimoteAccelY / 512f) * accelerationMultiplier,
	(value.WiimoteAccelZ / 512f) * accelerationMultiplier), 0);
						//if (HasSignificantAccelChange(shortVector, _previousWiimoteAccelValue, 5f)) {
						if (_yawReferenceTypeValue == RotationReferenceType.TrustDeviceYaw) {
							await udpHandler.SetSensorRotation(wiimoteRotation, 0);
						} else {
							await udpHandler.SetSensorRotation(new Vector3(finalX, finalY, _lastEulerPositon).ToQuaternion(), 0);
						}
						_previousWiimoteAccelValue = shortVector;
						//}

						if (value.NunchukConnected != 0) {
							if (YawReferenceTypeValue != ExtensionYawReferenceTypeValue) {
								trackerRotation = OpenVRReader.GetTrackerRotation(ExtensionYawReferenceTypeValue);
								trackerEuler = trackerRotation.GetYawFromQuaternion();
								_lastEulerPositon = -trackerEuler;
							}
							var nunchuckRotation = value.NunchuckOrientation;
							eulerUncalibrated = nunchuckRotation.QuaternionToEuler();
							euler = eulerUncalibrated;
							if (GenericTrackerManager.DebugOpen) {
								_debug +=
								$"\r\n\r\nNunchuck" +
								$"Euler Calibration Rotation Offset:\r\n" +
								$"X:{_nunchuckRotationCalibration.X}, Y:{_nunchuckRotationCalibration.Y}, Z:{_nunchuckRotationCalibration.Z}\r\n" +
								$"Euler Rotation Uncalibrated:\r\n" +
								$"X:{eulerUncalibrated.X}, Y:{eulerUncalibrated.Y}, Z:{eulerUncalibrated.Z}" +
								$"\r\nEuler Rotation:\r\n" +
								$"X:{euler.X}, Y:{euler.Y}, Z:{euler.Z}" +
								$"\r\nAcceleration:\r\n" +
								$"X:{value.NunchukAccelX}, Y:{value.NunchukAccelY}, Z:{value.NunchukAccelZ}\r\n" +
								$"Yaw Reference Rotation:\r\n" +
								$"Y:{trackerEuler}\r\n";
							}
							finalX = euler.X;
							shortVector = new Vector3Short(value.NunchukAccelX, value.NunchukAccelY, value.NunchukAccelZ);
							await udpHandler.SetSensorAcceleration(new Vector3(
(value.NunchukAccelX / 512f) * accelerationNunchuckMultiplier,
(value.NunchukAccelY / 512f) * accelerationNunchuckMultiplier,
(value.NunchukAccelZ / 512f) * accelerationNunchuckMultiplier), 0);
							if (HasSignificantAccelChange(shortVector, _previousNunchuckAccelValue, 1f)) {
								await udpHandler.SetSensorRotation(new Vector3(finalX, -euler.Y, _lastEulerPositon).ToQuaternion(), 1);
								_previousNunchuckAccelValue = shortVector;
							}
						}
						_isAlreadyUpdating = false;
					}
				} catch (Exception e) {
					OnTrackerError?.Invoke(this, e.StackTrace + "\r\n" + e.Message);
				}
			}

			return _ready;
		}

		public async void Recalibrate() {

			_calibratedHeight = OpenVRReader.GetHMDHeight();
			var value = _motionStateList.ElementAt(_index);
			var rotation = value.Value.WiimoteGravityOrientation;
			_wiimoteRotationCalibration = -rotation.QuaternionToEuler();
			RotationCalibration = _wiimoteRotationCalibration;

			rotation = value.Value.NunchuckOrientation;
			_nunchuckRotationCalibration = -rotation.QuaternionToEuler();
			ForwardedWiimoteManager.WiimoteTrackers[_rememberedStringId].StartCalibration();
			await Task.Delay(3000);
			await udpHandler.SendButton(FirmwareConstants.UserActionType.RESET_FULL);
		}
		public void Rediscover() {
			udpHandler.Initialize(FirmwareConstants.BoardType.UNKNOWN, FirmwareConstants.ImuType.UNKNOWN, FirmwareConstants.McuType.UNKNOWN, FirmwareConstants.MagnetometerStatus.NOT_SUPPORTED, _macAddressBytes);
		}

		public void Dispose() {
			_ready = false;
			_disconnected = true;
			_falseThighTracker?.Dispose();
		}

		public Vector3 GetCalibration() {
			return new Vector3();
		}

		public void Identify() {
			identifying = true;
			EngageHaptics(1000, 100);
			identifying = false;
		}

		private bool HasSignificantAccelChange(Vector3Short current, Vector3Short previous, float minAccelDelta) {
			if (_hasTrackedFirstAccel) {
				float delta = Vector3Short.Distance(current, previous);
				return delta > minAccelDelta;
			}
			_hasTrackedFirstAccel = true;
			return true; // Always accept if no previous data
		}
		public void EngageHaptics(int duration, float intensity) {
			_hapticEndTime = DateTime.Now.AddMilliseconds(duration);
			if (!isAlreadyVibrating) {
				isAlreadyVibrating = true;
				Task.Run(() => {
					ForwardedWiimoteManager.RumbleState[_wiimoteClient][_id] = 1;
					while (DateTime.Now < _hapticEndTime) {
						Thread.Sleep(10);
					}
					ForwardedWiimoteManager.RumbleState[_wiimoteClient][_id] = 0;
					isAlreadyVibrating = false;
					identifying = false;
				});
			}
		}
		public void DisableHaptics() {
			if (!identifying) {
				isAlreadyVibrating = false;
				EngageHaptics(Configuration.Instance.WiiPollingRate, 100);
			}
		}
		public override string ToString() {
			return _rememberedStringId;
		}

        public void HapticIntensityTest() {
           // throw new NotImplementedException();
        }

        public string Debug { get => _debug; set => _debug = value; }
		public bool Ready { get => _ready; set => _ready = value; }
		public bool Disconnected { get => _disconnected; set => _disconnected = value; }
		public int Id { get => _id; set => _id = value; }
		public string MacSpoof { get => macSpoof; set => macSpoof = value; }
		public Vector3 Euler { get; set; }
		public Vector3 Gyro { get; set; }
		public Vector3 Acceleration { get => _acceleration; set => _acceleration = value; }
		public float LastHmdPositon { get => _lastEulerPositon; set => _lastEulerPositon = value; }
		public bool UseWaistTrackerForYaw { get => _useWaistTrackerForYaw; set => _useWaistTrackerForYaw = value; }
		public RotationReferenceType YawReferenceTypeValue { get => _yawReferenceTypeValue; set => _yawReferenceTypeValue = value; }
		public HapticNodeBinding HapticNodeBinding { get => _hapticNodeBinding; set => _hapticNodeBinding = value; }
		public RotationReferenceType ExtensionYawReferenceTypeValue { get => _extensionYawReferenceTypeValue; set => _extensionYawReferenceTypeValue = value; }
		public Vector3 RotationCalibration { get; set; }
	}
}