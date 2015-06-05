using System.Configuration;

namespace NeuronDocumentSyncService.Cfg
{
    public class FbDbConfigSection : ConfigurationSection
    {
        public override bool IsReadOnly()
        {
            return false;
        }

        private const string CharsetName = "charset";
        private const string UserNameName = "userName";
        private const string PasswordName = "password";
        private const string HostNameName = "hostName";
        private const string DbFilePathName = "dbFilePath";
        private const string PortNumberName = "portNumber";

        [ConfigurationProperty(CharsetName, DefaultValue = "", IsRequired = false)]
        public string Charset
        {
            get { return (string)this[CharsetName]; }
            set { this[CharsetName] = value; }
        }

        [ConfigurationProperty(UserNameName, DefaultValue = "", IsRequired = false)]
        public string UserName
        {
            get { return (string)this[UserNameName]; }
            set { this[UserNameName] = value; }
        }

        [ConfigurationProperty(PasswordName, DefaultValue = "", IsRequired = false)]
        public string Password
        {
            get { return (string)this[PasswordName]; }
            set { this[PasswordName] = value; }
        }

        [ConfigurationProperty(HostNameName, DefaultValue = "", IsRequired = false)]
        public string HostName
        {
            get { return (string)this[HostNameName]; }
            set { this[HostNameName] = value; }
        }

        [ConfigurationProperty(DbFilePathName, DefaultValue = "", IsRequired = false)]
        public string DbFilePath
        {
            get { return (string)this[DbFilePathName]; }
            set { this[DbFilePathName] = value; }
        }

        [ConfigurationProperty(PortNumberName, DefaultValue = 0, IsRequired = false)]
        public int PortNumber
        {
            get { return (int)this[PortNumberName]; }
            set { this[PortNumberName] = value; }
        } 
    }
}