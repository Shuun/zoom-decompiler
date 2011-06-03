using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using Mi.Assemblies;
using Mi.Decompiler.AstServices;
using Mi.Decompiler.Tests.Helpers;

namespace Mi.Decompiler.Tests
{
    public static class TestingLogic
    {
        static void Main()
        {
            var succeded = new List<string>();
            var failed = new List<string>();
            foreach (var t in Mi.Decompiler.Tests.TestingLogic.GetTests("..\\..\\"))
            {
                Console.Write(t.Key + "...");
                try
                {
                    t.Value();
                    Console.WriteLine(" O.K.");
                    succeded.Add(t.Key);
                }
                catch (FileNotFoundException e)
                {
                    failed.Add(t.Key);
                    var saveColor = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(e.Message);
                    Console.ForegroundColor = saveColor;
                }
                catch (MatchException matchFailure)
                {
                    failed.Add(t.Key);
                    var saveColor = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(" failed to match.");
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine(matchFailure.DiffSummary);
                    Console.ForegroundColor = saveColor;
                }
                catch (Exception error)
                {
                    failed.Add(t.Key);
                    var saveColor = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(error);
                    Console.ForegroundColor = saveColor;
                }
            }

            Console.WriteLine(failed.Count + " failed ot of " + (succeded.Count+failed.Count) + ":");
            foreach (var t in failed)
            {
                Console.WriteLine(" " + t);
            }

            var file = new FileInfo(typeof(TestingLogic).Assembly.Location);
            Console.WriteLine("Created: " + (file.CreationTime > file.LastWriteTime ? file.CreationTime : file.LastWriteTime) + ".");
        }

        public static IEnumerable<KeyValuePair<string, Action>> GetTests(string path)
        {
            string testDll = typeof(TestingLogic).Assembly.Location;
            string testPdb = Path.ChangeExtension(testDll, ".pdb");

            var para = new ReaderParameters(ReadingMode.Immediate)
            {
                AssemblyResolver = new AssemblyResolver(),
                ReadSymbols = true,
                SymbolReaderProvider = new Mi.Assemblies.Pdb.PdbReaderProvider(),
                SymbolStream = new MemoryStream(File.ReadAllBytes(testPdb))
            };

            var asm = Assemblies.AssemblyDefinition.ReadAssembly(
                typeof(TestingLogic).Assembly.Location,
                para);

            var allCodeFiles =
                from f in Directory.GetFiles(path, "*.cs", SearchOption.AllDirectories)
                let code = File.ReadAllText(f)
                group code by Path.GetFileNameWithoutExtension(f);

            var allSampleClasses =
                from t in asm.MainModule.Types
                where t.Name.All(c=>char.IsLetterOrDigit(c) || c=='`')
                where string.IsNullOrEmpty(t.Namespace)
                || (!t.Namespace.StartsWith(typeof(TestingLogic).Namespace)
                && !t.Namespace.StartsWith(typeof(DiffLib.ChangeType).Namespace))
                select t;

            var testCases = new Dictionary<string, List<TypeDefinition>>();
            foreach (var t in allSampleClasses)
	        {
                if(string.IsNullOrEmpty(t.Namespace))
                {
                    testCases.Add(t.Name, new List<TypeDefinition> { t });
                }
                else
                {
                    string testCase = t.Namespace.Split('.').First();

                    List<TypeDefinition> testCaseTypes;
                    if(!testCases.TryGetValue(testCase, out testCaseTypes))
                        testCases[testCase] = testCaseTypes = new List<TypeDefinition>();

                    testCaseTypes.Add(t);
                }
	        }

            return
                from tc in testCases
                orderby tc.Key
                select new KeyValuePair<string, Action>(
                    tc.Key,
                    () => RunTest(tc.Key, asm, tc.Value, allCodeFiles));
        }

        private static void RunTest(string testCase, Assemblies.AssemblyDefinition asm, IEnumerable<TypeDefinition> types, IEnumerable<IGrouping<string,string>> matchingCode)
        {
            if (testCase == "Generics")
                testCase.GetHashCode();

            string decompiledText = Decompile(asm, types);

            IEnumerable<string> matchingFiles =
                matchingCode.FirstOrDefault(kv => string.Equals(kv.Key, testCase, StringComparison.OrdinalIgnoreCase));

            if (matchingFiles == null
                || matchingFiles.Count()==0)
                throw new FileNotFoundException("No source code found for " + testCase + ".");

            if (matchingFiles.Count() != 1)
                throw new AmbiguousMatchException("More than one source code file found for " + testCase + ".");

            string originalCode = matchingFiles.Single();


            string noComments = CodeSampleFileParser.ConcatLines(
                from line in SplitLines(originalCode)
                where !CodeSampleFileParser.IsIgnorableLine(line)
                select line);

            CodeAssert.AreEqual(noComments, decompiledText);
        }

        static IEnumerable<string> SplitLines(string text)
        {
            using (var reader = new StringReader(text))
            {
                while (true)
                {
                    string line = reader.ReadLine();

                    if (line == null)
                        yield break;
                    else
                        yield return line;
                }
            }
        }

        private static string Decompile(Assemblies.AssemblyDefinition asm, IEnumerable<TypeDefinition> types)
        {
            DecompilerSettings settings = new DecompilerSettings();
            settings.FullyQualifyAmbiguousTypeNames = false;
            var ctx = new DecompilerContext(asm.MainModule) { Settings = settings };
            var decompiler = new AstBuilder(ctx);
            foreach (var t in types)
            {
                decompiler.AddType(t);
            }
            new Helpers.RemoveCompilerAttribute().Run(decompiler.CompilationUnit);
            var output = new StringWriter();
            decompiler.GenerateCode(new PlainTextOutput(output));
            return output.ToString().Trim();
        }
    }
}