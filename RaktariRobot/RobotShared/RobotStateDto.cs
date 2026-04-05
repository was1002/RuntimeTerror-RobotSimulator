namespace RobotShared
{
    public class RobotStateDto
    {
        public double X {  get; set; }
        public double Y { get; set; }
        public int Battery { get; set; }
        public string StateMessage { get; set; } = string.Empty;
    }
}
