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

            this.AllowDrop = true;
            this.Drop += new DragEventHandler(MainPage_Drop);
        }

        private void openButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog { Multiselect = true };
            if (openFileDialog.ShowDialog() != true)
                return;

            openButton.IsEnabled = false;

            var files = openFileDialog.Files;

            AddAssemblies(files);
        }

        private void AddAssemblies(IEnumerable<FileInfo> files)
        {
            var bufs = new List<MemoryStream>();
            foreach (var f in files)
            {
                var buf = new MemoryStream();
                using (var stream = f.OpenRead())
                {
                    while (true)
                    {
                        int b = stream.ReadByte();
                        if (b < 0)
                            break;
                        else
                            buf.WriteByte((byte)b);
                    }
                }

                buf.Position = 0;

                bufs.Add(buf);
            }

            System.Threading.ThreadPool.QueueUserWorkItem(delegate
            {
                foreach (var buf in bufs)
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
                            var asmNode = new TreeViewItem
                            {
                                Header = asm.Name.Name + " v" + asm.Name.Version
                            };
                            this.treeView1.Items.Add(asmNode);

                            var grouped =
                                from t in asm.MainModule.Types
                                orderby t.Namespace, t.Name
                                group t by t.Namespace;

                            foreach (var g in grouped)
                            {
                                var nsNode = new TreeViewItem
                                {
                                    Header = string.IsNullOrEmpty(g.Key) ? "<>" : g.Key
                                };

                                asmNode.Items.Add(nsNode);

                                foreach (var t in g)
                                {
                                    var tNode = new TreeViewItem
                                    {
                                        Header = t.Name,
                                        Tag = t
                                    };

                                    nsNode.Items.Add(tNode);
                                }
                            }

                            openButton.IsEnabled = true;
                        });
                    }
                    catch (Exception error)
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
                }
            });
        }

        private void treeView1_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            DecompileSelectedNode();
        }

        private void DecompileSelectedNode()
        {
            if (treeView1.SelectedItem == null
                || !(((TreeViewItem)treeView1.SelectedItem).Tag is TypeDefinition))
            {
                codeTextBox.Blocks.Clear();
                return;
            }

            var ty = (TypeDefinition)(((TreeViewItem)treeView1.SelectedItem).Tag);

            var ctx = new DecompilerContext(ty.Module);
            var astBui = new AstBuilder(ctx);
            astBui.AddType(ty);
            astBui.RunTransformations();
            var outp = new PlainTextOutput();
            astBui.GenerateCode(outp);

            var rn = new Run { Text = outp.ToString() };
            var pa = new Paragraph();
            pa.Inlines.Add(rn);

            codeTextBox.Blocks.Clear();
            codeTextBox.Blocks.Add(pa);
        }

        void MainPage_Drop(object sender, DragEventArgs e)
        {
            var droppedFiles = e.Data.GetData(DataFormats.FileDrop) as FileInfo[];

            if (droppedFiles == null)
                return;

            AddAssemblies(droppedFiles);
        }
    }
}
