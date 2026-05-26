using Microsoft.Maui.Controls;
using RobotShared;
using RuntimeTerror.Client.Models; // Need to include models for ObservableRobot

namespace RuntimeTerror.Client.Selectors
{
    public class RobotStateTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? NormalTemplate { get; set; }
        public DataTemplate? WarningTemplate { get; set; }
        public DataTemplate? ErrorTemplate { get; set; }

        protected override DataTemplate? OnSelectTemplate(object item, BindableObject container)
        {
            if (item is ObservableRobot robot) // Use ObservableRobot
            {
                if (robot.DiagnosticLevel == DiagnosticLevel.Error || robot.State == RobotState.Error)
                {
                    return ErrorTemplate;
                }
                
                if (robot.DiagnosticLevel == DiagnosticLevel.Warning || robot.DiagnosticLevel == DiagnosticLevel.CriticalWarning)
                {
                    return WarningTemplate;
                }
            }

            return NormalTemplate;
        }
    }
}
