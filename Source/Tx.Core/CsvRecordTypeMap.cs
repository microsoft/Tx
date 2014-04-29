// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Reactive
{
    using System;
    using System.Globalization;

    public sealed class CsvRecordTypeMap : SingleTypeMap<string[]>
    {
        public override Func<string[], DateTimeOffset> TimeFunction
        {
            get
            {
                return i => DateTime.Parse(
                    i[0],
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
            }
        }
    }
}
