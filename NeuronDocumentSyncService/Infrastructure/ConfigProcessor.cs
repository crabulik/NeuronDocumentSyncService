using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Text;
using NeuronDocumentSync.Interfaces;
using NeuronDocumentSync.Models;
using  System.Configuration;
using NeuronDocumentSync.Resources;
using NeuronDocumentSyncService.Cfg;

namespace NeuronDocumentSyncService.Infrastructure
{
    public class ConfigProcessor : IConfigProcessor
    {
        private const string _generalSectionName = "General";
        

        private readonly string _fbDbSectionName = "FbDatabase";
        private INeuronLogger _logger;

        private void ProcessConfigErrors(ConfigurationSection section, string message)
        {
            var list = section.ElementInformation.Errors.SyncRoot as ConfigurationException[];
            if ((list != null) && (list.Length > 0))
            {
                var result = new StringBuilder();  
                result.AppendLine();
                foreach (var item in list)
                {
                    result.AppendLine(item.Message);
                }
                message += result.ToString();
            }              
            _logger.AddLog(message);
        }
        
        public ConfigProcessor(INeuronLogger logger)
        {
            _logger = logger;
        }
        public void LoadGeneralConfig(IGeneralConfig cfg)
        {
            var currentConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            GeneralConfigSection genSettings;
            try
            {
                genSettings
                    = currentConfig.GetSection(_generalSectionName) as GeneralConfigSection;
            }
            catch (ConfigurationErrorsException e)
            {
                _logger.AddLog(MainMessages.rs_CantReadGeneralConfigSection, e, EventLogEntryType.Error);
                return;
            }
            if (genSettings != null)
            {
                if (genSettings.ElementInformation.Errors.Count > 0)
                {
                    ProcessConfigErrors(genSettings, MainMessages.rs_CantReadGeneralConfigSection);
                    return;
                }
                try
                {
                    if (genSettings.TempDirectoryPath != "")
                        cfg.TempDirectoryPath = genSettings.TempDirectoryPath;

                    if(genSettings.SyncOperationBreakValue > 0)
                        cfg.SyncOperationBreakValue = genSettings.SyncOperationBreakValue;

                    cfg.IsServiceTimeEnable = genSettings.IsServiceTimeEnable;

                    var controlTimeSpan = new TimeSpan(24, 0, 0);
                    if (genSettings.ServiceTimeStart < controlTimeSpan)
                        cfg.ServiceTimeStart = genSettings.ServiceTimeStart;
                    else
                    {
                        _logger.AddLog(MainMessages.rs_CantReadGeneralConfigSection + Environment.NewLine + MainMessages.rs_ServiceTimeStartErrorValue);
                    }
                    if (genSettings.ServiceTimeEnd < controlTimeSpan)
                        cfg.ServiceTimeEnd = genSettings.ServiceTimeEnd;
                    else
                    {
                        _logger.AddLog(MainMessages.rs_CantReadGeneralConfigSection + Environment.NewLine + MainMessages.rs_ServiceTimeEndErrorValue);
                    }

                    cfg.IsServiceTimeEnable = genSettings.IsServiceTimeEnable;
                }
                catch (Exception e)
                {
                    _logger.AddLog(MainMessages.rs_CantReadGeneralConfigSection, e, EventLogEntryType.Error);
                }
            }
            
            
        }

        public bool SaveGeneralConfig(IGeneralConfig cfg)
        {
            var currentConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            GeneralConfigSection genSettings;
            try
            {
                genSettings
                    = currentConfig.GetSection(_generalSectionName) as GeneralConfigSection;
            }
            catch (ConfigurationErrorsException e)
            {
                _logger.AddLog(MainMessages.rs_CantReadGeneralConfigSection, e, EventLogEntryType.Error);
                currentConfig.Sections.Remove(_generalSectionName);
                genSettings = null;
            }

            if (genSettings == null)
            {
                genSettings = new GeneralConfigSection();
                currentConfig.Sections.Add(_generalSectionName, genSettings);
                currentConfig.Save(ConfigurationSaveMode.Full);
            }

            genSettings.TempDirectoryPath = cfg.TempDirectoryPath;
            genSettings.IsServiceTimeEnable = cfg.IsServiceTimeEnable;
            genSettings.ServiceTimeStart = cfg.ServiceTimeStart;
            genSettings.ServiceTimeEnd = cfg.ServiceTimeEnd;
            genSettings.SyncOperationBreakValue = cfg.SyncOperationBreakValue;
            
            ConfigurationManager.RefreshSection(_generalSectionName);
            currentConfig.Save(ConfigurationSaveMode.Full);

            return true;
        }

        public void LoadFbDbConnectionConfig(FbDbConnectionConfig cfg)
        {
            var currentConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            FbDbConfigSection dbSettings;
            try
            {
                dbSettings
                  = currentConfig.GetSection(_fbDbSectionName) as FbDbConfigSection;
            }
            catch (ConfigurationErrorsException e)
            {
                _logger.AddLog(MainMessages.rs_CantReadFbDbConfigSection, e, EventLogEntryType.Error);
                return;
            }
            
            if (dbSettings != null)
            {
                if (dbSettings.ElementInformation.Errors.Count > 0)
                {
                    ProcessConfigErrors(dbSettings, MainMessages.rs_CantReadFbDbConfigSection);
                    return;
                }
                try
                {
                    if (dbSettings.Charset != "")
                        cfg.Charset = dbSettings.Charset;
                    if (dbSettings.UserName != "")
                        cfg.UserName = dbSettings.UserName;
                    if (dbSettings.Password != "")
                        cfg.Password = dbSettings.Password;
                    if (dbSettings.HostName != "")
                        cfg.HostName = dbSettings.HostName;
                    if (dbSettings.DbFilePath != "")
                        cfg.DbFilePath = dbSettings.DbFilePath;
                    if (dbSettings.PortNumber > 0)
                        cfg.Charset = dbSettings.Charset;
                }
                catch (Exception e)
                {
                    _logger.AddLog(MainMessages.rs_CantReadFbDbConfigSection, e, EventLogEntryType.Error);
                }
            }
        }

        public bool SaveFbDbConnectionConfig(FbDbConnectionConfig cfg)
        {
            var currentConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            FbDbConfigSection dbSettings;
            try
            {
                dbSettings
                  = currentConfig.GetSection(_fbDbSectionName) as FbDbConfigSection;
            }
            catch (ConfigurationErrorsException e)
            {
                _logger.AddLog(MainMessages.rs_CantReadGeneralConfigSection, e, EventLogEntryType.Error);
                currentConfig.Sections.Remove(_fbDbSectionName);
                dbSettings = null;
            }

            if (dbSettings == null)
            {
                dbSettings = new FbDbConfigSection();
                currentConfig.Sections.Add(_fbDbSectionName, dbSettings);
                currentConfig.Save(ConfigurationSaveMode.Full);
            }

            dbSettings.Charset = cfg.Charset;
            dbSettings.UserName = cfg.UserName;
            dbSettings.Password = cfg.Password;
            dbSettings.HostName = cfg.HostName;
            dbSettings.DbFilePath = cfg.DbFilePath;
            dbSettings.PortNumber = cfg.PortNumber;

            ConfigurationManager.RefreshSection(_fbDbSectionName);
            currentConfig.Save(ConfigurationSaveMode.Full);

            return true;
        }
    }
}