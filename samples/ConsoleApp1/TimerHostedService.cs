using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public interface ITimerHostedService
    {
        Task Execute();
    }

    public class TimerHostedService : ITimerHostedService, IHostedService, IDisposable
    {
        private Timer _timer;
        private readonly IServiceProvider services;
        private readonly ILogger<TimerHostedService> logger;
        private readonly IOptions<ConnectionStrings> config;

        public TimerHostedService(IServiceProvider services, ILogger<TimerHostedService> logger, IOptions<ConnectionStrings> config)
        {
            this.services = services;
            this.logger = logger;
            this.config = config;
        }

        public async Task Execute()
        {
            logger.LogInformation(config.Value.DefaultConnection);

            using (var scope = services.CreateScope())
            {
                // resolve service w/DI
                var fuga = scope.ServiceProvider.GetRequiredService<IFugaService>();
                await fuga.Execute();
            }
        }

        private void DoWork(object state)
        {
            logger.LogInformation("Timed Background Service is working.");
        }

        /// <summary>
        /// Auto called via HostedService
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(DoWork, null, TimeSpan.Zero,TimeSpan.FromSeconds(5));

            logger.LogInformation("start");
            await Execute();
        }

        /// <summary>
        /// Auto called via HostedService
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("stop");
            _timer?.Change(Timeout.Infinite, 0);
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
