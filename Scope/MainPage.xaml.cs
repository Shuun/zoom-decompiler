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
using Mi.Decompiler;
using Mi.Decompiler.Ast;
using Mi.Assemblies;
using System.IO;

namespace Mi.Scope
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

            newVersionAlert.Visibility = System.Windows.Visibility.Visible;
            newVersionAlert.Text = "v" + new System.Reflection.AssemblyName(typeof(MainPage).Assembly.FullName).Version+"...";
            if (Application.Current.IsRunningOutOfBrowser)
            {
                Application.Current.CheckAndDownloadUpdateCompleted+=(sender, e) =>
                {
                    this.Dispatcher.BeginInvoke(() =>
                        {
                            if (e.UpdateAvailable)
                            {
                                newVersionAlert.Text = "Newer version available (restart)";
                                newVersionAlert.Foreground = new SolidColorBrush(Colors.Blue);
                            }
                            else if (e.Error != null)
                            {
                                newVersionAlert.Text = "Version check: " + e.Error.Message;
                                newVersionAlert.Visibility = System.Windows.Visibility.Visible;
                            }
                            else
                            {
                                newVersionAlert.Text = "v" + new System.Reflection.AssemblyName(typeof(MainPage).Assembly.FullName).Version;
                            }
                        });
                };

                Application.Current.CheckAndDownloadUpdateAsync();
            }
            else
            {
                newVersionAlert.Text = "v" + new System.Reflection.AssemblyName(typeof(MainPage).Assembly.FullName).Version+ " (online)";
            }
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
                        var para = new Mi.Assemblies.ReaderParameters(Mi.Assemblies.ReadingMode.Immediate);
                        Mi.Assemblies.AssemblyDefinition asm;
                        para.AssemblyResolver = new Resolver(
                            (asmName, _p) =>
                                assembies.FirstOrDefault(
                                    loaded =>
                                        string.Equals(
                                        loaded.FullName,
                                        asmName.FullName,
                                        StringComparison.OrdinalIgnoreCase)));

                        asm = Mi.Assemblies.AssemblyDefinition.ReadAssembly(buf, para);

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

        private sealed class RichTextOutput : ITextOutput
        {
            readonly Brush KeywordBrush = new SolidColorBrush(Colors.DarkGray);
            readonly Brush IdentifierBrush = new SolidColorBrush(Colors.Blue);

            readonly List<Paragraph> blocks = new List<Paragraph>();
            int indentCount = 0;
            bool newLine = true;

            public int CurrentLine { get { return blocks.Count; } }

            public void Indent() { indentCount++; }
            public void Unindent() { indentCount--; }

            public void Write(char ch)
            {
                Write(ch.ToString());
            }

            public void WriteKeyword(string text)
            {
                CheckCompleteLine();

                this.blocks.Last().Inlines.Add(
                    new Run { Text = text, Foreground = KeywordBrush });
            }

            public void WriteIdentifier(string text)
            {
                CheckCompleteLine();

                this.blocks.Last().Inlines.Add(
                    new Run { Text = text, Foreground = IdentifierBrush, FontWeight = FontWeights.Bold });
            }

            public void Write(string text)
            {
                CheckCompleteLine();

                this.blocks.Last().Inlines.Add(
                    new Run { Text = text });
            }

            public void WriteLine()
            {
                if (newLine)
                    CheckCompleteLine();

                newLine = true;
            }

            public void WriteDefinition(string text, object definition)
            {
                CheckCompleteLine();

                this.blocks.Last().Inlines.Add(
                    new Run { Text = text, Foreground = new SolidColorBrush(Colors.Blue) });
            }

            public void WriteReference(string text, object reference)
            {
                CheckCompleteLine();

                var bold = new Bold();
                bold.Inlines.Add(new Run { Text = text });
                this.blocks.Last().Inlines.Add(bold);
            }

            public void MarkFoldStart(string collapsedText = "...", bool defaultCollapsed = false)
            {
            }

            public void MarkFoldEnd()
            {
            }

            void CheckCompleteLine()
            {
                if (newLine)
                {
                    var newPara = new Paragraph { };
                    if (this.indentCount > 0)
                    {
                        newPara.Inlines.Add(new Run { Text = new string('\t', this.indentCount) });
                    }
                    blocks.Add(newPara);
                    newLine = false;
                }
            }

            public IEnumerable<Block> GetBlocks()
            {
                foreach (var b in this.blocks)
                {
                    yield return b;
                }
            }
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
            var outp = new RichTextOutput();
            astBui.GenerateCode(outp);

            //var rn = new Run { Text = outp.ToString() };
            //var pa = new Paragraph();
            //pa.Inlines.Add(rn);

            codeTextBox.Blocks.Clear();
            //codeTextBox.Blocks.Add(pa);

            foreach (var b in outp.GetBlocks())
            {
                codeTextBox.Blocks.Add(b);
            }

            codeTextBox.Selection.Select(codeTextBox.ContentStart, codeTextBox.ContentStart);
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
