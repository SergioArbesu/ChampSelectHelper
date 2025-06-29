using System;

namespace ChampSelectHelper
{
    class LCUEventArgs : EventArgs
    {
        public string Path { get; set; }
        public string? Data { get; set; }
        public string EventType { get; set; }
    }
}
