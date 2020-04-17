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
        private EventlogListener listener;
        private AzureUploader uploader;

        /// <summary>
        /// The constructor (called by framework)
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="config"></param>
        public Worker(ILogger<Worker> logger, IConfiguration config)
        {
            listener = new EventlogListener
            {
                Messages = new Queue<EventlogMessage>(),
                Logger = logger,
            };

            var laparam = config.GetSection("LogAnalytics");
            uploader = new AzureUploader
            {
                Messages = listener.Messages,
                Logger = logger,
                WorkspaceID = laparam["WorkspaceID"],
                PrimaryKey = laparam["PrimaryKey"],
                LogName = laparam["LogName"],
            };
            var nErr = 0;
            if( uploader.WorkspaceID == null)
            {
                logger.LogError($"ERROR: Blank WorkspaceID in appsettings.json");
                nErr++;
            }
            if (uploader.PrimaryKey == null)
            {
                logger.LogError($"ERROR: Blank PrimaryKey in appsettings.json");
                nErr++;
            }
            if (uploader.LogName == null)
            {
                logger.LogError($"ERROR: Blank LogName in appsettings.json");
                nErr++;
            }
            if( nErr > 0)
            {
                throw new ArgumentNullException("invalid appsettings.json");
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
#if DEBUG
            await Task.Delay(1, stoppingToken);
#else
            await Task.Delay(5531, stoppingToken);
#endif
            while (!stoppingToken.IsCancellationRequested)
            {
                var w1 = await listener.ListenAsync(stoppingToken);
                var w2 = await uploader.UploadAsync(stoppingToken);
                await Task.Delay(Math.Min(w1, w2), stoppingToken);
            }
        }
    }
}
