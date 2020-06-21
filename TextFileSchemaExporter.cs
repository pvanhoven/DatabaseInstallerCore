using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.SqlClient;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;

namespace DatabaseInstallerCore
{
    public class TextFileSchemaExporter
    {
        private readonly DbScripter scripter;
        private readonly DirectoryService directoryService;

        public TextFileSchemaExporter(DbScripter scripter, DirectoryService directoryService)
        {
            this.scripter = scripter;
            this.directoryService = directoryService;
        }

        public void Export(Options options)
        {
            CreateDirectories(options);

            using(SqlConnection sqlConnection = new SqlConnection(string.Format("Data Source={0};Integrated Security=SSPI", options.Server)))
            {
                ServerConnection serverConnection = new ServerConnection(sqlConnection);
                Server server = new Server(serverConnection);

                Database database = server.Databases[options.Name];
                if (database == null)
                {
                    throw new Exception(string.Format("Database: '{0}' does not exist", options.Name));
                }

                foreach (Table table in database.Tables.Cast<Table>().Where(t => !t.IsSystemObject))
                {
                    if (table.Name.StartsWith("__"))
                    {
                        Console.WriteLine("Skipping table: " + table.Name);
                        continue;
                    }

                    Console.WriteLine("Scripting table: '{0}'", table.Name);

                    scripter.ScriptObject(table, options);

                    IEnumerable<Trigger> tableTriggers = table.Triggers.Cast<Trigger>();
                    if (tableTriggers.Any())
                    {
                        scripter.ScriptObject(tableTriggers, table.Name, options);
                    }
                }

                foreach (View view in database.Views.Cast<View>().Where(v => !v.IsSystemObject))
                {
                    Console.WriteLine("Scripting view: '{0}'", view.Name);

                    scripter.ScriptObject(view, options);
                }

                foreach (StoredProcedure procedure in database.StoredProcedures.Cast<StoredProcedure>().Where(p => !p.IsSystemObject))
                {
                    Console.WriteLine("Scripting stored procedure: '{0}'", procedure.Name);

                    scripter.ScriptObject(procedure, options);
                }

                foreach (UserDefinedFunction function in database.UserDefinedFunctions.Cast<UserDefinedFunction>().Where(f => !f.IsSystemObject))
                {
                    Console.WriteLine("Scripting function: '{0}'", function.Name);

                    scripter.ScriptObject(function, options);
                }
            }
        }

        private void CreateDirectories(Options options)
        {
            directoryService.CreateTablesDirectory(options.Root);
            directoryService.CreateForeignKeysDirectory(options.Root);
            directoryService.CreateViewsDirectory(options.Root);
            directoryService.CreateStoredProceduresDirectory(options.Root);
            directoryService.CreateFunctionsDirectory(options.Root);
            directoryService.CreateTriggersDirectory(options.Root);
        }
    }
}