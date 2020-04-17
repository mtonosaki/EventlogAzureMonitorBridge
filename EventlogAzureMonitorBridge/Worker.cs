// (c) 2020 Manabu Tonosaki
// Licensed under the MIT license.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tono;

namespace EventlogAzureMonitorBridge
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private IConfiguration _config;


        public Worker(ILogger<Worker> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
#if DEBUG
            await Task.Delay(1, stoppingToken);
#else
            await Task.Delay(5531, stoppingToken);
#endif
            var listener = new EventlogListener
            {
                Messages = new Queue<EventlogMessage>(),
                Logger = _logger,
            };

            var config = _config.GetSection("LogAnalytics");

            var uploader = new AzureUploader
            {
                Messages = listener.Messages,
                Logger = _logger,
                WorkspaceID = config["PathLog"],
                Key1 = config["PathLog"],
                LogName = config["LogName"],
            };

            while (!stoppingToken.IsCancellationRequested)
            {
                var w1 = await listener.ListenAsync(stoppingToken);
                var w2 = await uploader.UploadAsync(stoppingToken);

                await Task.Delay(Math.Min(w1, w2), stoppingToken);
            }
        }
    }
}
