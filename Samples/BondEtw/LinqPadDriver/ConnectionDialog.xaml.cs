//-----------------------------------------------------------------------
// <copyright file="ConnectionDialog.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace BondInEtwDriver
{
    using Microsoft.Win32;
    using System;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text;
    using System.Windows;

    using BondInEtwLinqpadDriver;

    using Tx.Bond.Extensions;

    /// <summary>
    /// Interaction logic for ConnectionDialog.xaml
    /// </summary>
    public partial class ConnectionDialog
    {
        /// <summary>
        /// Driver's properties object.
        /// </summary>
        private readonly BondInEtwProperties bondInEtwProperties;

        /// <summary>
        /// Dynamic list to keep track of removed/added files.
        /// </summary>
        private readonly ObservableCollection<string> fileList;

        /// <summary>
        /// Inistantiates an instance of the <see cref="ConnectionDialog"/> class.
        /// </summary>
        /// <param name="bondInEtwProperties"></param>
        public ConnectionDialog(BondInEtwProperties bondInEtwProperties)
        {
            this.DataContext = bondInEtwProperties;
            this.bondInEtwProperties = bondInEtwProperties;

            this.InitializeComponent();

            this.fileList = new ObservableCollection<string>(bondInEtwProperties.Files);

            this.FileList.ItemsSource = this.fileList;
        }

        /// <summary>
        /// Add files event handler.
        /// </summary>
        /// <param name="sender"> Sender. </param>
        /// <param name="e"> Routed event args. </param>
        private void AddFiles_OnClick(object sender, RoutedEventArgs e)
        {
            var fileDialog = new OpenFileDialog
                {
                    Title = "Add Bond-In-ETW file(s)",
                    Multiselect = true,
                    Filter = "Bond ETW traces|*.etl"
                };

            if (!fileDialog.ShowDialog().GetValueOrDefault())
            {
                return;
            }

            foreach (var fileName in fileDialog.FileNames)
            {
                this.fileList.Add(fileName);
            }
        }

        /// <summary>
        /// Remove files event handler.
        /// </summary>
        /// <param name="sender"> sender. </param>
        /// <param name="e"> Routed event args. </param>
        private void RemoveFiles_OnClick(object sender, RoutedEventArgs e)
        {
            var removedFiles = this.FileList.SelectedItems.Cast<string>().ToList();

            foreach (var removedFile in removedFiles)
            {
                this.fileList.Remove(removedFile);
            }
        }

        /// <summary>
        /// Click OK event handler.
        /// </summary>
        /// <param name="sender"> sender </param>
        /// <param name="e"> Routed event args. </param>
        private void ButtonOk_OnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(this.bondInEtwProperties.ContextName))
            {
                MessageBox.Show("Please enter a connection name", "Bond-In-ETW driver");
            }
            else
            {
                this.bondInEtwProperties.Files = this.fileList.ToArray();
                this.DialogResult = true;
            }
        }
    }
}
