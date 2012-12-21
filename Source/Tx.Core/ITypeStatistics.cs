namespace System.Reactive
{
    using System;
    using System.Collections.Generic;

    public class TreeNode
    {
        public string ToolTipText { get; set; }
        public string DragText { get; set; }
        public Type Type { get; set; }
        public SortedList<string, TreeNode> Children { get; set; }
    }

    public interface ITypeStatistics
    {
        SortedList<string, TreeNode> GetTypeStatistics(
            IEnumerable<string> files,
            IEnumerable<Type> availableTypes,
            bool onlyEventsOccurring);
    }
}
