using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using NeuronDocumentSync.Enums;
using NeuronDocumentSync.Infrastructure;
using NeuronDocumentSync.Interfaces;
using NeuronDocumentSync.Models;
using NeuronDocumentSync.Resources;
using Ninject;
using Ninject.Parameters;

namespace NeuronDocumentSyncService
{
    static class Program
    {
        static int Main(string[] args)
        {
            var kernel = new StandardKernel(new SyncServiceBindings());
            var logger = kernel.Get<INeuronLogger>();
            var dbConfig = kernel.Get<FbDbConnectionConfig>();
            dbConfig.Load();
            var config = kernel.Get<IGeneralConfig>();
            config.Load();
            dbConfig.Load();
            try
            {
                ServiceManagerCommand serviceCommand;
                string[] serviceArgs;

                if (!TryParseCommandLine(args, out serviceCommand, out serviceArgs))
                {
                    PrintUsage();
                    Console.ReadKey();

                    ExitCode = -1;
                }
                else
                {
                    logger.LogType = logger.LogType | LoggerType.File;
                    switch (serviceCommand)
                    {
                        case ServiceManagerCommand.Unknown:
                            PrintUsage();
                            Console.ReadKey();

                            break;
                        case ServiceManagerCommand.Application:
                            RunConsoleMode(kernel, serviceArgs);
                            break;
                        default:
                            var serviceManager = kernel.Get<IServiceManager>(new ConstructorArgument("serviceArgs", serviceArgs));
                            switch (serviceCommand)
                            {
                                case ServiceManagerCommand.Install:
                                    serviceManager.InstallService();
                                    break;
                                case ServiceManagerCommand.Start:
                                    if (Environment.UserInteractive)
                                    {
                                        serviceManager.StartService();
                                    }
                                    else
                                    {
                                        if (!serviceManager.IsServiceInstalled())
                                        {
                                            serviceManager.InstallService();
                                        }
                                        ServiceBase.Run(new DocumentSyncService());
                                    }
                                    break;
                                case ServiceManagerCommand.Stop:
                                    serviceManager.StopService();
                                    break;
                                case ServiceManagerCommand.UnInstall:
                                    serviceManager.UninstallService();
                                    break;
                            }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                var msg = string.Format("{0}: {1}.",
                    MainMessages.rs_ErrorOccurred,
                    "MSSBackupService.Program.Main()");
                logger.AddLog(msg, ex, EventLogEntryType.Error);

                ExitCode = -1;
            }
            return ExitCode;
        }

        private static int ExitCode = 0;

        private static void RunConsoleMode(IKernel kernel, string[] args)
        {
            var logger = kernel.Get<INeuronLogger>();
            logger.LogType = LoggerType.Console;
            logger.AddLog(MainMessages.rs_ServiceStartedAsConsoleApplication);
            var syncCore = kernel.Get<ISyncCore>();

            try
            {
                syncCore.OnStart(args);
            }
            catch (Exception ex)
            {
                var msg = string.Format("{0}: {1}", MainMessages.rs_ErrorOccurred, "OnStartService");
                logger.AddLog(msg, ex, EventLogEntryType.Error);

                ExitCode = -1;
            }

            if (syncCore.GetStatus() == SyncCoreStatus.Error)
            {
                logger.AddLog(MainMessages.rs_ServerErrorServiceWillBeStopped);
                ExitCode = -1;

                try
                {
                    syncCore.OnStop();
                    return;
                }
                catch (Exception ex)
                {
                    var msg = string.Format("{0}: {1}", MainMessages.rs_ErrorOccurred, "OnStopService");
                    logger.AddLog(msg, ex, EventLogEntryType.Error);

                    ExitCode = -1;
                }
            }

            Console.WriteLine(MainMessages.rs_PressEnterToStop);
            Console.Read();

            try
            {
                syncCore.OnStop();
            }
            catch (Exception ex)
            {
                var msg = string.Format("{0}: {1}", MainMessages.rs_ErrorOccurred, "OnStopService");
                logger.AddLog(msg, ex, EventLogEntryType.Error);

                ExitCode = -1;
            }
        }

        private static bool TryParseCommandLine(string[] args,
            out ServiceManagerCommand aCommand, out string[] serviceArgs)
        {
            aCommand = ServiceManagerCommand.Unknown;
            serviceArgs = null;
            var res = false;

            if (args != null && args.Length > 0)
            {
                if (commands.ContainsKey(args[0]))
                {
                    aCommand = commands[args[0]];
                    switch (aCommand)
                    {
                        case ServiceManagerCommand.Start:
                        case ServiceManagerCommand.Application:
                            if (args.Length == 2)
                            {
                                serviceArgs = new[] { args[1] };
                                res = true;
                            }
                            else
                            {
                                serviceArgs = new string[] { };
                                res = args.Length == 1;
                            }
                            break;
                        default:
                            res = true;
                            break;
                    }
                }
            }
            else
            {
                serviceArgs = new string[] { };
                aCommand = ServiceManagerCommand.Start;
                res = true;
            }
            return res;
        }

        private static void PrintUsage()
        {
            var exeName = Assembly.GetExecutingAssembly().ManifestModule.Name;
            Console.WriteLine(MainMessages.rs_CommansInfo);
            foreach (var key in commands.Keys)
            {
                Console.WriteLine(string.Format("  {0} {1}", exeName, key));
            }
        }

        private static readonly Dictionary<string, ServiceManagerCommand> commands =
        #region commands

        new Dictionary<string, ServiceManagerCommand>
            {
                {"-console", ServiceManagerCommand.Application},
                {"-install", ServiceManagerCommand.Install},
                {"-start", ServiceManagerCommand.Start},
                {"-stop", ServiceManagerCommand.Stop},
                {"-uninstall", ServiceManagerCommand.UnInstall},
                {"-help", ServiceManagerCommand.Unknown}
            };

        #endregion

        public enum ServiceManagerCommand
        {
            Unknown,
            Application,
            Install,
            Start,
            Stop,
            UnInstall
        }
        /*static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
            { 
                new DocumentSyncService() 
            };
            ServiceBase.Run(ServicesToRun);
        }*/
    }
}
