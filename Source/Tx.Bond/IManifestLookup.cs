// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Tx.Binary
{
    using System;
    using System.Collections.Generic;

    public interface IManifestLookup<TKey>
    {
        TKey LookupManifestId(Type type);

        Type TypeNameToType(string typeName);

        Type ManifestToType(string manifestId);

        IEnumerable<KeyValuePair<TKey, Type>> GetAllSupportedTypes();
    }
}
