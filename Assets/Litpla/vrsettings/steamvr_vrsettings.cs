using System;

namespace Litpla.VR.Util
{
    [Serializable]
    public class steamvr_vrsettings
    {
        public Dashboard dashboard;
        public Steamvr steamvr;
        public Userinterface userinterface;

        [Serializable]
        public class Dashboard
        {
            public bool enableDashboard;
        }

        [Serializable]
        public class Steamvr
        {
            public bool activateMultipleDrivers;
            public bool allowAsyncReprojection;
            public bool allowInterleavedReprojection;
            public bool allowSupersampleFiltering;
            public bool enableHomeApp;
            public string forcedDriver;
            public string mirrorViewGeometry;
            public double supersampleScale;
        }

        [Serializable]
        public class Userinterface
        {
            public bool MinimizeToTray;
            public bool StatusAlwaysOnTop;
        }
    }
}