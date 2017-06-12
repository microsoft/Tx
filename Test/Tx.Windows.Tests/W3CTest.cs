using System;
using System.Globalization;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tx.Windows.Tests
{
    [TestClass]
    public class W3CTest
    {
        [TestMethod]
        public void W3CbasicRead()
        {
            var en = W3CEnumerable.FromFile("u_ex130609.log");
            int count = en.Count();

            Assert.AreEqual(17, count);
        }

        [TestMethod]
        public void W3C_Parsing()
        {
            var @event = W3CEnumerable.FromFile("u_ex130609.log").First();

            Assert.AreEqual("::1", @event.c_ip, false, CultureInfo.InvariantCulture);
            Assert.IsNull(@event.cs_Cookie);
            Assert.IsNull(@event.cs_Referer);
            Assert.AreEqual(@"Mozilla/5.0+(compatible;+MSIE+10.0;+Windows+NT+6.2;+WOW64;+Trident/6.0)", @event.cs_User_Agent, false, CultureInfo.InvariantCulture);
            Assert.IsNull(@event.cs_bytes);
            Assert.IsNull(@event.cs_host);
            Assert.AreEqual("GET", @event.cs_method, false, CultureInfo.InvariantCulture);
            Assert.IsNull( @event.cs_uri_query);
            Assert.AreEqual(@"/", @event.cs_uri_stem, false, CultureInfo.InvariantCulture);
            Assert.IsNull(@event.cs_username);
            Assert.IsNull(@event.cs_version);
            Assert.AreEqual(DateTimeKind.Unspecified, @event.dateTime.Kind);
            Assert.AreEqual(new DateTime(635063969570000000L, DateTimeKind.Unspecified), @event.dateTime);
            Assert.IsNull(@event.s_computername);
            Assert.AreEqual("80", @event.s_port, false, CultureInfo.InvariantCulture);
            Assert.IsNull(@event.s_sitename);
            Assert.IsNull(@event.sc_bytes);
            Assert.AreEqual("200", @event.sc_status, false, CultureInfo.InvariantCulture);
            Assert.AreEqual("0", @event.sc_substatus, false, CultureInfo.InvariantCulture);
            Assert.AreEqual("0", @event.sc_win32_status, false, CultureInfo.InvariantCulture);
            Assert.AreEqual("156", @event.time_taken, false, CultureInfo.InvariantCulture);
        }

        [TestMethod]
        public void W3CPlayback()
        {
            using (var playback = new Playback())
            {
                playback.AddW3CLogFiles("u_ex130609.log");

                int count = 0;
                using (playback.GetObservable<W3CEvent>()
                    .Count()
                    .Subscribe(c => count = c))
                {
                    playback.Run();
                }

                Assert.AreEqual(17, count);
            }
        }
    }
}
