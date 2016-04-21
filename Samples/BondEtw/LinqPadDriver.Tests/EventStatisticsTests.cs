using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tx.Bond.Extensions;

namespace LinqPadDriver.Tests
{
    [TestClass]
    public class EventStatisticsTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddOperatorTestValidations()
        {
            EventStatistics y = new EventStatistics
            {
                AverageByteSize = 2,
                ByteSize = 3,
                EventCount = 4,
                EventsPerSecond = 5,
            };

           var result =  null + y;
        }
    }
}
