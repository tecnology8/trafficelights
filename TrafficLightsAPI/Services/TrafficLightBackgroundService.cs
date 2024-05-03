namespace TrafficLightsAPI.Services
{
    public class TrafficLightBackgroundService  : BackgroundService
    {
        private readonly TrafficLightService _trafficLightService;
        private Timer _timer;
        public TrafficLightBackgroundService(TrafficLightService trafficLightService)
        {
            _trafficLightService = trafficLightService;
        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _timer = new Timer(UpdateTrafficLights, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));

            return Task.CompletedTask;
        }
        private void UpdateTrafficLights(object state)
        {
            _trafficLightService.UpdateLights();
        }
        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            await base.StopAsync(stoppingToken);
        }
    }
}
