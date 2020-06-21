using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Text;
using Microsoft.Data.SqlClient;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;

namespace DatabaseInstallerCore
{
    public class Installer
    {
        private readonly ConnectionStringService _connectionStringService;
        private readonly DirectoryService _directoryService;

        public Installer()
        {
            _connectionStringService = new ConnectionStringService();
            _directoryService = new DirectoryService();
        }

        public void Create(Options options)
        {
            if (ShouldDropDatabase(options))
            {
                Console.WriteLine("Dropping database: '{0}'", options.Name);
                DropDatabase(options);
            }

            CreateDatabase(options);

            CreateTables(options);
            CreateTriggers(options);
            CreateForeignKeys(options);
            CreateViews(options);
            CreateFunctions(options);
            CreateStoredProcedures(options);
        }

        public bool ShouldDropDatabase(Options options)
        {
            if (!Exists(options.Name, options))
            {
                return false;
            }

            if (!options.Drop)
            {
                // database already exists, we don't want to modify during an install
                throw new Exception("Database already exists");
            }

            if (options.ForceDrop)
            {
                return true;
            }

            Console.Write($"Drop database: {options.Name} (Y/n): ");
            string dropText = Console.ReadLine();
            return StringIsY(dropText);
        }

        private bool StringIsY(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            return text.ToLower() == "y";
        }

        public void CreateDatabase(Options options)
        {
            string connectionString = _connectionStringService.GetServerConnectionString(options);
            using(SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                ServerConnection serverConnection = new ServerConnection(sqlConnection);
                Server server = new Server(serverConnection);

                Console.WriteLine("Creating database: '{0}'", options.Name);
                Database database = new Database(server, options.Name);
                database.Create();
            }
        }

        public bool Exists(string databaseName, Options options)
        {
            string connectionString = _connectionStringService.GetServerConnectionString(options);
            using(SqlConnection connection = new SqlConnection(connectionString))
            {
                ServerConnection serverConnection = new ServerConnection(connection);
                Server server = new Server(serverConnection);

                return server.Databases[databaseName] != null;
            }
        }

        public void DropDatabase(Options options)
        {
            string connectionString = _connectionStringService.GetServerConnectionString(options);
            using(SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                ServerConnection serverConnection = new ServerConnection(sqlConnection);
                Server server = new Server(serverConnection);

                server.KillDatabase(options.Name);
            }
        }

        public void CreateTables(Options options)
        {
            string directory = _directoryService.GetTablesDirectory(options.Root);
            if (!Directory.Exists(directory))
            {
                throw new Exception(string.Format("Directory '{0}' does not exist", directory));
            }

            IEnumerable<string> files = Directory.GetFiles(directory);
            ProcessDirectoryFiles(files, options);
        }

        public void CreateTriggers(Options options)
        {
            string directory = _directoryService.GetTriggersDirectory(options.Root);
            if (!Directory.Exists(directory))
            {
                Console.WriteLine($"Triggers directory: '{directory}' does not exist, skipping");
                return;
            }

            IEnumerable<string> files = Directory.GetFiles(directory);
            ProcessDirectoryFiles(files, options);
        }

        public void CreateForeignKeys(Options options)
        {
            string directory = _directoryService.GetForeignKeysDirectory(options.Root);
            if (!Directory.Exists(directory))
            {
                throw new Exception(string.Format("Directory '{0}' does not exist", directory));
            }

            IEnumerable<string> files = Directory.GetFiles(directory, "*AddFks.sql");
            ProcessDirectoryFiles(files, options);
        }

        public void CreateViews(Options options)
        {
            string directory = _directoryService.GetViewsDirectory(options.Root);
            if (!Directory.Exists(directory))
            {
                throw new Exception(string.Format("Directory '{0}' does not exist", directory));
            }

            for (int x = 0; x < 2; x++)
            {
                IEnumerable<string> files = Directory.GetFiles(directory);
                ProcessDirectoryFiles(files, options);
            }
        }

        public void CreateFunctions(Options options)
        {
            string directory = _directoryService.GetFunctionsDirectory(options.Root);
            if (!Directory.Exists(directory))
            {
                Console.WriteLine($"Functions directory '{directory}' does not exist, skipping");
                return;
            }

            IEnumerable<string> files = Directory.GetFiles(directory);
            ProcessDirectoryFiles(files, options);
        }

        public void CreateStoredProcedures(Options options)
        {
            string directory = _directoryService.GetStoredProceduresDirectory(options.Root);
            if (!Directory.Exists(directory))
            {
                Console.WriteLine($"Stored procedures directory '{directory}' does not exist");
                return;
            }

            IEnumerable<string> files = Directory.GetFiles(directory);
            ProcessDirectoryFiles(files, options);
        }

        private void ProcessDirectoryFiles(IEnumerable<string> files, Options options)
        {
            string connectionString = _connectionStringService.GetDatabaseConnectionString(options);
            using(IDbConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                foreach (string fileName in files)
                {
                    Console.WriteLine("Processing file: '{0}'", fileName);
                    IEnumerable<string> statements = GetFileStatements(fileName);

                    foreach (string statement in statements)
                    {
                        IDbCommand createTableCommand = connection.CreateCommand();
                        createTableCommand.CommandText = statement;

                        try
                        {
                            createTableCommand.ExecuteNonQuery();
                        }
                        catch (SqlException se)
                        {
                            if (se.Message.Contains("Invalid Object"))
                            {
                                Console.WriteLine("Invalid object exception : '{0}'", fileName);
                            }
                        }
                        catch (DbException)
                        {
                            throw;
                        }
                    }
                }
            }
        }

        private static IEnumerable<string> GetFileStatements(string path)
        {
            using(StreamReader reader = new StreamReader(path))
            {
                List<string> statements = new List<string>();
                StringBuilder statement = new StringBuilder();
                while (reader.Peek() > 0)
                {
                    string line = reader.ReadLine();
                    if (string.IsNullOrEmpty(line))
                    {
                        continue;
                    }

                    if (line.Equals("GO", StringComparison.InvariantCultureIgnoreCase))
                    {
                        statements.Add(statement.ToString().Trim());
                        statement.Clear();
                        continue;
                    }

                    statement.AppendLine(line);
                }

                if (statement.Length > 0)
                {
                    statements.Add(statement.ToString());
                }

                return statements;
            }
        }
    }
}