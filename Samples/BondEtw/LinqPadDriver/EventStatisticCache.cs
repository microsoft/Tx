namespace Tx.Bond.LinqPad
{
    using BondEtwDriver;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using Tx.Binary;
    using Tx.Bond.Extensions;

    public class EventStatisticCache
    {
        private readonly string CsvHeaders = "ManifestId,TypeOfEvent,EventCount,Events/Second,ByteSize,Average Byte Size";

        private const char CsvSeparator = ',';

        public EventStatisticCache()
        {
        }

        public IDictionary<Type, EventStatistics> GetTypeStatistics(TypeCache typeCache, params string[] inputFiles)
        {
            Stopwatch sw = Stopwatch.StartNew();

            if (typeCache == null)
            {
                throw new ArgumentNullException("typeCache");
            }

            if (inputFiles == null || inputFiles.Length <= 0 || inputFiles.Any(f => string.IsNullOrWhiteSpace(f)))
            {
                throw new ArgumentNullException("inputFiles");
            }

            if (inputFiles.Any(a => !File.Exists(a)))
            {
                throw new ArgumentException("Some file paths are invalid.");
            }

            var overallStatsPerType = new Dictionary<Type, EventStatistics>();

            Console.WriteLine("Getting Statistics...");

            foreach (var file in inputFiles)
            {
                Dictionary<Type, EventStatistics> statsOfThisFile = null;

                var dir = System.IO.Path.GetDirectoryName(file);
                var fileName = System.IO.Path.GetFileNameWithoutExtension(file) + ".csv";
                var expectedCsv = System.IO.Path.Combine(dir, fileName);

                if (!this.IsStatsCsvAvailable(expectedCsv) || 
                    !this.TryParseStatsFromCsv(expectedCsv, typeCache, out statsOfThisFile))
                {
                    var csvRelatedStatsOfThisFile = this.CalculateTypeStatistics(typeCache, file);

                    if (csvRelatedStatsOfThisFile != null)
                    {
                        statsOfThisFile = new Dictionary<Type, EventStatistics>();
                        foreach (var item in csvRelatedStatsOfThisFile)
                        {
                            statsOfThisFile.Add(item.Key, item.Value.Statistics);
                        }
                        
                        this.CreateCsvStatsFile(expectedCsv, csvRelatedStatsOfThisFile);
                    }
                }

                if (statsOfThisFile != null)
                {
                    foreach (var item in statsOfThisFile)
                    {
                        EventStatistics existingStatsForThisType;
                        if (overallStatsPerType.TryGetValue(item.Key, out existingStatsForThisType))
                        {
                            overallStatsPerType[item.Key] = existingStatsForThisType + item.Value;
                        }
                        else
                        {
                            overallStatsPerType[item.Key] = item.Value;
                        }
                    }
                }
            }

            sw.Stop();
            Console.WriteLine("GetTypeStatistics took {0} milliseconds", sw.ElapsedMilliseconds);

            return overallStatsPerType;
        }

        private Dictionary<Type, CsvRelatedStats> CalculateTypeStatistics(TypeCache typeCache, string inputFile)
        {
            Console.WriteLine("Getting statistics from file {0}.", inputFile);

            var statsPerType = new Dictionary<Type, CsvRelatedStats>();

            Stopwatch sw = Stopwatch.StartNew();

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
                                           maxTime = Math.Max(ac.maxTime, events.ReceiveFileTimeUtc),
                                       })
                               select new { ManifestId = eventTypes.Key, all.EventCount, all.Bytes, all.minTime, all.maxTime, };

            var counts = rawCount.ToEnumerable().ToArray();

            sw.Stop();
            Console.WriteLine("Query took {0} milliseconds.", sw.ElapsedMilliseconds);

            foreach (var c in counts)
            {
                var typeCacheItem = typeCache.FindMatchOrDefault(c.ManifestId);

                if (typeCacheItem != null && typeCacheItem.Type != null)
                {
                    var type = typeCacheItem.Type;
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
                            EventsPerSecond = Math.Round(c.EventCount / duration, 3, MidpointRounding.AwayFromZero),
                        };

                        statsPerType[type] = new CsvRelatedStats
                        {
                            ManifestId = c.ManifestId,
                            Statistics = stats,
                        };
                    }
                }
            }

            return statsPerType;
        }

        private bool IsStatsCsvAvailable(string csvFile)
        {
            if (File.Exists(csvFile) && new FileInfo(csvFile).Length > 0)
            {
                Console.WriteLine("Csv file was found.");
                return true;
            }

            return false;
        }

        private bool TryParseStatsFromCsv(string inputFile, TypeCache typeCache, out Dictionary<Type, EventStatistics> eventStatsCollection)
        {
            bool isParsingSuccessful = true;
            
            bool headersFound = false;
            using (var streamReader = File.OpenText(inputFile))
            {
                eventStatsCollection = new Dictionary<Type, EventStatistics>();

                // At any point parsing is not successful, means current csv is not good enough. Rewrite of csv is required.
                while (!streamReader.EndOfStream && isParsingSuccessful)
                {
                    Console.WriteLine("Starting to parse csv...");

                    string line = string.Empty;

                    // First few lines are not important for stats.
                    while (!headersFound)
                    {
                        line = streamReader.ReadLine();
                        headersFound = !string.IsNullOrWhiteSpace(line) ? line.Contains(this.CsvHeaders) : false;
                    }

                    // headers found
                    if (headersFound)
                    {
                        line = streamReader.ReadLine();

                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            // Every line should pass this test else rewrite of csv is required.
                            isParsingSuccessful = this.IsCsvLineParsable(line, typeCache, eventStatsCollection);
                        }
                    }
                }
            }

            if (eventStatsCollection.Count == 0)
            {
                isParsingSuccessful = false;
            }

            return isParsingSuccessful;
        }

        private string GetTypeNameFromManifest(EventManifest manifest)
        {
            var line = manifest.Manifest.Split('\n').LastOrDefault(l => l.Trim().StartsWith(@"struct ", StringComparison.OrdinalIgnoreCase));

            string className = string.Empty;
            if (line != null)
            {
                className = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[1].Trim();
            }

            return className;
        }

        private bool IsCsvLineParsable(string line, TypeCache typeCache, Dictionary<Type, EventStatistics> eventStatsCollection)
        {
            bool isParsable = false;

            var tokens = line.Split(CsvSeparator);

            string eventManifestIdFromCsv = string.Empty;
            string typeNameFromCsv = string.Empty;

            if (tokens != null && tokens.Length == 6 && tokens.All(a => !string.IsNullOrWhiteSpace(a)))
            {
                eventManifestIdFromCsv = tokens[0];
                typeNameFromCsv = tokens[1];

                var typeCacheItem = typeCache.FindMatchOrDefault(eventManifestIdFromCsv);

                if (typeCacheItem != null && typeCacheItem.Type != null && string.Equals(typeCacheItem.Type.Name, typeNameFromCsv, StringComparison.OrdinalIgnoreCase))
                {
                    Debug.WriteLine(typeCacheItem.Type.Name);

                    EventStatistics statsFromCsv = new EventStatistics
                    {
                        AverageByteSize = long.Parse(tokens[5]),
                        ByteSize = long.Parse(tokens[4]),
                        EventCount = long.Parse(tokens[2]),
                        EventsPerSecond = double.Parse(tokens[3]),
                    };

                    EventStatistics existingStats;
                    if (eventStatsCollection.TryGetValue(typeCacheItem.Type, out existingStats))
                    {

                        existingStats = existingStats + statsFromCsv;
                        Console.WriteLine("Stats for type {0} updated.", typeNameFromCsv);
                    }
                    else
                    {
                        eventStatsCollection[typeCacheItem.Type] = statsFromCsv;
                        Console.WriteLine("Stats for type {0} added.", typeNameFromCsv);
                    }

                    isParsable = true;
                }
            }

            return isParsable;
            
        }

        private void CreateCsvStatsFile(string file, Dictionary<Type, CsvRelatedStats> stats)
        {
            // process only if you have write access.
            try
            {
                stats = stats.OrderBy(a => a.Key.Name).ToDictionary(a => a.Key, b => b.Value);

                using (StreamWriter sw = new StreamWriter(file))
                {
                    StringBuilder sb = new StringBuilder();
                    this.AppendFileRelatedDetails(sb, file);
                    sb.AppendLine();
                    sb.AppendLine();
                    this.AppendStats(sb, stats);

                    sw.Write(sb.ToString());
                }
            }
            catch 
            {
                // writing to file, erred. What do we do?
            }

        }

        private void AppendStats(StringBuilder sb, Dictionary<Type, CsvRelatedStats> stats)
        {
            sb.AppendLine(CsvHeaders);

            foreach (var item in stats)
            {
                sb.AppendFormat("{0},{1},{2},{3},{4},{5}", 
                    item.Value.ManifestId, 
                    item.Key.Name,
                    item.Value.Statistics.EventCount,
                    item.Value.Statistics.EventsPerSecond,
                    item.Value.Statistics.ByteSize,
                    item.Value.Statistics.AverageByteSize);
                sb.AppendLine();
            }
        }

        private void AppendFileRelatedDetails(StringBuilder sb, string file)
        {
            string parentFile = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(file), System.IO.Path.GetFileNameWithoutExtension(file) + ".etl");

            sb.AppendFormat("File Name,{0}", System.IO.Path.GetFileName(parentFile));
            sb.AppendLine();
            sb.AppendFormat("\nTimestamp,{0}", File.GetCreationTimeUtc(parentFile));
            sb.AppendLine();
            sb.AppendFormat("\nChecksum,{0}", CheckSum(parentFile));
            sb.AppendLine();
        }

        private static string CheckSum(string filename)
        {
            using (var checkSum = MD5.Create())
            {
                var hashValue = GetHashValue(checkSum, filename);

                var checkSumResult = hashValue;

                return checkSumResult;
            }
        }

        private static string GetHashValue(HashAlgorithm md5, string filename)
        {
            var stringBuilder = new StringBuilder();

            using (var fileStream = File.OpenRead(filename))
            {
                byte[] result = md5.ComputeHash(fileStream);

                foreach (var bytes in result)
                {
                    stringBuilder.Append(bytes.ToString("x2"));
                }
            }

            return stringBuilder.ToString();
        }

        private class CsvRelatedStats
        {
            internal string ManifestId;
            internal EventStatistics Statistics;
        }

    }

    
}
