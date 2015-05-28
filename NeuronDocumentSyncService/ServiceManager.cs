using System;
using System.Collections;
using System.Configuration.Install;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using NeuronDocumentSync.Infrastructure;
using NeuronDocumentSync.Interfaces;
using NeuronDocumentSync.Resources;

namespace NeuronDocumentSyncService
{

    public class ServiceManager : IServiceManager
    {

        private readonly string _serviceName;
        private readonly string[] _args;
        private readonly INeuronLogger _logger;


        public ServiceManager(INeuronLogger logger, ServiceBase service, string[] serviceArgs)
        {
            _logger = logger;
            _serviceName = service.ServiceName;
            _args = serviceArgs;
        }

        public bool IsServiceInstalled()
        {
            using (var serviceController = new ServiceController(_serviceName))
            {
                try
                {
                    var status = serviceController.Status;
                }
                catch (InvalidOperationException)
                {
                    return false;
                }
                catch (Exception ex)
                {
                    _logger.AddLog(MainMessages.rs_CanNotReadServiceStatus, ex, EventLogEntryType.Error);
                    return false;
                }
                return true;
            }
        }

        public bool IsServiceRunning()
        {
            using (var serviceController = new ServiceController(_serviceName))
            {
                return IsServiceInstalled() && serviceController.Status == ServiceControllerStatus.Running;
            }
        }

        public void InstallService()
        {
            if (!IsServiceInstalled())
            {
                try
                {
                    IDictionary savedState = new Hashtable();
                    var installer = new AssemblyInstaller
                    {
                        Path = Assembly.GetEntryAssembly().Location,
                        UseNewContext = true
                    };
                    try
                    {
                        _logger.DeleteEventSource();

                        installer.Install(savedState);
                        installer.Commit(savedState);
                    }
                    catch (Exception ex)
                    {
                        _logger.AddLog(MainMessages.rs_CanNotInstallService, ex, EventLogEntryType.Error);
                        installer.Rollback(savedState);
                        _logger.AddLog(MainMessages.rs_ServiceInstallationWasRolledBack);
                    }
                    /*try
                    {
                        new ServiceControlManager().SetRestartOnFailure(_serviceName);
                    }
                    catch (Exception ex)
                    {
                        _logger.AddLog(MainMessages.rs_ErrorWhileChangingServiceFailureActions, ex, EventLogEntryType.Error);
                    }*/
                }
                catch (Exception ex)
                {
                    _logger.AddLog(MainMessages.rs_ErrorWhileInstallingService, ex, EventLogEntryType.Error);
                }
            }
        }

        public void UninstallService()
        {
            if (IsServiceInstalled())
            {
                try
                {
                    IDictionary savedState = new Hashtable();
                    var installer = new AssemblyInstaller
                    {
                        Path = Assembly.GetEntryAssembly().Location,
                        UseNewContext = true
                    };
                    try
                    {
                        installer.Uninstall(savedState);
                        if (EventLog.SourceExists(_serviceName))
                        {
                            EventLog.DeleteEventSource(_serviceName);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.AddLog(MainMessages.rs_CanNotUninstallService, ex, EventLogEntryType.Error);
                    }
                }
                catch (Exception ex)
                {
                    _logger.AddLog(MainMessages.rs_ErrorWhileUninstallingService, ex, EventLogEntryType.Error);
                }
            }
        }

        public void StartService()
        {
            InstallService();

            if (IsServiceInstalled())
            {
                using (var serviceController = new ServiceController(_serviceName))
                {
                    if (serviceController.Status == ServiceControllerStatus.Stopped)
                    {
                        try
                        {
                            serviceController.Start(_args);
                            serviceController.WaitForStatus(ServiceControllerStatus.Running, new TimeSpan(0, 0, 0, 30));
                        }
                        catch (InvalidOperationException ex)
                        {
                            _logger.AddLog(MainMessages.rs_CanNotStartService, ex, EventLogEntryType.Error);
                        }
                    }
                }
            }
            else
            {
                _logger.AddLog(MainMessages.rs_CanNotStartNotInstalledService, null, EventLogEntryType.Error);
            }
        }

        public void StopService()
        {
            if (IsServiceInstalled())
            {
                using (var serviceController = new ServiceController(_serviceName))
                {
                    if (serviceController.Status == ServiceControllerStatus.Running)
                    {
                        try
                        {
                            serviceController.Stop();
                            serviceController.WaitForStatus(ServiceControllerStatus.Stopped, new TimeSpan(0, 0, 0, 30));
                        }
                        catch (Exception ex)
                        {
                            _logger.AddLog(MainMessages.rs_CanNotStopService, ex, EventLogEntryType.Error);
                        }
                    }
                }
            }
        } 
    }
}