using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.Ast;
using Mono.Cecil;
using System.IO;

namespace ILSpySL
{
    public partial class MainPage : UserControl
    {
        private sealed class Resolver : IAssemblyResolver
        {
            readonly Func<AssemblyNameReference, ReaderParameters, AssemblyDefinition> resolve;

            public Resolver(Func<AssemblyNameReference, ReaderParameters, AssemblyDefinition> resolve)
            {
                this.resolve = resolve;
            }

            public AssemblyDefinition Resolve(AssemblyNameReference name)
            {
                return Resolve(name, null);
            }

            public AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
            {
                return this.resolve(name, parameters);
            }

            public AssemblyDefinition Resolve(string fullName)
            {
                return Resolve(fullName, null);
            }

            public AssemblyDefinition Resolve(string fullName, ReaderParameters parameters)
            {
                return Resolve(AssemblyNameReference.Parse(fullName), parameters);
            }
        }

        readonly List<AssemblyDefinition> assembies = new List<AssemblyDefinition>();

        public MainPage()
        {
            InitializeComponent();
        }

        private void openButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() != true)
                return;

            var buf = new MemoryStream();
            using (var stream = openFileDialog.File.OpenRead())
            {
                while(true)
                {
                    int b = stream.ReadByte();
                    if(b<0)
                        break;
                    else
                        buf.WriteByte((byte)b);
                }
            }

            buf.Position = 0;

            openButton.IsEnabled = false;

            System.Threading.ThreadPool.QueueUserWorkItem(delegate
            {
                try
                {
                    var para = new Mono.Cecil.ReaderParameters(Mono.Cecil.ReadingMode.Immediate);
                    Mono.Cecil.AssemblyDefinition asm;
                        para.AssemblyResolver = new Resolver(
                            (asmName, _p) =>
                                assembies.FirstOrDefault(
                                    loaded =>
                                        string.Equals(
                                        loaded.FullName,
                                        asmName.FullName,
                                        StringComparison.OrdinalIgnoreCase)));

                    asm = Mono.Cecil.AssemblyDefinition.ReadAssembly(buf, para);

                    this.assembies.Add(asm);

                    this.Dispatcher.BeginInvoke(delegate
                    {
                        this.treeView1.Items.Add(new TreeViewItem
                        {
                            Header = asm.Name.Name + " v" + asm.Name.Version
                        });
                    });

                    var ctx = new DecompilerContext(asm.MainModule);
                    var astBui = new AstBuilder(ctx);
                    astBui.AddAssembly(asm);
                    astBui.RunTransformations();
                    var outp = new PlainTextOutput();
                    astBui.GenerateCode(outp);

                    this.Dispatcher.BeginInvoke(delegate
                    {
                        openButton.IsEnabled = true;
                        var rn = new Run { Text = outp.ToString() };
                        var pa = new Paragraph();
                        pa.Inlines.Add(rn);

                        codeTextBox.Blocks.Clear();
                        codeTextBox.Blocks.Add(pa);
                    });
                }
                catch(Exception error)
                {
                    this.Dispatcher.BeginInvoke(delegate
                    {
                        openButton.IsEnabled = true;
                        var rn = new Run { Text = error.ToString() };
                        var pa = new Paragraph();
                        pa.Inlines.Add(rn);

                        codeTextBox.Blocks.Clear();
                        codeTextBox.Blocks.Add(pa);
                    });
                }
            });
        }
    }
}
