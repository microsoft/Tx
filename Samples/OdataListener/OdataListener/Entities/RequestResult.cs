// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace OdataListener.Entities
{
    using System.Collections.Generic;

    public class RequestResult<T>
    {
        public RequestResult()
        {
            this.Links = new List<Link>();
        }

        public int Count { get; set; }

        public T Data { get; set; }

        public List<Link> Links { get; set; }
    }
}
