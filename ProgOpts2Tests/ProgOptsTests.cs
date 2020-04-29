using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
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
            var args = new[] { "-ai" };
            opts.ParseCommandLine(args);
            Assert.IsTrue(opts.IsOptionPresent("ignorecase"));
            Assert.IsTrue(opts.IsOptionPresent('i'));
            Assert.IsTrue(opts.IsOptionPresent("all"));
            Assert.IsTrue(opts.IsOptionPresent('a'));
            Assert.IsFalse(opts.IsOptionPresent('f'));
            Assert.IsFalse(opts.IsOptionPresent("file"));
        }

        [TestMethod()]
        public void NonOptions()
        {
            var opts = new ProgOpts(testOptions);
            var args = new[] { "the", "fat", "cat", "sat", "on", "the", "mat" };
            opts.ParseCommandLine(args);
            var nonOptions = opts.NonOptions;

            Assert.AreEqual(args.Length, nonOptions.Length);
            Assert.AreEqual(args[0], nonOptions[0].arg);
            Assert.AreEqual(args[1], nonOptions[1].arg);
            Assert.AreEqual(args[2], nonOptions[2].arg);
            Assert.AreEqual(args[3], nonOptions[3].arg);
            Assert.AreEqual(args[4], nonOptions[4].arg);
            Assert.AreEqual(args[5], nonOptions[5].arg);
            Assert.AreEqual(args[6], nonOptions[6].arg);

            Assert.AreEqual(0, nonOptions[0].index);
            Assert.AreEqual(1, nonOptions[1].index);
            Assert.AreEqual(2, nonOptions[2].index);
            Assert.AreEqual(3, nonOptions[3].index);
            Assert.AreEqual(4, nonOptions[4].index);
            Assert.AreEqual(5, nonOptions[5].index);
            Assert.AreEqual(6, nonOptions[6].index);

            // no options present so these should all be false:
            Assert.IsFalse(opts.IsOptionPresent("ignorecase"));
            Assert.IsFalse(opts.IsOptionPresent('i'));
            Assert.IsFalse(opts.IsOptionPresent("all"));
            Assert.IsFalse(opts.IsOptionPresent('a'));
            Assert.IsFalse(opts.IsOptionPresent('f'));
            Assert.IsFalse(opts.IsOptionPresent("file"));
        }

        [TestMethod()]
        public void TryGetValueTest()
        {
            var opts = new ProgOpts(testOptions);
            var args = new[] { "-f", "file1.cpp" };
            opts.ParseCommandLine(args);

            // no options present so these should all be false:
            Assert.IsFalse(opts.IsOptionPresent("ignorecase"));
            Assert.IsFalse(opts.IsOptionPresent('i'));
            Assert.IsFalse(opts.IsOptionPresent("all"));
            Assert.IsFalse(opts.IsOptionPresent('a'));
            Assert.IsTrue(opts.IsOptionPresent('f'));
            Assert.IsTrue(opts.IsOptionPresent("file"));

            var result = opts.TryGetValue("file", out var str);
            Assert.IsTrue(result);
            Assert.AreEqual("file1.cpp", str);
        }

        [TestMethod()]
        public void TryMultiOptionTest()
        {
            ProgOpts.OptionSpec[] testOptions = new ProgOpts.OptionSpec[] {
                new ProgOpts.OptionSpec { ShortOption = 'a', LongOption = "all", NumberOfParams = 0 },
                new ProgOpts.OptionSpec { ShortOption = 'i', LongOption = "ignorecase", NumberOfParams = 0 },
                new ProgOpts.OptionSpec { ShortOption = 'f', LongOption = "file", NumberOfParams = 1, MaxOccurs=255 },
                };

            var opts = new ProgOpts(testOptions);
            var args = new[] { "-f", "file1.cpp", "-f", "file2.cpp", "-f", "file3.cpp" };
            opts.ParseCommandLine(args);

            // no options present so these should all be false:
            Assert.IsFalse(opts.IsOptionPresent("ignorecase"));
            Assert.IsFalse(opts.IsOptionPresent('i'));
            Assert.IsFalse(opts.IsOptionPresent("all"));
            Assert.IsFalse(opts.IsOptionPresent('a'));
            Assert.IsTrue(opts.IsOptionPresent('f'));
            Assert.IsTrue(opts.IsOptionPresent("file"));

            var result = opts.TryGetValue("file", out var str1, 0);
            Assert.IsTrue(result);
            Assert.AreEqual("file1.cpp", str1);
            result = opts.TryGetValue("file", out var str2, 1);
            Assert.IsTrue(result);
            Assert.AreEqual("file2.cpp", str2);
            result = opts.TryGetValue("file", out var str3, 2);
            Assert.IsTrue(result);
            Assert.AreEqual("file3.cpp", str3);
        }

        [TestMethod()]
        public void MoreThanOneParamTest()
        {
            ProgOpts.OptionSpec[] testOptions = new ProgOpts.OptionSpec[] {
                new ProgOpts.OptionSpec { ShortOption = 'a', LongOption = "all", NumberOfParams = 0 },
                new ProgOpts.OptionSpec { ShortOption = 'i', LongOption = "ignorecase", NumberOfParams = 0 },
                new ProgOpts.OptionSpec { ShortOption = 'f', LongOption = "file", NumberOfParams = 3},
                };

            var opts = new ProgOpts(testOptions);
            var args = new[] { "-f", "file1.cpp", "file2.cpp", "file3.cpp", "file4.cpp" };
            opts.ParseCommandLine(args);

            // no options present so these should all be false:
            Assert.IsFalse(opts.IsOptionPresent("ignorecase"));
            Assert.IsFalse(opts.IsOptionPresent('i'));
            Assert.IsFalse(opts.IsOptionPresent("all"));
            Assert.IsFalse(opts.IsOptionPresent('a'));
            Assert.IsTrue(opts.IsOptionPresent('f'));
            Assert.IsTrue(opts.IsOptionPresent("file"));

            var result = opts.TryGetList("file", out var strArray, 0);
            Assert.IsTrue(result);
            string[] arr =  { "file1.cpp", "file2.cpp", "file3.cpp" };
            CollectionAssert.AreEqual(arr, strArray);
            var nonOpts = opts.NonOptions;
            Assert.AreEqual(1, nonOpts.Length);
            Assert.AreEqual("file4.cpp", nonOpts[0].arg);
            Assert.AreEqual(4, nonOpts[0].index);
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