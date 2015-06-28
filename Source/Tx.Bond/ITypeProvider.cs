// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Tx.Binary
{
    using System;
    using System.Collections.Generic;

    public interface ITypeProvider
    {
        IEnumerable<Type> GetSupportedTypes();
    }
}
