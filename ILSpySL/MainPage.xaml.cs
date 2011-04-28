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

namespace ILSpySL
{
    public partial class MainPage : UserControl
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void openButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() != true)
                return;

            Mono.Cecil.ModuleDefinition module;
            using (var stream = openFileDialog.File.OpenRead())
            {
                module = Mono.Cecil.ModuleDefinition.ReadModule(stream);
            }
            DecompilerContext decompiler = new DecompilerContext(module);
        }
    }
}
