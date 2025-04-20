using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChampSelectHelperApp
{
    class LCUEventArgs : EventArgs
    {
        public string Path { get; set; }
        public string Data { get; set; }
        public string EventType { get; set; }
    }
}
