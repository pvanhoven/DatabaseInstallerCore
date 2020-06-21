using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.Management.Smo;

namespace DatabaseInstallerCore
{
    public class DbScripter
    {
        private readonly FileWriter _fileWriter;
        private readonly DirectoryService _directoryService;
        private readonly ScriptingOptions _scriptingOptions;

        public DbScripter(FileWriter fileWriter, DirectoryService directoryService)
        {
            _fileWriter = fileWriter;
            _directoryService = directoryService;

            _scriptingOptions = new ScriptingOptions
            {
                ClusteredIndexes = true,
                Encoding = Encoding.UTF8,
                DriPrimaryKey = true,
                DriAllConstraints = true
            };
        }

        public void ScriptObject(Table table, Options options)
        {
            // script table without constraints
            StringCollection script = table.Script(_scriptingOptions);
            string path = _directoryService.GetTablesDirectory(options.Root);
            string fileName = Path.Combine(path, string.Concat(table.Name, ".sql"));
            _fileWriter.WriteFile(fileName, script.Cast<string>());

            // script constraints
            string foreignKeyPath = _directoryService.GetForeignKeysDirectory(options.Root);

            string addForeignKeyFileName = Path.Combine(foreignKeyPath, string.Concat(table.Name, "AddFks.sql"));
            string addForeignKeys = string.Join("\r\nGO\r\n\r\n", table.ForeignKeys.Cast<ForeignKey>().SelectMany(fk => fk.Script().Cast<string>()));

            string dropForeignKeyFileName = Path.Combine(foreignKeyPath, string.Concat(table.Name, "DropFks.sql"));
            string dropForeignKeys = string.Join("\r\nGO\r\n\r\n",
                table.ForeignKeys
                .Cast<ForeignKey>()
                .SelectMany(fk => fk.Script(new ScriptingOptions { ScriptDrops = true }).Cast<string>()));

            if (table.ForeignKeys.Cast<ForeignKey>().Any())
            {
                _fileWriter.WriteFile(addForeignKeyFileName, new [] { addForeignKeys });
                _fileWriter.WriteFile(dropForeignKeyFileName, new [] { dropForeignKeys });
            }
        }

        public void ScriptObject(View view, Options options)
        {
            StringCollection script = view.Script(_scriptingOptions);
            string path = _directoryService.GetViewsDirectory(options.Root);
            string fileName = Path.Combine(path, string.Concat(view.Name, ".sql"));
            _fileWriter.WriteFile(fileName, script.Cast<string>());
        }

        public void ScriptObject(StoredProcedure storedProcedure, Options options)
        {
            StringCollection script = storedProcedure.Script(_scriptingOptions);
            string path = _directoryService.GetStoredProceduresDirectory(options.Root);
            string fileName = Path.Combine(path, string.Concat(storedProcedure.Name, ".sql"));
            _fileWriter.WriteFile(fileName, script.Cast<string>());
        }

        public void ScriptObject(UserDefinedFunction function, Options options)
        {
            StringCollection script = function.Script(_scriptingOptions);
            string path = _directoryService.GetFunctionsDirectory(options.Root);
            string fileName = Path.Combine(path, string.Concat(function.Name, ".sql"));
            _fileWriter.WriteFile(fileName, script.Cast<string>());
        }

        public void ScriptObject(IEnumerable<Trigger> tableTriggers, string tableName, Options options)
        {
            IEnumerable<string> lines = tableTriggers.SelectMany(t => t.Script(_scriptingOptions).Cast<string>());
            string path = _directoryService.GetTriggersDirectory(options.Root);
            string fileName = Path.Combine(path, string.Concat(tableName, ".sql"));
            _fileWriter.WriteFile(fileName, lines);
        }
    }
}