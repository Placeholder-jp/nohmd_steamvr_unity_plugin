using System;

namespace Litpla.VR.Util
{
    /// <summary>
    /// SteamVRアプリケーション 一般設定
    /// </summary>
    [Serializable]
    public class steamvr_vrsettings
    {
        public Dashboard dashboard = new Dashboard();
        public Steamvr steamvr = new Steamvr();
        public Userinterface userinterface = new Userinterface();

        [Serializable]
        public class Dashboard
        {
            public bool enableDashboard = false;
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