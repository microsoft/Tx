// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using Microsoft.Win32;

namespace Tx.LinqPad
{
    /// <summary>
    ///     Interaction logic for ConnectionDialog.xaml
    /// </summary>
    public partial class ConnectionDialog : Window
    {
        private const string ErrorMessageTitle = "Tx LINQPad Driver";
        private readonly ObservableCollection<string> _files;
        private readonly string _filter;
        private readonly ObservableCollection<string> _metadataFiles;
        private readonly TxProperties _properties;

        internal ConnectionDialog(TxProperties properties, string filter)
        {
            DataContext = properties;
            _properties = properties;
            _filter = filter;

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
                MessageBox.Show("The connection name can not be empty", ErrorMessageTitle);
                return;
            }

            if (rbPast.IsChecked.Value)
            {
                if (_files.Count == 0)
                {
                    MessageBox.Show("Empty list of files", ErrorMessageTitle);
                    return;
                }
            }

            _properties.Files = _files.ToArray();
            _properties.MetadataFiles = _metadataFiles.ToArray();
            _properties.IsRealTime = rbRealTime.IsChecked.Value;
            _properties.IsUsingDirectoryLookup = rbLookup.IsChecked.Value;
            DialogResult = true;
        }

        private void AddFiles_Click(object sender, RoutedEventArgs e)
        {
            var fileDialog = new OpenFileDialog
                {
                    Title = "Add file to the query context",
                    Multiselect = true,
                    Filter = _filter
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
            var removed = new List<string>();
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
            var fileDialog = new OpenFileDialog
                {
                    Title = "Select Data Collector Set Template",
                    Multiselect = false,
                    Filter = "Data Collector Set Templates (*.xml)|*.xml"
                };
        }

        private static T GetAttribute<T>(ICustomAttributeProvider provider)
        {
            return (T) (provider.GetCustomAttributes(typeof (T), false))[0];
        }

        private void SwitchPastOrRealTime(object sender, RoutedEventArgs e)
        {
            if (!IsInitialized)
                return;

            if (rbRealTime.IsChecked.Value)
            {
                PastUI.Visibility = Visibility.Collapsed;
                RealTimeUI.Visibility = Visibility.Visible;
            }
            else
            {
                PastUI.Visibility = Visibility.Visible;
                RealTimeUI.Visibility = Visibility.Collapsed;
            }

            LookupOrSelectMetadata_Checked(sender, e);
        }

        private void LookupOrSelectMetadata_Checked(object sender, RoutedEventArgs e)
        {
            if (!IsInitialized)
                return;

            if (rbLookup.IsChecked.Value)
            {
                LookupDirPanel.Visibility = Visibility.Visible;
                AddMetadataPanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                LookupDirPanel.Visibility = Visibility.Collapsed;
                AddMetadataPanel.Visibility = Visibility.Visible;
            }
        }

        private void AddMetadataFiles_Click(object sender, RoutedEventArgs e)
        {
            var fileDialog = new OpenFileDialog
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
            var removed = new List<string>();
            foreach (string name in MetadataFileList.SelectedItems)
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