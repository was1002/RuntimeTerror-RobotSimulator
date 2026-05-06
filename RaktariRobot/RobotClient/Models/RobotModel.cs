using RobotShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuntimeTerror.RobotClient.Models
{
    public class RobotModel
    {
        public string Id { get; set; } = string.Empty;
        public double X { get; set; }
        public double Y { get; set; }
        public int Battery { get; set; }
        public RobotState State { get; set; } = RobotState.Idle;
    }
}
