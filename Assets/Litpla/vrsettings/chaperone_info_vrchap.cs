using System;
using System.Collections.Generic;

namespace Litpla.VR.Util
{
    [Serializable]
    public class chaperone_info_vrchap
    {
        public string jsonid;
        public List<Universe> universes;
        public int version;

        [Serializable]
        public class Standing
        {
            public List<double> translation;
            public double yaw;
        }

        [Serializable]
        public class Universe
        {
            public List<List<List<double>>> collision_bounds;
            public List<int> play_area;
            public Standing standing;
            public string time;
            public string universeID;
        }
    }
}