using System.Configuration;

namespace NeuronDocumentSyncService.Cfg
{
    public class GeneralConfigSection : ConfigurationSection
    {
        private const string TempDirectoryPathName = "tempDirectoryPath";
        [ConfigurationProperty(TempDirectoryPathName, DefaultValue = "", IsRequired = true)]
        public string TempDirectoryPath
        {
            get { return (string)this[TempDirectoryPathName]; }
            set { this[TempDirectoryPathName] = value; }
        }

        public override bool IsReadOnly()
        {
            return false;
        }
    }
}