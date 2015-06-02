using System.Collections.Specialized;
using NeuronDocumentSync.Interfaces;
using NeuronDocumentSync.Models;
using  System.Configuration;
using NeuronDocumentSyncService.Cfg;

namespace NeuronDocumentSyncService.Infrastructure
{
    public class ConfigProcessor : IConfigProcessor
    {
        private const string _generalSectionName = "General";
        

        private readonly string _fbDbSectionName = "FbDatabase";
        public void LoadGeneralConfig(IGeneralConfig cfg)
        {
            var currentConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var genSettings
                = currentConfig.GetSection(_generalSectionName) as GeneralConfigSection;
            if (genSettings != null)
            {
                if (genSettings.TempDirectoryPath != "")
                  cfg.TempDirectoryPath = genSettings.TempDirectoryPath;
            }
            
            
        }

        public bool SaveGeneralConfig(IGeneralConfig cfg)
        {
            var currentConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var genSettings
                = currentConfig.GetSection(_generalSectionName) as GeneralConfigSection;

            if (genSettings == null)
            {
                genSettings = new GeneralConfigSection();
                currentConfig.Sections.Add(_generalSectionName, genSettings);
                currentConfig.Save(ConfigurationSaveMode.Full);
            }

            genSettings.TempDirectoryPath = cfg.TempDirectoryPath;
            
            ConfigurationManager.RefreshSection(_generalSectionName);
            currentConfig.Save(ConfigurationSaveMode.Full);

            return true;
        }

        public void LoadFbDbConnectionConfig(FbDbConnectionConfig cfg)
        {
            var currentConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var dbSettings
                = currentConfig.GetSection(_fbDbSectionName) as FbDbConfigSection;
            if (dbSettings != null)
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
        }

        public bool SaveFbDbConnectionConfig(FbDbConnectionConfig cfg)
        {
            var currentConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var dbSettings
                = currentConfig.GetSection(_fbDbSectionName) as FbDbConfigSection;

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