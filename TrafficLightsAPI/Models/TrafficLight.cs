using TrafficLightsAPI.Enums;

namespace TrafficLightsAPI.Models
{
    public class TrafficLight
    {
        public string Direction { get; set; }
        public LightState CurrentState { get; set; }
        public string CurrentStateColor => CurrentState.ToString();
        public int GreenDuration { get; set; }
        public int YellowDuration { get; set; } = 5;
        public DateTime LastTransitionTime { get; set; }  // The last state transition
        public bool IsRightTurnActive { get; set; } = false;  // Check the right-turn signal
        public int GroupId { get; set; }  // 1 for North-South, 2 for East-West
    }
}
