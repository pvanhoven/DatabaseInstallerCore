using System;
using CommandLine;
using CommandLine.Text;

namespace DatabaseInstallerCore
{
    class Program
    {
        static void Main(string[] args)
        {
            var parserResult = CommandLine.Parser.Default.ParseArguments<Options>(args);
            parserResult
                .WithParsed(options =>
                {
                    if (options.Help)
                    {
                        PrintHelp(parserResult);
                        return;
                    }

                    if (options.Export)
                    {
                        TextFileSchemaExporter exporter =
                            new TextFileSchemaExporter(new DbScripter(new FileWriter(), new DirectoryService()), new DirectoryService());

                        exporter.Export(options);
                        return;
                    }

                    if (options.Install)
                    {
                        Installer installer = new Installer();
                        installer.Create(options);
                        return;
                    }

                    PrintHelp(parserResult);
                }).WithNotParsed(errors =>
                {
                    PrintHelp(parserResult);
                    return;
                });
        }

        private static void PrintHelp(ParserResult<Options> parserResult)
        {
            // Passing empty funcs below bypasses bug where exception gets thrown here
            HelpText helpText = HelpText.AutoBuild(parserResult, _ => _, _ => _);
            Console.WriteLine(helpText.ToString());
        }
    }
}