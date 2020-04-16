using System;
using System.Collections.Generic;
using System.Configuration.Install;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EventlogAzureMonitorBridge
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
			args = Environment.GetCommandLineArgs();   // Necessary to get command args
            var service = new Service1();

			if (Environment.UserInteractive)    // Console version
			{
				if (args.Length > 0)
				{
					// Support to install myself.
					var isServiceExists = ServiceController.GetServices().Any(s => s.ServiceName == service.ServiceName);
					var path = Assembly.GetExecutingAssembly().Location;
					switch (args[0].ToLower())
					{
						case "/i":
							if (isServiceExists)
							{
								Console.WriteLine($"The service named {service.ServiceName} have already installed.");
							}
							else
							{
								ManagedInstallerClass.InstallHelper(new[] { path });
							}
							return;
						case "/u":
							if (isServiceExists)
							{
								ManagedInstallerClass.InstallHelper(new[] { "/u", path });
							}
							else
							{
								Console.WriteLine($"The service '{service.ServiceName}' is not installed yet.");
							}
							return;
					}
				}
				service.OnStartConsole(args);
				service.OnStopConsole();

				Console.WriteLine($"\r\n\r\n==== Hit any key to exit.");
				Console.Read();
			}
			else
			{    // Run from windows service
				ServiceBase.Run(new[] { service, });
			}
		}
    }
}
