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

            foreach (var peFile in Directory.EnumerateFiles(Environment.GetFolderPath(Environment.SpecialFolder.System)))
            {
                if (".exe.dll".IndexOf(Path.GetExtension(peFile), StringComparison.OrdinalIgnoreCase) < 0)
                    continue;

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