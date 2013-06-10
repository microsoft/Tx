using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tx.Windows;
using System.Reactive;
using System.Reactive.Linq;

namespace Tests.Tx
{
    [TestClass]
    public class W3CTest
    {
        [TestMethod]
        public void W3CbasicRead()
        {
            var en = W3CEnumerable.FromFile("u_ex130609.log");
            int count = en.Count();

            Assert.AreEqual(17, en.Count());
        }

        [TestMethod]
        public void W3CPlayback()
        {
            Playback playback = new Playback();
            playback.AddW3CLogFiles("u_ex130609.log");
            int count = 0;
            playback.GetObservable<W3CEvent>().Count().Subscribe(c => count = c);
            playback.Run();

            Assert.AreEqual(17, count);
        }
    }
}
