using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Automation;
using System.IO;
using System.Collections.Generic;
using System.Windows;
using System.Reflection;

namespace UITest
{
    class LinqPadAutomation : IDisposable
    {
        Process _process;
        AutomationElement _mainWindow;
        AutomationElement _samplesTree;
        string _exportDirectory;
        string _baselineDirectory;

        public LinqPadAutomation(string linqpadPath, string sampleSet, string exportDirectory)
        {
            if (String.IsNullOrWhiteSpace(linqpadPath))
                throw new ArgumentException("linqpadPath is null or empty");

            if (String.IsNullOrWhiteSpace(sampleSet))
                throw new ArgumentException("sampleSet is null or empty");

            if (String.IsNullOrWhiteSpace(exportDirectory))
                throw new ArgumentException("exportDirectory is null or empty");

            _exportDirectory = Path.Combine(Environment.CurrentDirectory, exportDirectory);

            _baselineDirectory = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "baseline");

            if (!Directory.Exists(_baselineDirectory))
            {
                _baselineDirectory = null;
            }

            _process = Process.Start(linqpadPath);

            var desktop = AutomationElement.RootElement;
            _mainWindow = desktop.WaitForChild("LINQPad 4", TimeSpan.FromSeconds(10));

            var samplesTab = _mainWindow
                .FindInDepth(ControlType.Pane, 4)
                .FindChildByIndex(ControlType.Pane, 1)
                .FindChildByAutomationId("tcQueryTrees")
                .FindChildByName("Samples");
            samplesTab.SetFocus();

            _samplesTree = samplesTab
                .FindChildByAutomationId("sampleQueries")
                .FindChildByName(sampleSet);
            _samplesTree.Expand();
        }

        public AutomationElementCollection GetSampleGroups()
        {
            var samplesTab = _mainWindow
                .FindInDepth(ControlType.Pane, 4)
                .FindChildByIndex(ControlType.Pane, 1)
                .FindChildByAutomationId("tcQueryTrees")
                .FindChildByName("Samples");
            samplesTab.SetFocus();

            return _samplesTree.FindAllChildren(ControlType.TreeItem);
         }

        public AutomationElementCollection GetSamplesInGroup(AutomationElement group)
        {
            group.Expand();
            return group.FindAllChildren(ControlType.TreeItem);
        }

        public bool ExecuteQuery(AutomationElement querySet, AutomationElement queryNode)
        {
            string exportDir = Path.Combine(_exportDirectory, querySet.GetName());
            if (!Directory.Exists(exportDir))
            {
                Directory.CreateDirectory(exportDir);
            }

            string fileName = Path.Combine(exportDir, queryNode.GetName() + ".html");
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            queryNode.AddToSelection();

            var queryEditor = _mainWindow.FindChildByAutomationId("verticalSplit")
                .FindChildByIndex(ControlType.Pane, 1)
                .FindChildByAutomationId("tcQueries")
                .FindChildByName(queryNode.GetName());
            queryEditor.SetFocus();

            var btnPin = queryEditor
                .FindChildByAutomationId("QueryControl")
                .FindChildByAutomationId("panTop")
                .FindChildByAutomationId("btnPin");
            
            //Mouse.Click(btnPin.GetClickablePoint().ToDrawingPoint());

            var btnExecute = queryEditor
                .FindChildByAutomationId("QueryControl")
                .FindChildByAutomationId("panTop")
                .FindChildByAutomationId("btnExecute");
            btnExecute.Invoke();

            // sleep just enough to start the query and show the status bar
            Thread.Sleep(1000); 

            // wait for the status bar to say "Query sucessful"
            var panBottom = queryEditor
                .FindChildByAutomationId("QueryControl")
                .FindChildByAutomationId("panMain")
                .FindChildByAutomationId("splitContainer")
                .FindChildByIndex(ControlType.Pane, 1)
                .FindChildByAutomationId("panBottom");

            while (true)
            {
                var statusStrip = queryEditor
                    .FindChildByAutomationId("QueryControl")
                    .FindChildByAutomationId("statusStrip");

                if (statusStrip.FindChildByName("Query successful") !=null )
                    break;

                Thread.Sleep(100);
            }

            // export to html
            var mnuExport = panBottom
                .FindChildByName("toolStrip1")
                .FindChildByName("Export");
            mnuExport.Invoke();

            var mnuExportToHtml = mnuExport.FindChildByName("Export to HTML");
            mnuExportToHtml.Invoke();

            var dialog = _mainWindow.WaitForChild("Save Results", TimeSpan.FromSeconds(10));
            var editFilename = dialog
                .FindChildByIndex(ControlType.Pane, 0)
                .FindChildByAutomationId("FileNameControlHost")
                .FindChildByIndex(ControlType.Edit, 0);
            var save = dialog.FindChildByName("Save");

            editFilename.SetText(fileName);
            save.Invoke();
            _mainWindow.WaitForChildToDissapear("Save Results", TimeSpan.FromSeconds(10));

            if (_baselineDirectory != null)
            {
                string baseline = Path.Combine(_baselineDirectory, querySet.GetName(), queryNode.GetName() + ".html");

                string result = File.ReadAllText(fileName);
                string expected = File.ReadAllText(baseline);
                if (result != expected)
                {
                    return false;
                }

                Console.Write("...baseline matches...");
            }

            return true;
        }

        public void Dispose()
        {
            if (null != _process)
            {
                _process.Kill();
                _process = null;
            }
        }
    }
}
