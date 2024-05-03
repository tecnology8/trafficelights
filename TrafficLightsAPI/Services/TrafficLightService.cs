using TrafficLightsAPI.Enums;
using TrafficLightsAPI.Models;

namespace TrafficLightsAPI.Services
{
    public class TrafficLightService
    {
        private List<TrafficLight> _lights;

        public TrafficLightService()
        {
            var currentDateTime = DateTime.Now;
            _lights = new List<TrafficLight>
            {
                new TrafficLight { Direction = "North", GreenDuration = 20, GroupId = 1, CurrentState = LightState.Green, LastTransitionTime = currentDateTime },
                new TrafficLight { Direction = "South", GreenDuration = 20, GroupId = 1, CurrentState = LightState.Green, LastTransitionTime = currentDateTime },
                new TrafficLight { Direction = "East", GreenDuration = 20, GroupId = 2, CurrentState = LightState.Red, LastTransitionTime = currentDateTime },
                new TrafficLight { Direction = "West", GreenDuration = 20, GroupId = 2, CurrentState = LightState.Red, LastTransitionTime = currentDateTime }
            };
        }
        public List<TrafficLight> RetrieveLights() => _lights;
        public void UpdateLights()
        {
            lock (_lights)
            {
                DateTime currentTime = DateTime.Now;
                bool isPeakHours = IsPeakHours(currentTime);

                AdjustSouthboundForNorthRightTurn(currentTime);

                foreach (var group in _lights.GroupBy(l => l.GroupId))
                {
                    bool shouldSwitchToYellow = group.Any(l => l.CurrentState == LightState.Green && ShouldSwitchFromGreen((currentTime - l.LastTransitionTime).TotalSeconds, isPeakHours, l.Direction));
                    bool shouldSwitchToRed = group.Any(l => l.CurrentState == LightState.Yellow && (currentTime - l.LastTransitionTime).TotalSeconds >= 5);

                    if (shouldSwitchToYellow)
                    {
                        foreach (var light in group)
                        {
                            if (light.CurrentState == LightState.Red)
                            {
                                break;
                            }
                            light.CurrentState = LightState.Yellow;
                            light.LastTransitionTime = currentTime;
                            if (light.Direction == "North")
                            {
                                light.IsRightTurnActive = false;
                            }
                        }
                    }
                    else if (shouldSwitchToRed)
                    {
                        foreach (var light in group)
                        {
                            light.CurrentState = LightState.Red;
                            light.LastTransitionTime = currentTime;
                        }
                        SetOppositeGroupToGreen(group.Key);
                    }
                }
            }
        }

        #region Private Methods

        private void SetOppositeGroupToGreen(int groupId)
        {
            int oppositeGroupId = groupId == 1 ? 2 : 1;
            foreach (var light in _lights.Where(l => l.GroupId == oppositeGroupId))
            {
                light.CurrentState = LightState.Green;
                light.LastTransitionTime = DateTime.Now;
            }
        }

        private bool IsPeakHours(DateTime time)
        {
            return (time.Hour >= 8 && time.Hour < 10) || (time.Hour >= 17 && time.Hour < 19);
        }

        private bool ShouldSwitchFromGreen(double elapsedSeconds, bool isPeakHours, string direction)
        {
            int requiredSeconds = direction == "North" || direction == "South" ?
                                  isPeakHours ? 40 : 20 :
                                  isPeakHours ? 10 : 20;
            return elapsedSeconds >= requiredSeconds;
        }


        private void AdjustSouthboundForNorthRightTurn(DateTime currentTime)
        {
            bool isPeakHours = IsPeakHours(currentTime);
            var northLight = _lights.Single(l => l.Direction == "North");

            if (northLight.CurrentState == LightState.Green && !northLight.IsRightTurnActive && ShouldActivateRightTurn((currentTime - northLight.LastTransitionTime).TotalSeconds, isPeakHours))
            {
                northLight.IsRightTurnActive = true;
                foreach (var light in _lights.Where(l => l.Direction != "North"))
                {
                    if (light.CurrentState != LightState.Red)
                    {
                        light.CurrentState = LightState.Red;
                        light.LastTransitionTime = currentTime;
                    }
                }

            }
        }

        private bool ShouldActivateRightTurn(double elapsedSeconds, bool isPeakHours)
        {
            // Activate right-turn signal for the last 10 seconds of the green phase
            int greenDuration = isPeakHours ? 40 : 20;

            return elapsedSeconds >= (greenDuration - 10);
        }

        #endregion
    }
}
