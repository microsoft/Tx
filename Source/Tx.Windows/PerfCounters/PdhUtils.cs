// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace Tx.Windows
{
    internal static class PdhUtils
    {
        public static List<string> MultiSzToStringList(char[] multiSz)
        {
            var returnValue = new List<string>();
            var buildBuffer = new StringBuilder();

            for (int i = 0; i <= multiSz.GetUpperBound(0); i++)
            {
                if (multiSz[i] != '\0')
                {
                    buildBuffer.Append(multiSz[i]);
                }
                else
                {
                    if (buildBuffer.Length > 0)
                    {
                        returnValue.Add(buildBuffer.ToString());
                        buildBuffer.Length = 0;
                    }
                }
            }

            returnValue.Sort();
            return returnValue;
        }

        public static void CheckStatus(PdhStatus actualStatus, params PdhStatus[] expectedStatus)
        {
            for (int i = 0; i <= expectedStatus.GetUpperBound(0); i++)
            {
                if (actualStatus == expectedStatus[i])
                {
                    return;
                }
            }

            throw new Exception(actualStatus.ToString());
        }

        public static List<string> GetMachineList(string logFileName)
        {
            List<string> machines;
            uint objectBufferLength = 0;

            PdhStatus pdhStatus = PdhNativeMethods.PdhEnumMachines(
                logFileName,
                null,
                ref objectBufferLength);

            CheckStatus(pdhStatus, PdhStatus.PDH_MORE_DATA);

            var objectListBuffer = new char[objectBufferLength];

            pdhStatus = PdhNativeMethods.PdhEnumMachines(
                logFileName,
                objectListBuffer,
                ref objectBufferLength);

            CheckStatus(pdhStatus, PdhStatus.PDH_CSTATUS_VALID_DATA);

            machines = MultiSzToStringList(objectListBuffer);

            return machines;
        }

        public static List<string> GetObjectList(string logFilename, string machineName)
        {
            uint objectBufferLength = 0;

            PdhStatus pdhStatus = PdhNativeMethods.PdhEnumObjects(
                logFilename,
                machineName,
                null,
                ref objectBufferLength,
                PdhDetailLevel.PERF_DETAIL_WIZARD,
                0);

            CheckStatus(pdhStatus, PdhStatus.PDH_MORE_DATA, PdhStatus.PDH_CSTATUS_VALID_DATA);
            if (pdhStatus == PdhStatus.PDH_MORE_DATA)
            {
                var objectListBuffer = new char[objectBufferLength];

                pdhStatus = PdhNativeMethods.PdhEnumObjects(
                    logFilename,
                    machineName,
                    objectListBuffer,
                    ref objectBufferLength,
                    PdhDetailLevel.PERF_DETAIL_WIZARD,
                    0);

                CheckStatus(pdhStatus, PdhStatus.PDH_CSTATUS_VALID_DATA);

                return MultiSzToStringList(objectListBuffer);
            }

            return new List<string>();
        }

        public static void GetCounterAndInstanceList(
            string logFilename,
            string machineName,
            string objectName,
            out List<string> counterList,
            out List<string> instanceList)
        {
            uint counterBufferLength = 0;
            uint instanceBufferLength = 0;

            PdhStatus pdhStatus = PdhNativeMethods.PdhEnumObjectItems(
                logFilename,
                machineName,
                objectName,
                null,
                ref counterBufferLength,
                null,
                ref instanceBufferLength,
                PdhDetailLevel.PERF_DETAIL_WIZARD,
                0);

            CheckStatus(pdhStatus, PdhStatus.PDH_CSTATUS_NO_OBJECT, PdhStatus.PDH_MORE_DATA, PdhStatus.PDH_CSTATUS_VALID_DATA);
            if (pdhStatus == PdhStatus.PDH_MORE_DATA)
            {
                var counterListBuffer = new char[counterBufferLength];
                var instanceListBuffer = new char[instanceBufferLength];

                pdhStatus = PdhNativeMethods.PdhEnumObjectItems(
                    logFilename,
                    machineName,
                    objectName,
                    counterListBuffer,
                    ref counterBufferLength,
                    instanceListBuffer,
                    ref instanceBufferLength,
                    PdhDetailLevel.PERF_DETAIL_WIZARD,
                    0);

                CheckStatus(pdhStatus, PdhStatus.PDH_CSTATUS_VALID_DATA);

                counterList = MultiSzToStringList(counterListBuffer);
                instanceList = MultiSzToStringList(instanceListBuffer);
            }
            else
            {
                counterList = new List<string>();
                instanceList = new List<string>();
            }
        }

        public static SortedList<string, SortedSet<string>> Parse(string dataSource)
        {
            var allCounters = new SortedList<string, SortedSet<string>>();

            List<string> machines = GetMachineList(dataSource);
            foreach (string machine in machines)
            {
                List<string> counterSets = GetObjectList(dataSource, machine);
                foreach (string counterSet in counterSets)
                {
                    SortedSet<string> countersSoFar;
                    if (!allCounters.TryGetValue(counterSet, out countersSoFar))
                    {
                        countersSoFar = new SortedSet<string>();
                        allCounters.Add(counterSet, countersSoFar);
                    }

                    List<string> counters;
                    List<string> instances;
                    GetCounterAndInstanceList(dataSource, machine, counterSet, out counters, out instances);

                    foreach (string c in counters)
                    {
                        if (!countersSoFar.Contains(c))
                            countersSoFar.Add(c);
                    }
                }
            }

            return allCounters;
        }

        public static string[] GetCounterPaths(string dataSource)
        {
            var counterPaths = new List<string>();

            List<string> machines = GetMachineList(dataSource);
            foreach (string machine in machines)
            {
                List<string> counterSets = GetObjectList(dataSource, machine);
                foreach (string counterSet in counterSets)
                {
                    List<string> counters;
                    List<string> instances;
                    GetCounterAndInstanceList(dataSource, machine, counterSet, out counters, out instances);

                    foreach (string c in counters)
                    {
                        if (instances.Count == 0)
                        {
                            var sb = new StringBuilder();
                            sb.Append(machine);
                            sb.Append('\\');
                            sb.Append(counterSet);
                            sb.Append('\\');
                            sb.Append(c);

                            counterPaths.Add(sb.ToString());
                        }
                        else
                        {
                            foreach (string instance in instances)
                            {
                                var sb = new StringBuilder();
                                sb.Append(machine);
                                sb.Append('\\');
                                sb.Append(counterSet);
                                sb.Append('(');
                                sb.Append(instance);
                                sb.Append(')');
                                sb.Append('\\');
                                sb.Append(c);

                                counterPaths.Add(sb.ToString());
                            }
                        }
                    }
                }
            }

            return counterPaths.ToArray();
        }
    }
}