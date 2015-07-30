using System;
using System.Configuration;

namespace NeuronDocumentSyncService.Cfg
{
    public class GeneralConfigSection : ConfigurationSection
    {
        private const string TempDirectoryPathName = "tempDirectoryPath";
        [ConfigurationProperty(TempDirectoryPathName, DefaultValue = "", IsRequired = false)]
        public string TempDirectoryPath
        {
            get { return (string)this[TempDirectoryPathName]; }
            set { this[TempDirectoryPathName] = value; }
        }

        private const string IsServiceTimeEnableName = "isServiceTimeEnable";
        [ConfigurationProperty(IsServiceTimeEnableName, DefaultValue = false, IsRequired = false)]
        public bool IsServiceTimeEnable
        {
            get { return (bool)this[IsServiceTimeEnableName]; }
            set { this[IsServiceTimeEnableName] = value; }
        }

        private const string ServiceTimeStartName = "serviceTimeStart";
        [ConfigurationProperty(ServiceTimeStartName, IsRequired = false)]
        public TimeSpan ServiceTimeStart
        {
            get { return (TimeSpan)this[ServiceTimeStartName]; }
            set { this[ServiceTimeStartName] = value; }
        }

        private const string ServiceTimeEndName = "serviceTimeEnd";
        [ConfigurationProperty(ServiceTimeEndName, IsRequired = false)]
        public TimeSpan ServiceTimeEnd
        {
            get { return (TimeSpan)this[ServiceTimeEndName]; }
            set { this[ServiceTimeEndName] = value; }
        }

        private const string SyncOperationBreakValueName = "syncOperationBreakValue";
        [ConfigurationProperty(SyncOperationBreakValueName, DefaultValue = 0, IsRequired = false)]
        public int SyncOperationBreakValue
        {
            get { return (int)this[SyncOperationBreakValueName]; }
            set { this[SyncOperationBreakValueName] = value; }
        }

        private const string WebImportUrlName = "webImportUrl";
        [ConfigurationProperty(WebImportUrlName, DefaultValue = "", IsRequired = false)]
        public string WebImportUrl
        {
            get { return (string)this[WebImportUrlName]; }
            set { this[WebImportUrlName] = value; }
        }

        private const string WebImportLoginName = "webImportLogin";
        [ConfigurationProperty(WebImportLoginName, DefaultValue = "", IsRequired = false)]
        public string WebImportLogin
        {
            get { return (string)this[WebImportLoginName]; }
            set { this[WebImportLoginName] = value; }
        }

        private const string WebImportPassName = "webImportPass";
        [ConfigurationProperty(WebImportPassName, DefaultValue = "", IsRequired = false)]
        public string WebImportPass
        {
            get { return (string)this[WebImportPassName]; }
            set { this[WebImportPassName] = value; }
        }

        public override bool IsReadOnly()
        {
            return false;
        }
    }
}