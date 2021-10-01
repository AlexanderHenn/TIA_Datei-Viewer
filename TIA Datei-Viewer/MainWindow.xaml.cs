using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Schema;
using Microsoft.Win32;

namespace TIA_Datei_Viewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TiaFile tiaFile;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            //delete UI elements for old file
            NodeList.Items.Clear();
            TypeButtons.Children.Clear();
            //open new file
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "TIA (*.tia)|*.tia|All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                // create tiaFile and Validate
                tiaFile = new TiaFile(openFileDialog.FileName);

                if (!tiaFile.Validate())
                {
                    MessageBox.Show("Could not load TIA file. Detailed Error: " + tiaFile.ValidationMessage);
                    return;
                }
                else
                {
                    // we have a valid tiaFile now
                    // create UI (possibly should be a separate function)

                    this.Title = "TIA Selection Tool - Datei Viewer - \"" + openFileDialog.SafeFileName + "\"";
                    //create new Buttons with the Type names on them
                    List<string> types = tiaFile.LoadTypes();
                    int i = 0;
                    foreach (string type in types)
                    {
                        Button button = new Button()
                        {
                            Content = type,
                            Margin = new Thickness(10),
                            Padding = new Thickness(10, 0, 10, 0),
                            Tag = i++
                        };
                        button.Click += new RoutedEventHandler(TypeName_Click);
                        TypeButtons.Children.Add(button);
                    }
                }
            }
        }

        void ValidationHandler(object sender, ValidationEventArgs e)
        {
            if (e.Severity == XmlSeverityType.Warning)
            {
                MessageBox.Show
                    ("The following validation warning occurred: " + e.Message);
            }
            else if (e.Severity == XmlSeverityType.Error)
            {
                MessageBox.Show
                    ("The following critical validation errors occurred: " + e.Message);
                Type objectType = sender.GetType();
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
            List<string> nodes = tiaFile.LoadType(type);
            foreach (string node in nodes)
            {
                NodeList.Items.Add(node);
            }

        }

    }
}