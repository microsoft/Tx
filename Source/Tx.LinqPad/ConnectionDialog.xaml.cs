using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Reflection;
using System.IO;

namespace Tx.LinqPad
{
    /// <summary>
    /// Interaction logic for ConnectionDialog.xaml
    /// </summary>
    public partial class ConnectionDialog : Window
    {
        ObservableCollection<string> _files;
        ObservableCollection<string> _metadataFiles;
        TxProperties _properties;

        internal ConnectionDialog(TxProperties properties)
        {

            this.DataContext = properties;
            _properties = properties;
            InitializeComponent();

            _files = new ObservableCollection<string>(_properties.Files);
            _metadataFiles = new ObservableCollection<string>(_properties.MetadataFiles);

            FileList.ItemsSource = _files;
            MetadataFileList.ItemsSource = _metadataFiles;
            rbLookup.IsChecked = _properties.IsUsingDirectoryLookup;
            rbRealTime.IsChecked = _properties.IsRealTime;
        }

        private void ClickOK(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrWhiteSpace(_properties.ContextName))
            {
                System.Windows.MessageBox.Show("The connection name can not be empty", "TraceInsight");
                return;
            }

            if (rbPast.IsChecked.Value)
            {
                if (_files.Count == 0)
                {
                    System.Windows.MessageBox.Show("Empty list of files", "TraceInsight");
                    return;
                }
            }
            
            _properties.Files = _files.ToArray();
            _properties.MetadataFiles = _metadataFiles.ToArray();
            _properties.IsRealTime = rbRealTime.IsChecked.Value;
            _properties.IsUsingDirectoryLookup = rbLookup.IsChecked.Value;
            this.DialogResult = true;
        }

        private void AddFiles_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog()
            {
                Title = "Add file to the query context",
                Multiselect = true,
                Filter = ParserRegistry.Filter
            };
            if (fileDialog.ShowDialog().GetValueOrDefault())
            {
                foreach (string name in fileDialog.FileNames)
                {
                    _files.Add(name);
                }
            }
        }

        private void RemoveFiles_Click(object sender, RoutedEventArgs e)
        {
            List<string> removed = new List<string>();
            foreach (string name in FileList.SelectedItems)
            {
                removed.Add(name);
            }
            foreach (string name in removed)
            {
                _files.Remove(name);
            }
        }

        private void btnBrowseDefinition_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog()
            {
                Title = "Select Data Collector Set Template",
                Multiselect = false,
                Filter = "Data Collector Set Templates (*.xml)|*.xml"
            };
        }

        static T GetAttribute<T>(ICustomAttributeProvider provider)
        {
            return (T)(provider.GetCustomAttributes(typeof(T), false))[0];
        }

        private void SwitchPastOrRealTime(object sender, RoutedEventArgs e)
        {
            if (!IsInitialized)
                return;

            if (rbRealTime.IsChecked.Value)
            {
                PastUI.Visibility = System.Windows.Visibility.Collapsed;
                RealTimeUI.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                PastUI.Visibility = System.Windows.Visibility.Visible;
                RealTimeUI.Visibility = System.Windows.Visibility.Collapsed;
            }

            LookupOrSelectMetadata_Checked(sender, e);
        }

        private void LookupOrSelectMetadata_Checked(object sender, RoutedEventArgs e)
        {
            if (!IsInitialized)
                return;

            if (rbLookup.IsChecked.Value)
            {
                LookupDirPanel.Visibility = System.Windows.Visibility.Visible;
                AddMetadataPanel.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                LookupDirPanel.Visibility = System.Windows.Visibility.Collapsed;
                AddMetadataPanel.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void AddMetadataFiles_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog()
            {
                Title = "Add metadata files",
                Multiselect = true,
                Filter = "All Files|*.man|Manifests|*.man;"
            };
            if (fileDialog.ShowDialog().GetValueOrDefault())
            {
                foreach (string name in fileDialog.FileNames)
                {
                    _metadataFiles.Add(name);
                }
            }
        }

        private void RemoveMetadataFiles_Click(object sender, RoutedEventArgs e)
        {
            List<string> removed = new List<string>();
            foreach (string name in FileList.SelectedItems)
            {
                removed.Add(name);
            }
            foreach (string name in removed)
            {
                _metadataFiles.Remove(name);
            }
        }
    }
}
