using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ChampSelectHelperApp
{
    public static class ProcessHandler
    {
        private static Regex AUTH_TOKEN_REGEX = new Regex("\"--remoting-auth-token=(.+?)\"");
        private static Regex PORT_REGEX = new Regex("\"--app-port=(\\d+?)\"");
    }
}
