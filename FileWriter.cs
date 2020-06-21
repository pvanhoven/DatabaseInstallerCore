using System;
using System.Collections.Generic;
using System.IO;

namespace DatabaseInstallerCore
{
    public class FileWriter
    {
        private static string Separator = string.Format("{0}GO{0}", Environment.NewLine);

        public void WriteFile(string path, IEnumerable<string> lines)
        {
            using(StreamWriter streamWriter = new StreamWriter(path))
            {
                string script = string.Join(Separator, lines);
                streamWriter.Write(script);
            }
        }
    }
}