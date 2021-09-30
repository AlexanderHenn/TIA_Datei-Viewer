using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using Microsoft.Win32;

namespace TIA_Datei_Viewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        TiaFile tiaFile;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "TIA (*.tia)|*.tia|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                tiaFile = new TiaFile(openFileDialog.FileName);
                this.Title = "TIA Selection Tool - Datei Viewer - \"" + openFileDialog.SafeFileName + "\"";
            }


            List<string> types = tiaFile.TypeButtonNames();
            
            //create Buttons with the names on them
            //TODO arrange buttons better (as it currently is, pretty terrible)
            int i = 10;
            int j = 0;
            foreach (string type in types) {
                //RadioButton would have made sense in terms of functionality, but making them look like normal Buttons seems hard
                Button button = new Button()
                {
                    Content = type,
                    Margin = new Thickness(i, 10, 0, 0),

                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    Tag = j++
                };
                button.Click += new RoutedEventHandler(TypeName_Click);

                this.grid.Children.Add(button);
                i = i + type.Length *8;
            }
        }

        // fill the ListBox with nodes
        private void TypeName_Click(object sender, RoutedEventArgs e)
        {
            //emtpy the current List
            NodeList.Items.Clear();
            //identify button
            int type = (int) ((Button)sender).Tag;

            //get nodes and fill the list with them
            List<string> nodes = tiaFile.NodesOfType(type);
            foreach (string node in nodes)
            {
                NodeList.Items.Add(node);
            }

        }

    }
}