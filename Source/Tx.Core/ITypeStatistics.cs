// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace System.Reactive
{
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