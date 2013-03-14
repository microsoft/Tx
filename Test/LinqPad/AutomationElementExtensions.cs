using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Automation;

namespace UITest
{
    public static class AutomationElementExtensions
    {
        public static AutomationElement FindChildByName(this AutomationElement root, string name)
        {
            return root.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.NameProperty, name));
        }

        public static AutomationElement FindChildByAutomationId(this AutomationElement root, string id)
        {
            return root.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.AutomationIdProperty, id));
        }

        public static AutomationElement FindChildByIndex(this AutomationElement root, ControlType type, int index)
        {
            AutomationElementCollection controls = root.FindAll(TreeScope.Children, new PropertyCondition(AutomationElement.ControlTypeProperty, type));
            return controls[index];
        }

        public static AutomationElement FindInDepth(this AutomationElement root, ControlType type, int levels)
        {
            AutomationElement result = root;
            for (int i = 0; i < levels; i++)
            {
                Thread.Sleep(100);
                result = result.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.ControlTypeProperty, type));
            }
            return result;
        }

        public static AutomationElement WaitForChild(this AutomationElement root, string childName, TimeSpan timeout)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            while (true)
            {
                AutomationElement child = root.FindChildByName(childName);
                if (child != null)
                    return child;

                TimeSpan elapsed = stopwatch.Elapsed;
                if (elapsed >= timeout)
                {
                    return null;
                }

                Thread.Sleep(100);
            }
        }

        public static void WaitForChildToDissapear(this AutomationElement root, string childName, TimeSpan timeout)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            while (true)
            {
                AutomationElement child = root.FindChildByName(childName);
                if (child == null)
                    return;

                TimeSpan elapsed = stopwatch.Elapsed;
                if (elapsed >= timeout)
                {
                    return;
                }

                Thread.Sleep(100);
            }
        }

        public static AutomationElementCollection FindAllChildren(this AutomationElement root, ControlType type)
        {
            return root.FindAll(TreeScope.Children, new PropertyCondition(AutomationElement.ControlTypeProperty, type));
        }

        public static void Expand(this AutomationElement root)
        {
            Thread.Sleep(100);
            ExpandCollapsePattern p = (ExpandCollapsePattern)root.GetCurrentPattern(ExpandCollapsePattern.Pattern);
            p.Expand();
        }

        public static void AddToSelection(this AutomationElement root)
        {
            SelectionItemPattern p = (SelectionItemPattern)root.GetCurrentPattern(SelectionItemPattern.Pattern);
            p.AddToSelection();
        }

        public static void Invoke(this AutomationElement root)
        {
            InvokePattern p = (InvokePattern)root.GetCurrentPattern(InvokePattern.Pattern);
            p.Invoke();
        }

        public static string GetText(this AutomationElement root)
        {
            ValuePattern p = (ValuePattern)root.GetCurrentPattern(ValuePattern.Pattern);
            return p.Current.Value;
        }

        public static void SetText(this AutomationElement root, string text)
        {
            ValuePattern p = (ValuePattern)root.GetCurrentPattern(ValuePattern.Pattern);
            p.SetValue(text);
        }
        
        public static string GetName(this AutomationElement root)
        {
            return (string) root.GetCurrentPropertyValue(AutomationElement.NameProperty);
        }
    }
}
