using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PS5_Dualsense_To_IMU_SlimeVR {
    public class TrackerConfig {
        bool _simulatesThighs;

        public bool SimulatesThighs { get => _simulatesThighs; set => _simulatesThighs = value; }
    }
}

