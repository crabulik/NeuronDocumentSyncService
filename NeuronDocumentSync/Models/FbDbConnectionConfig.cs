using FirebirdSql.Data.FirebirdClient;

namespace NeuronDocumentSync.Models
{
    public class FbDbConnectionConfig
    {
        public FbDbConnectionConfig()
        {
            Charset = "UTF8";
            UserName = "SYSDBA";
            Password = "masterkey";
            HostName = "localhost";
            PortNumber = 3050;
        }
        public string Charset { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public string HostName { get; set; }

        public string DbFilePath { get; set; }

        public int PortNumber { get; set; }
        
        
        
        public string ToConnectionString()
        {
            var builder = new FbConnectionStringBuilder();
            builder.Charset = Charset;
            builder.UserID = UserName;
            builder.Password = Password;
            builder.DataSource = HostName;
            builder.Database = DbFilePath;
            builder.Port = PortNumber;


            return builder.ToString();
        }
    }
}
