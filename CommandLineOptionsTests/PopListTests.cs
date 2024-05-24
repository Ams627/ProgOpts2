using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;


namespace CommandLineOptions.Tests
{
    [TestClass()]
    public class PopQueueTests
    {
        [TestMethod()]
        public void PopQueueTest()
        {
            var args = new[] { "the", "fat", "cat", "sat", "on", "the", "mat" };
            var popList = new PopQueue<string>(args);
            Assert.AreEqual(args.Length, popList.OriginalCount);
            var popList2 = new PopQueue<string>(args, 2);
            Assert.AreEqual(args.Length - 2, popList2.OriginalCount);
        }

        [TestMethod()]
        public void PopFrontTest()
        {
            var args = new[] { "the", "fat", "cat", "sat", "on", "the", "mat" };
            var popList = new PopQueue<string>(args);
            Assert.AreEqual(args.Length, popList.OriginalCount);
            var pop1 = popList.PopFront();
            var pop2 = popList.PopFront();
            Assert.AreEqual(("the", 0), pop1);
            Assert.AreEqual(("fat", 1), pop2);
            var pop4 = new PopQueue<string>(args.Take(3).ToArray());
            pop4.PopFront();
            pop4.PopFront();
            pop4.PopFront();
            Assert.ThrowsException<InvalidOperationException>(() => pop4.PopFront());
        }

        [TestMethod()]
        public void UndoTest()
        {
            var args = new[] { "the", "fat", "cat", "sat", "on", "the", "mat" };
            var popList = new PopQueue<string>(args);
            Assert.AreEqual(args.Length, popList.OriginalCount);
            var pop1 = popList.PopFront();
            var pop2 = popList.PopFront();
            Assert.AreEqual(("the", 0), pop1);
            Assert.AreEqual(("fat", 1), pop2);
            popList.Undo();
            var pop3 = popList.PopFront();
            Assert.AreEqual(("fat", 1), pop3);
            var pop4 = new PopQueue<string>(args.Take(2).ToArray());
            pop4.PopFront();
            pop4.PopFront();
            pop4.Undo();
            pop4.Undo();
            Assert.ThrowsException<InvalidOperationException>(() => pop4.Undo());
            var pop5 = new PopQueue<string>(new string[] { });
            Assert.ThrowsException<InvalidOperationException>(() => pop5.Undo());
        }
    }
}