using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Msk
{
    public class Config
    {
        #region Master

        public bool IsHostDNS { get; set; } = false;

        public string MasterHost { get; set; } = "0.0.0.0";

        public ushort MasterPort { get; set; } = 20000;

        public int MaxConnectionsToMaster { get; set; } = 200;

        public int DispatchRate { get; set; } = 15;

        #endregion

        #region Spawner

        public ushort PortStart { get; set; } = 25000;

        public int MaxInstanceCount { get; set; } = 100;

        public bool UseVersionInInstancePath { get; set; } = true;

        public string ServerInstancePath { get; set; } = "";

        public string InstanceFileName { get; set; } = "";

        #endregion

    }
}
