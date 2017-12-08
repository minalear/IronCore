using System;

namespace IronCore.Utils
{
    public struct GameTime
    {
        public float FrameDelta
        {
            get { return (float)ElapsedTime.TotalSeconds; }
            set { ElapsedTime = TimeSpan.FromSeconds(value); }
        }

        public TimeSpan ElapsedTime { get; set; }
        public TimeSpan TotalTime { get; set; }
    }
}
