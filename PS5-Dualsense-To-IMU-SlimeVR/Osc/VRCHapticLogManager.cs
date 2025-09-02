using System.Runtime.InteropServices;
namespace Everything_To_IMU_SlimeVR.Osc {

    public static class ExplorerFolderRefresher {
        // Flags for SHChangeNotify
        private const uint SHCNE_UPDATEDIR = 0x00001000;
        private const uint SHCNF_PATHW = 0x0005;

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern void SHChangeNotify(uint wEventId, uint uFlags, IntPtr dwItem1, IntPtr dwItem2);

        /// <summary>
        /// Programmatically notifies the shell that a folder has changed,
        /// causing Explorer windows to refresh that folder view.
        /// </summary>
        /// <param name="folderPath">Full path of the folder to refresh.</param>
        public static void RefreshFolder(string folderPath) {
            if (string.IsNullOrEmpty(folderPath))
                throw new ArgumentException("folderPath cannot be null or empty.", nameof(folderPath));

            // Allocate unmanaged memory for the folder path string (Unicode)
            IntPtr ptr = Marshal.StringToHGlobalUni(folderPath);
            try {
                SHChangeNotify(SHCNE_UPDATEDIR, SHCNF_PATHW, ptr, IntPtr.Zero);
            } finally {
                Marshal.FreeHGlobal(ptr);
            }
        }
    }

    public class VRCHapticLogManager {
        private readonly string _localLowPath;
        private readonly string _vrcPath;
        private FileSystemWatcher _watcher;

        private CancellationTokenSource? _cts;
        private Task? _tailTask;
        private string? _currentLogFile;
        Dictionary<string, bool> loopingPatterns = new Dictionary<string, bool>();
        public VRCHapticLogManager() {
            _localLowPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).Replace("Roaming", "LocalLow");
            _vrcPath = Path.Combine(_localLowPath, @"VRChat\VRChat");

            // Setup watcher to monitor new log files created
            _watcher = new FileSystemWatcher(_vrcPath, "output_log_*.txt") {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
                IncludeSubdirectories = false,
                EnableRaisingEvents = true,
            };
            _watcher.Created += OnNewLogFileCreated;

            // Start by tailing the newest existing log file
            _currentLogFile = FindNewestLogFile();
            if (_currentLogFile != null)
                StartTailingLog(_currentLogFile);
        }

        private string? FindNewestLogFile() {
            var files = Directory.GetFiles(_vrcPath, "output_log_*.txt");
            return files.OrderByDescending(File.GetLastWriteTimeUtc).FirstOrDefault();
        }

        private void OnNewLogFileCreated(object sender, FileSystemEventArgs e) {
            if (_currentLogFile != null && e.FullPath.Equals(_currentLogFile, StringComparison.OrdinalIgnoreCase))
                return; // same file, ignore

            SwitchToNewLogFile(e.FullPath);
        }

        private void SwitchToNewLogFile(string newFile) {
            // Cancel existing tail
            _cts?.Cancel();
            try {
                _tailTask?.Wait();
            } catch (AggregateException) { }

            _currentLogFile = newFile;
            StartTailingLog(newFile);
        }

        private void StartTailingLog(string file) {
            _cts = new CancellationTokenSource();
            _tailTask = Task.Run(() => TailLogFileLoop(file, _cts.Token));
        }

        private async Task TailLogFileLoop(string filePath, CancellationToken token) {
            try {
                using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var sr = new StreamReader(fs);

                // Seek to end to only get new lines from now on
                fs.Seek(0, SeekOrigin.End);

                while (!token.IsCancellationRequested) {
                    var line = await sr.ReadLineAsync();
                    if (line != null) {
                        ProcessLogLine(line);
                    } else {
                        await Task.Delay(50, token); // Wait briefly for new data
                    }
                }
            } catch (OperationCanceledException) { } catch (Exception ex) {
                Console.WriteLine($"Error tailing VRChat log file: {ex}");
            }
        }

        private void ProcessLogLine(string line) {
            try {
                string cleanedValue = line.Split("-")[1].Trim();
                var args = cleanedValue.Split(" ");

                if (args.Length < 3)
                    return;

                switch (args[0]) {
                    case "[Behaviour] ":
                        if (args[1] == "Requesting") {
                            HapticsManager.StopNodeVibrations();
                        }
                        break;
                    case "[bLog]":
                        switch (args[1]) {
                            case "Play":
                                CheckForHapticAndTrigger(args[2], 1, 300);
                                break;
                            case "PlayParam":
                                CheckForHapticAndTrigger("_chest", 1, 300);
                                break;
                            case "PlayLoop":
                                Task.Run(delegate {
                                    loopingPatterns[args[2]] = true;
                                    while (loopingPatterns[args[2]]) {
                                        CheckForHapticAndTrigger("_head", 1, 30);
                                        Thread.Sleep(40);
                                        CheckForHapticAndTrigger("_chest", 1, 30);
                                        CheckForHapticAndTrigger("_shoulder_r", 1, 30);
                                        CheckForHapticAndTrigger("_shoulder_l", 1, 30);
                                        Thread.Sleep(40);
                                        CheckForHapticAndTrigger("_arm_r", 1, 30);
                                        CheckForHapticAndTrigger("_arm_l", 1, 30);
                                        Thread.Sleep(40);
                                        CheckForHapticAndTrigger("_foot_l", 1, 30);
                                        CheckForHapticAndTrigger("_foot_r", 1, 30);
                                        Thread.Sleep(40);
                                    }
                                });
                                break;
                            case "Stop":
                                CheckForHapticAndStop(args[2]);
                                break;
                        }
                        break;
                }
            } catch {
                // ignore malformed lines
            }
        }
        private void CheckForHapticAndTrigger(string value, float intensity, int duration) {
            List<HapticEvent> events = new List<HapticEvent>();
            if (value.EndsWith("_head")) {
                events.Add(new HapticEvent(HapticNodeBinding.Head, intensity, duration));
            } else if (value.EndsWith("_allchest")) {
                events.Add(new HapticEvent(HapticNodeBinding.Chest, intensity, duration));
                events.Add(new HapticEvent(HapticNodeBinding.ChestAndHipsFront, intensity, duration));
                events.Add(new HapticEvent(HapticNodeBinding.ChestAndHipsBack, intensity, duration));
                events.Add(new HapticEvent(HapticNodeBinding.ChestFront, intensity, duration));
                events.Add(new HapticEvent(HapticNodeBinding.ChestBack, intensity, duration));
                events.Add(new HapticEvent(HapticNodeBinding.Hips, intensity, duration));
                events.Add(new HapticEvent(HapticNodeBinding.HipsFront, intensity, duration));
                events.Add(new HapticEvent(HapticNodeBinding.HipsBack, intensity, duration));
            } else if (value.EndsWith("_chest")) {
                events.Add(new HapticEvent(HapticNodeBinding.Chest, intensity, duration));
                events.Add(new HapticEvent(HapticNodeBinding.ChestAndHipsFront, intensity, duration));
                events.Add(new HapticEvent(HapticNodeBinding.ChestAndHipsBack, intensity, duration));
                events.Add(new HapticEvent(HapticNodeBinding.ChestFront, intensity, duration));
                events.Add(new HapticEvent(HapticNodeBinding.ChestBack, intensity, duration));
            } else if (value.EndsWith("_hips")) {
                events.Add(new HapticEvent(HapticNodeBinding.ChestAndHipsFront, intensity, duration));
                events.Add(new HapticEvent(HapticNodeBinding.ChestAndHipsBack, intensity, duration));
                events.Add(new HapticEvent(HapticNodeBinding.Hips, intensity, duration));
                events.Add(new HapticEvent(HapticNodeBinding.HipsFront, intensity, duration));
                events.Add(new HapticEvent(HapticNodeBinding.HipsBack, intensity, duration));
            } else if (value.EndsWith("_shoulder_r")) {
                events.Add(new HapticEvent(HapticNodeBinding.RightShoulder, intensity, duration));
            } else if (value.EndsWith("_shoulder_l")) {
                events.Add(new HapticEvent(HapticNodeBinding.LeftShoulder, intensity, duration));
            } else if (value.EndsWith("_arm_r")) {
                events.Add(new HapticEvent(HapticNodeBinding.RightUpperArm, intensity, duration));
                events.Add(new HapticEvent(HapticNodeBinding.RightForeArm, intensity, duration));
            } else if (value.EndsWith("_arm_l")) {
                events.Add(new HapticEvent(HapticNodeBinding.LeftUpperArm, intensity, duration));
                events.Add(new HapticEvent(HapticNodeBinding.LeftForeArm, intensity, duration));
            } else if (value.EndsWith("_thigh_r")) {
                events.Add(new HapticEvent(HapticNodeBinding.RightThigh, intensity, duration));
            } else if (value.EndsWith("_thigh_l")) {
                events.Add(new HapticEvent(HapticNodeBinding.LeftThigh, intensity, duration));
            } else if (value.EndsWith("_calf_r")) {
                events.Add(new HapticEvent(HapticNodeBinding.RightCalf, intensity, duration));
            } else if (value.EndsWith("_calf_l")) {
                events.Add(new HapticEvent(HapticNodeBinding.LeftCalf, intensity, duration));
            } else if (value.EndsWith("_foot_r")) {
                events.Add(new HapticEvent(HapticNodeBinding.RightThigh, intensity, duration));
                events.Add(new HapticEvent(HapticNodeBinding.RightCalf, intensity, duration));
                events.Add(new HapticEvent(HapticNodeBinding.RightFoot, intensity, duration));
            } else if (value.EndsWith("_foot_l")) {
                events.Add(new HapticEvent(HapticNodeBinding.LeftThigh, intensity, duration));
                events.Add(new HapticEvent(HapticNodeBinding.LeftCalf, intensity, duration));
                events.Add(new HapticEvent(HapticNodeBinding.LeftFoot, intensity, duration));
            } else if (value.EndsWith("_r")) {
                events.Add(new HapticEvent(HapticNodeBinding.RightHand, intensity, duration));
            } else if (value.EndsWith("_l")) {
                events.Add(new HapticEvent(HapticNodeBinding.LeftHand, intensity, duration));
            } else {
                var enumList = Enum.GetValues(typeof(HapticNodeBinding)).Cast<HapticNodeBinding>().ToList();
                foreach (var enumValue in enumList) {
                    events.Add(new HapticEvent(enumValue, intensity, duration));
                }
            }
            foreach (var eventItem in events) {
                HapticsManager.SetNodeVibration(eventItem.Node, duration, eventItem.Intensity);
            }
        }

        private void CheckForHapticAndStop(string value) {
            List<HapticEvent> events = new List<HapticEvent>();
            if (value.EndsWith("_head")) {
                events.Add(new HapticEvent(HapticNodeBinding.Head, 1));
            } else if (value.EndsWith("_allchest")) {
                events.Add(new HapticEvent(HapticNodeBinding.Chest, 1));
                events.Add(new HapticEvent(HapticNodeBinding.ChestAndHipsFront, 1));
                events.Add(new HapticEvent(HapticNodeBinding.ChestAndHipsBack, 1));
                events.Add(new HapticEvent(HapticNodeBinding.ChestFront, 1));
                events.Add(new HapticEvent(HapticNodeBinding.ChestBack, 1));
                events.Add(new HapticEvent(HapticNodeBinding.Hips, 1));
                events.Add(new HapticEvent(HapticNodeBinding.HipsFront, 1));
                events.Add(new HapticEvent(HapticNodeBinding.HipsBack, 1));
            } else if (value.EndsWith("_chest")) {
                events.Add(new HapticEvent(HapticNodeBinding.Chest, 1));
                events.Add(new HapticEvent(HapticNodeBinding.ChestAndHipsFront, 1));
                events.Add(new HapticEvent(HapticNodeBinding.ChestAndHipsBack, 1));
                events.Add(new HapticEvent(HapticNodeBinding.ChestFront, 1));
                events.Add(new HapticEvent(HapticNodeBinding.ChestBack, 1));
            } else if (value.EndsWith("_hips")) {
                events.Add(new HapticEvent(HapticNodeBinding.ChestAndHipsFront, 1));
                events.Add(new HapticEvent(HapticNodeBinding.ChestAndHipsBack, 1));
                events.Add(new HapticEvent(HapticNodeBinding.Hips, 1));
                events.Add(new HapticEvent(HapticNodeBinding.HipsFront, 1));
                events.Add(new HapticEvent(HapticNodeBinding.HipsBack, 1));
            } else if (value.EndsWith("_shoulder_r")) {
                events.Add(new HapticEvent(HapticNodeBinding.RightShoulder, 1));
            } else if (value.EndsWith("_shoulder_l")) {
                events.Add(new HapticEvent(HapticNodeBinding.LeftShoulder, 1));
            } else if (value.EndsWith("_arm_r")) {
                events.Add(new HapticEvent(HapticNodeBinding.RightUpperArm, 1));
                events.Add(new HapticEvent(HapticNodeBinding.RightForeArm, 1));
            } else if (value.EndsWith("_arm_l")) {
                events.Add(new HapticEvent(HapticNodeBinding.LeftUpperArm, 1));
                events.Add(new HapticEvent(HapticNodeBinding.LeftForeArm, 1));
            } else if (value.EndsWith("_thigh_r")) {
                events.Add(new HapticEvent(HapticNodeBinding.RightThigh, 1));
            } else if (value.EndsWith("_thigh_l")) {
                events.Add(new HapticEvent(HapticNodeBinding.LeftThigh, 1));
            } else if (value.EndsWith("_calf_r")) {
                events.Add(new HapticEvent(HapticNodeBinding.RightCalf, 1));
            } else if (value.EndsWith("_calf_l")) {
                events.Add(new HapticEvent(HapticNodeBinding.LeftCalf, 1));
            } else if (value.EndsWith("_foot_r")) {
                events.Add(new HapticEvent(HapticNodeBinding.RightThigh, 1));
                events.Add(new HapticEvent(HapticNodeBinding.RightCalf, 1));
                events.Add(new HapticEvent(HapticNodeBinding.RightFoot, 1));
            } else if (value.EndsWith("_foot_l")) {
                events.Add(new HapticEvent(HapticNodeBinding.LeftThigh, 1));
                events.Add(new HapticEvent(HapticNodeBinding.LeftCalf, 1));
                events.Add(new HapticEvent(HapticNodeBinding.LeftFoot, 1));
            } else if (value.EndsWith("_r")) {
                events.Add(new HapticEvent(HapticNodeBinding.RightHand, 1));
            } else if (value.EndsWith("_l")) {
                events.Add(new HapticEvent(HapticNodeBinding.LeftHand, 1));
            } else {
                var enumList = Enum.GetValues(typeof(HapticNodeBinding)).Cast<HapticNodeBinding>().ToList();
                foreach (var enumValue in enumList) {
                    events.Add(new HapticEvent(enumValue, 1));
                }
            }
            if (loopingPatterns.ContainsKey(value)) {
                if (loopingPatterns[value]) {
                    loopingPatterns[value] = false;
                }
            }
            //foreach (var eventItem in events) {
            //    HapticsManager.StopNodeVibration(eventItem.Node);
            //}
        }
    }
}
