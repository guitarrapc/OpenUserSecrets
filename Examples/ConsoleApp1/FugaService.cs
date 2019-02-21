using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public interface IFugaService
    {
        Task Execute();
    }

    public class FugaService : IFugaService
    {
        private readonly ILogger<TimerHostedService> logger;
        private readonly IOptions<ConnectionStrings> config;

        // resolve with DI
        public FugaService(ILogger<TimerHostedService> logger, IOptions<ConnectionStrings> config)
        {
            this.logger = logger;
            this.config = config;
        }

        public async Task Execute()
        {
            logger.LogInformation($"fuga : {config.Value.DefaultConnection}");
            await Task.Delay(TimeSpan.FromSeconds(1));
        }
    }
}
