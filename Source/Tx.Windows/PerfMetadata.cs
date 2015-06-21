// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Tx.Windows
{
    public class PerfMetadata
    {
        ReadOnlyCollection<MachineInfo> _machines;
 
        public PerfMetadata(string logFileName)
        {
            List<string> machines = PdhUtils.GetMachineList(logFileName);
            var machineInfos = new List<MachineInfo>();

            foreach (string m in machines)
            {
                var objects = PdhUtils.GetObjectList(logFileName, m);
                var objectInfos = new List<ObjectInfo>();

                foreach (string o in objects)
                {
                    var counters = new List<string>();
                    var instances = new List<string>();

                    PdhUtils.GetCounterAndInstanceList(logFileName, m, o, out counters, out instances);
                    ObjectInfo info = new ObjectInfo(o, counters, instances);

                    objectInfos.Add(info);
                }

                 machineInfos.Add(new MachineInfo(m, objectInfos));

            }

            _machines = machineInfos.AsReadOnly();
        }

        public IEnumerable<MachineInfo> Machines { get { return _machines; } }
        
    }

    public class MachineInfo
    {
        readonly string _name;
        ObjectInfo[] _objects;

        internal MachineInfo(string name, List<ObjectInfo> objects)
        {
            _name = name;
            _objects = objects.ToArray();
        }

        public string Name { get { return _name; } }

        public ObjectInfo[] Objects { get { return _objects; } }
    }

    public class ObjectInfo
    {
        readonly string _name;
        string[] _counters;
        string[] _instances;

        internal ObjectInfo(string name, List<string> counters, List<string> instances)
        {
            _name = name;
            _counters = counters.ToArray();
            _instances = instances.ToArray();
        }

        public string Name { get { return _name; } }
        public string[] Counters { get { return _counters; } }
        public string[] Instances { get { return _instances; } }
    }
}
