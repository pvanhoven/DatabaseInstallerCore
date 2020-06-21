using CommandLine;

namespace DatabaseInstallerCore
{
    public class Options
    {
        [Option('e', "export", HelpText = "Export the specified database's schema")]
        public bool Export { get; set; }

        [Option('i', "install", HelpText = "Create a new database instance")]
        public bool Install { get; set; }

        [Option('s', "server", HelpText = "Network name or ip address of server (and optionally SQL instance) to install a database")]
        public string Server { get; set; }

        [Option('n', "name", HelpText = "The name of the database")]
        public string Name { get; set; }

        [Option('u', "username", HelpText = "The database username to connect with")]
        public string UserName { get; set; }

        [Option('p', "password", HelpText = "The database passpword to connect with")]
        public string Password { get; set; }

        [Option('r', "root", HelpText = "The root database folder")]
        public string Root { get; set; }

        [Option('d', "drop", HelpText = "Indicates the an existing database should be dropped before creating a new one")]
        public bool Drop { get; set; }

        [Option('f', "force", HelpText = "Will drop an existing database without warning")]
        public bool ForceDrop { get; set; }

        [Option('h', "help", HelpText = "Displays the help text")]
        public bool Help { get; set; }
    }
}