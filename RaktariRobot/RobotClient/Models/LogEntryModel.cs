using RobotShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuntimeTerror.RobotClient.Models
{
    public class LogEntryModel
    {
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string Message { get; set; } = string.Empty;
        public DiagnosticLevel Level { get; set; } = DiagnosticLevel.Normal;
    }
}
