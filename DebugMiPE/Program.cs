using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Mi.PE;

namespace DebugMiPE
{
    class Program
    {
        static void Main(string[] args)
        {
            var reader = new PEFileReader();

            var sources =
                from file in Directory.EnumerateFiles(Environment.GetFolderPath(Environment.SpecialFolder.System))
                where ".exe.dll".IndexOf(Path.GetExtension(file), StringComparison.OrdinalIgnoreCase) < 0
                select file;

            sources = new[] { typeof(Program).Assembly.Location };

            foreach (var peFile in sources)
            {
                Stream stream;

                try
                {
                    stream = File.OpenRead(peFile);
                }
                catch(Exception error)
                {
                    continue;
                }

                using(stream)
                {
                    var pe = reader.Read(stream);
                
                }
            }
        }
    }
}