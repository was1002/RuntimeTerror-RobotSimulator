using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuntimeTerror.RobotClient.Models
{
    public class ConnectionSettingsModel
    {
        public string ServerAddress { get; set; } = "http://localhost:5090/";
        public bool isConnected { get; set; }
    }
}
