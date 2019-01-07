using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace Virtual_Port
{
    public class CmdOptions
    {
        // Required
        [Option('s', "server", Required = true, Default = true,
            HelpText = "Virtual Port Server")]
        public bool Server {get; set;}
        
        [Option('d', "device", Required = true,
            HelpText = "Serial Port Device")]
        public string PortName { get; set; }
        
        [Option('p', "port", Required = true, Default = 9001,
            HelpText = "Server TCP Port Number")]
        public int ServerTCPPort { get; set; }

        [Option('v', "verbose", Required = false, Default = false,
            HelpText = "Verbose")]
        public bool Verbose { get; set; }

    }
}
