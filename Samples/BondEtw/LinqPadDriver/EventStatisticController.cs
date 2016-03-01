namespace Tx.Bond.Extensions
{
    using BondInEtwLinqpadDriver;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using Tx.Binary;

    public class EventStatisticController
    {
        //private readonly Dictionary<string, string> typeNames;

        //private readonly Regex regex = new Regex("\".*?\"", RegexOptions.Compiled);

        // private long averageByteSize;
        // private double duration;
        // private double eventsPerSecond;

        public EventStatisticController(string connectionPath)
        {
            //this.TypeNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            //var typeMaps = BondInEtwRegistry.LoadSupportedTypeMaps(connectionPath);

            //foreach (var configType in typeMaps)
            //{
            //    var supportedTypes = ((ITypeProvider)configType).GetSupportedTypes();

            //    foreach (var supportedType in supportedTypes)
            //    {
            //        var key = configType.GetTypeKey(supportedType);

            //        if (!this.TypeNames.ContainsKey(key))
            //        {
            //            this.TypeNames.Add(key, supportedType.Name);
            //        }
            //    }
            //}

            //var createdTypeNames = this.GetCreatedTypeNames(connectionPath);

            //foreach (var pair in createdTypeNames)
            //{
            //    this.TypeNames[pair.Key.Replace("\"", "")] = pair.Value;
            //}
        }

        public Dictionary<Type, EventStatistics> GetTypeStatistics(TypeCache typeCache, string inputFile)
        {
            if (string.IsNullOrWhiteSpace(inputFile))
            {
                throw new ArgumentException("inputFile");
            }

            var statsPerType = new Dictionary<Type, EventStatistics>();

            Console.WriteLine("Getting Statistics...");

            var rawCount = from events in BinaryEtwObservable.FromSequentialFiles(inputFile)
                group events by events.PayloadId
                    into eventTypes
                    from all in
                        eventTypes.Aggregate(
                            new { EventCount = (long)0, Bytes = (long)0, minTime = long.MaxValue, maxTime = 0L },
                            (ac, events) =>
                            new
                            {
                                EventCount = ac.EventCount + 1,
                                Bytes = ac.Bytes + events.EventPayloadLength,
                                minTime = Math.Min(ac.minTime, events.ReceiveFileTimeUtc),
                                maxTime = Math.Max(ac.maxTime, events.ReceiveFileTimeUtc)
                            })
                    select new { ManifestId = eventTypes.Key, all.EventCount, all.Bytes, all.minTime, all.maxTime };

            var counts = rawCount.ToEnumerable().ToArray();

            foreach (var c in counts)
            {
                var manifest = typeCache.Manifests
                    .FirstOrDefault(m => string.Equals(m.ManifestId, c.ManifestId, StringComparison.OrdinalIgnoreCase));

                if (manifest != null)
                {
                    var line = manifest.Manifest
                        .Split('\n').LastOrDefault(l => l.Trim().StartsWith(@"struct ", StringComparison.OrdinalIgnoreCase));

                    if (line != null)
                    {
                        var className = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[1].Trim();

                        var type = typeCache.Types.FirstOrDefault(t => t.Name == className);

                        if (type != null)
                        {
                            var minDateTime = DateTime.FromFileTimeUtc(c.minTime);
                            var maxDateTime = DateTime.FromFileTimeUtc(c.maxTime);
                            var duration = (maxDateTime - minDateTime).TotalSeconds;
                            if (Math.Abs(duration) < 0.01) duration = 1;

                            var stats = new EventStatistics
                            {
                                AverageByteSize = c.Bytes / c.EventCount,
                                ByteSize = c.Bytes,
                                EventCount = c.EventCount,
                                EventsPerSecond = c.EventCount / duration
                            };

                            statsPerType[type] = stats;
                        }
                    }
                }
            }

            return statsPerType;
        }

        //private Dictionary<string, string> GetCreatedTypeNames(string connectionPath)
        //{
        //    var createdTypeNames = new Dictionary<string, string>();

        //    var bondFiles = Directory.GetFiles(connectionPath, "*.bond");

        //    var typeNames = bondFiles.Select(Path.GetFileNameWithoutExtension);

        //    foreach (var type in typeNames)
        //    {
        //        var csFile = Path.Combine(connectionPath, type + ".cs");

        //        if (File.Exists(csFile))
        //        {
        //            var lines = File.ReadAllLines(csFile);

        //            foreach (var line in lines)
        //            {
        //                if (line.Contains("[Guid("))
        //                {
        //                    foreach (Match match in this.regex.Matches(line))
        //                    {
        //                        var manifestId = match.ToString();

        //                        if (!createdTypeNames.ContainsKey(manifestId))
        //                        {
        //                            createdTypeNames.Add(manifestId, type);
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    return createdTypeNames;
        //}

        ///// <summary>
        ///// Writes to a summary (csv) file.
        ///// </summary>
        ///// <param name="file">ETL file</param>
        ///// <param name="fileName">Summary file name</param>
        ///// <param name="fileDirectory">Directory to write the summary file</param>
        ///// <param name="statsDictionary">Dictionary of events and stats</param>
        //private void WriteToSummaryFile(string file, string fileName, string fileDirectory, Dictionary<string, Stats> statsDictionary)
        //{
        //    if (string.IsNullOrWhiteSpace(file))
        //    {
        //        throw new ArgumentException("sourceFile");
        //    }

        //    if (string.IsNullOrWhiteSpace(fileName))
        //    {
        //        throw new ArgumentException("statsFileName");
        //    }

        //    if (string.IsNullOrWhiteSpace(fileDirectory))
        //    {
        //        throw new ArgumentException("statsFileDirectory");
        //    }

        //    if (statsDictionary == null)
        //    {
        //        throw new ArgumentNullException("statsDictionary");
        //    }

        //    Console.WriteLine("Writing to summary file... \n");
        //    string statsFilePath = Path.Combine(fileDirectory, fileName + ".csv");

        //    using (var streamWriter = new StreamWriter(statsFilePath))
        //    {
        //        WriteFileDetails(streamWriter, file);

        //        this.WriteStats(streamWriter, statsDictionary);
        //    }
        //}

        ///// <summary>
        ///// Write stats to the summary file.
        ///// </summary>
        ///// <param name="streamWriter">An instance of streamWriter class</param>
        ///// <param name="statsDictionary">Dictionary of events and stats</param>
        //private void WriteStats(StreamWriter streamWriter, Dictionary<string, Stats> statsDictionary)
        //{
        //    streamWriter.WriteLine("ManifestId" + "," + "TypeOfEvent " + "," + "EventCount " + "," + "Events/Second " + "," + "ByteSize " + "," + "Average Byte Size " + "," + "MinDateTime " + "," + "MaxDateTime");

        //    var e =
        //        (from pair in statsDictionary orderby pair.Key select pair).ToArray();

        //    foreach (var keyValuePair in e)
        //    {
        //        this.averageByteSize = keyValuePair.Value.ByteSize / keyValuePair.Value.EventCount;

        //        var minDateTime = DateTime.FromFileTimeUtc(keyValuePair.Value.MinTime);

        //        var maxDateTime = DateTime.FromFileTimeUtc(keyValuePair.Value.MaxTime);

        //        this.duration = (maxDateTime - minDateTime).TotalSeconds;

        //        if (Math.Abs(this.duration) < 0.01)
        //        {
        //            this.duration = 1;
        //        }

        //        this.eventsPerSecond = keyValuePair.Value.EventCount / this.duration;

        //        streamWriter.WriteLine(keyValuePair.Value.ManifestId + "," + keyValuePair.Key + "," + keyValuePair.Value.EventCount + "," + this.eventsPerSecond + "," + keyValuePair.Value.ByteSize + "," + this.averageByteSize + "," + minDateTime + "," + maxDateTime);
        //    }
        //}

        //private static void WriteFileDetails(TextWriter streamWriter, string file)
        //{
        //    if (streamWriter == null)
        //    {
        //        throw new ArgumentNullException("streamWriter");
        //    }

        //    if (string.IsNullOrWhiteSpace(file))
        //    {
        //        throw new ArgumentException("file");
        //    }

        //    streamWriter.WriteLine("File Name" + "," + Path.GetFileName(file));

        //    streamWriter.WriteLine("Timestamp" + "," + File.GetCreationTimeUtc(file) + " (UTC)");

        //    streamWriter.WriteLine("Checksum" + "," + CheckSum(file));

        //    streamWriter.WriteLine();
        //}

        //private string LookupTypeName(string manifestId)
        //{
        //    if (string.IsNullOrWhiteSpace(manifestId))
        //    {
        //        throw new ArgumentException("manifestId");
        //    }

        //    string type;
        //    if (!this.TypeNames.TryGetValue(manifestId, out type))
        //    {
        //        return "UnknownEvent" + " " + manifestId;
        //    }

        //    return type;
        //}

        //private static string CheckSum(string filename)
        //{
        //    if (string.IsNullOrWhiteSpace(filename))
        //    {
        //        throw new ArgumentException("fileName");
        //    }

        //    using (var checkSum = MD5.Create())
        //    {
        //        var hashValue = GetHashValue(checkSum, filename);

        //        var checkSumResult = hashValue;

        //        return checkSumResult;
        //    }
        //}

        //private static string GetHashValue(HashAlgorithm md5, string filename)
        //{
        //    if (md5 == null)
        //    {
        //        throw new ArgumentNullException("md5");
        //    }

        //    if (string.IsNullOrWhiteSpace(filename))
        //    {
        //        throw new ArgumentException("filename");
        //    }

        //    var stringBuilder = new StringBuilder();

        //    var streamReader = File.OpenRead(filename);

        //    byte[] result = md5.ComputeHash(streamReader);

        //    foreach (var bytes in result)
        //    {
        //        stringBuilder.Append(bytes.ToString("x2"));
        //    }

        //    return stringBuilder.ToString();
        //}
    }
}
