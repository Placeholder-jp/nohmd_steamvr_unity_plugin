using System;

namespace Litpla.VR.Util
{
    /// <summary>
    /// SteamVRアプリケーション 仮想HMDドライバ設定
    /// </summary>
    [Serializable]
    public class default_vrsettings
    {
        public DriverNull driver_null = new DriverNull();

        [Serializable]
        public class DriverNull
        {
            public double displayFrequency;
            public bool enable;
            public string modelNumber;
            public int renderHeight;
            public int renderWidth;
            public double secondsFromVsyncToPhotons;
            public string serialNumber;
            public int windowHeight;
            public int windowWidth;
            public int windowX;
            public int windowY;
        }
    }
}