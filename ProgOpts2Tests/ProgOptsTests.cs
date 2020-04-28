using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace ProgOpts2.Tests
{
    [TestClass()]
    public class ProgOptsTests
    {
        private readonly ProgOpts.OptionSpec[] testOptions = new ProgOpts.OptionSpec[] {
                    new ProgOpts.OptionSpec { ShortOption = 'a', LongOption = "all", NumberOfParams = 0 },
                    new ProgOpts.OptionSpec { ShortOption = 'i', LongOption = "ignorecase", NumberOfParams = 0 },
                    new ProgOpts.OptionSpec { ShortOption = 'f', LongOption = "file", NumberOfParams = 1 },
                };

        [TestMethod()]
        public void ProgOptsTest()
        {
            var args = testOptions;
            var opts = new ProgOpts(args);
            // No exception

            ProgOpts.OptionSpec[] dupOptions1 = new ProgOpts.OptionSpec[] {
                    new ProgOpts.OptionSpec { ShortOption = 'a', LongOption = "all", NumberOfParams = 0 },
                    new ProgOpts.OptionSpec { ShortOption = 'i', LongOption = "ignorecase", NumberOfParams = 0 },
                    // duplicate i:
                    new ProgOpts.OptionSpec { ShortOption = 'i', LongOption = "input", NumberOfParams = 1 },
                    new ProgOpts.OptionSpec { ShortOption = 'f', LongOption = "file", NumberOfParams = 1 },
                };

            Assert.ThrowsException<ArgumentException>(() => new ProgOpts(dupOptions1));

            ProgOpts.OptionSpec[] dupOptions2 = new ProgOpts.OptionSpec[] {
                    new ProgOpts.OptionSpec { ShortOption = 'a', LongOption = "all", NumberOfParams = 0 },
                    new ProgOpts.OptionSpec { ShortOption = 'i', LongOption = "ignorecase", NumberOfParams = 0 },
                    new ProgOpts.OptionSpec { ShortOption = 'j', LongOption = "ignorecase", NumberOfParams = 1 },
                    new ProgOpts.OptionSpec { ShortOption = 'f', LongOption = "file", NumberOfParams = 1 },
                };

            Assert.ThrowsException<ArgumentException>(() => new ProgOpts(dupOptions2));
        }

        [TestMethod()]
        public void ParseOptionsTestNoSpaceSingleParam()
        {
            var opts = new ProgOpts(testOptions);
            var args = new[] { "--all", "-i", "-frequest.xml" };
            opts.ParseCommandLine(args);
            Assert.IsTrue(opts.IsOptionPresent("ignorecase"));
            Assert.IsTrue(opts.IsOptionPresent('i'));
            Assert.IsTrue(opts.IsOptionPresent("all"));
            Assert.IsTrue(opts.IsOptionPresent('a'));
            Assert.IsTrue(opts.IsOptionPresent('f'));
            Assert.IsTrue(opts.IsOptionPresent("file"));
            Assert.IsFalse(opts.IsOptionPresent('z'));
            Assert.IsFalse(opts.IsOptionPresent("zed"));
            var filename = opts.GetParam<char>('f');
            Assert.AreEqual(filename, "request.xml");
        }

        [TestMethod()]
        public void ParseOptionsTestSpaceSingleDashParam()
        {
            var opts = new ProgOpts(testOptions);
            var args = new[] { "--all", "-i", "-f", "request.xml" };
            opts.ParseCommandLine(args);
            Assert.IsTrue(opts.IsOptionPresent("ignorecase"));
            Assert.IsTrue(opts.IsOptionPresent('i'));
            Assert.IsTrue(opts.IsOptionPresent("all"));
            Assert.IsTrue(opts.IsOptionPresent('a'));
            Assert.IsTrue(opts.IsOptionPresent('f'));
            Assert.IsTrue(opts.IsOptionPresent("file"));

            var filename = opts.GetParam<string>("file");
            Assert.AreEqual(filename, "request.xml");
        }

        /// <summary>
        /// Example command line: --all -i --file=request.xml
        /// </summary>
        [TestMethod()]
        public void ParseOptionsTestEqualsDoubleDashEqualsParam()
        {
            var opts = new ProgOpts(testOptions);
            var args = new[] { "--all", "-i", "--file=request.xml" };
            opts.ParseCommandLine(args);
            Assert.IsTrue(opts.IsOptionPresent("ignorecase"));
            Assert.IsTrue(opts.IsOptionPresent('i'));
            Assert.IsTrue(opts.IsOptionPresent("all"));
            Assert.IsTrue(opts.IsOptionPresent('a'));
            Assert.IsTrue(opts.IsOptionPresent('f'));
            Assert.IsTrue(opts.IsOptionPresent("file"));

            var filename = opts.GetParam<string>("file");
            Assert.AreEqual(filename, "request.xml");
            var filename2 = opts.GetParam<char>('f');
            Assert.AreEqual(filename, "request.xml");
        }

        /// <summary>
        /// Example command line: --file page.html
        /// </summary>
        [TestMethod()]

        public void ParseOptionsTestDoubleDashParam()
        {
            var opts = new ProgOpts(testOptions);
            var args = new[] { "--all", "-i", "--file", "request.xml" };
            opts.ParseCommandLine(args);
            Assert.IsTrue(opts.IsOptionPresent("ignorecase"));
            Assert.IsTrue(opts.IsOptionPresent('i'));
            Assert.IsTrue(opts.IsOptionPresent("all"));
            Assert.IsTrue(opts.IsOptionPresent('a'));

            var filename = opts.GetParam<string>("file");
            Assert.AreEqual(filename, "request.xml");
            var filename2 = opts.GetParam<char>('f');
            Assert.AreEqual(filename, "request.xml");
        }

        [TestMethod()]
        public void SingleConcatenated()
        {
            var opts = new ProgOpts(testOptions);
            var args4 = new[] { "-ai" };
            opts.ParseCommandLine(args4);
            Assert.IsTrue(opts.IsOptionPresent("ignorecase"));
            Assert.IsTrue(opts.IsOptionPresent('i'));
            Assert.IsTrue(opts.IsOptionPresent("all"));
            Assert.IsTrue(opts.IsOptionPresent('a'));
            Assert.IsFalse(opts.IsOptionPresent('f'));
            Assert.IsFalse(opts.IsOptionPresent("file"));
        }


        [Ignore]
        [TestMethod()]
        public void OptionCountTest()
        {
            Assert.Fail();
        }

        [Ignore]
        [TestMethod()]
        public void IsOptionPresentTest()
        {
        }

        [Ignore]
        [TestMethod()]
        public void IsOptionPresentTest1()
        {
            Assert.Fail();
        }

        [Ignore]
        [TestMethod()]
        public void GetParamTest()
        {
            Assert.Fail();
        }
    }
}