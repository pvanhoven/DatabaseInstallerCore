namespace DatabaseInstallerCore
{
    public class ConnectionStringService
    {
        public string GetDatabaseConnectionString(Options options)
        {
            if (string.IsNullOrWhiteSpace(options.UserName) || string.IsNullOrWhiteSpace(options.Password))
            {
                return GetDatabaseIntegratedSecurityConnectionString(options.Server, options.Name);
            }

            return GetDatabaseUserPassConnectionString(options.Server, options.Name, options.UserName, options.Password);
        }

        public string GetServerConnectionString(Options options)
        {
            if (string.IsNullOrWhiteSpace(options.UserName) || string.IsNullOrWhiteSpace(options.Password))
            {
                return GetServerIntegratedSecurityConnectionString(options.Server);
            }

            return GetServerUserPassConnectionString(options.Server, options.UserName, options.Password);
        }

        private string GetServerIntegratedSecurityConnectionString(string server)
        {
            return $"Server={server};Integrated Security=SSPI";
        }

        private string GetServerUserPassConnectionString(string server, string user, string password)
        {
            return $"Server={server};User Id={user};Password={password}";
        }

        private string GetDatabaseIntegratedSecurityConnectionString(string server, string database)
        {
            return $"Server={server};Database={database};Integrated Security=SSPI";
        }

        private string GetDatabaseUserPassConnectionString(string server, string database, string user, string password)
        {
            return $"Server={server};Database={database};User Id={user};Password={password}";
        }
    }
}