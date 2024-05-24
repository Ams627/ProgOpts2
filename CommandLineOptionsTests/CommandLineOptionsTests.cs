// Copyright (c) Adrian Sims 2020
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Linq;

namespace CommandLineOptions.Tests
{
    [TestClass()]
    public class CommandLineOptionsTests
    {
        private readonly Options.OptionSpec[] testOptions = new Options.OptionSpec[] {
                    new Options.OptionSpec { ShortOption = 'a', LongOption = "all", NumberOfParams = 0 },
                    new Options.OptionSpec { ShortOption = 'i', LongOption = "ignorecase", NumberOfParams = 0 },
                    new Options.OptionSpec { ShortOption = 'f', LongOption = "file", NumberOfParams = 1 },
                };

        [TestMethod()]
        public void OptionsTest()
        {
            var args = testOptions;
            var opts = new Options(args);
            // No exception

            Options.OptionSpec[] dupOptions1 = new Options.OptionSpec[] {
                    new Options.OptionSpec { ShortOption = 'a', LongOption = "all", NumberOfParams = 0 },
                    new Options.OptionSpec { ShortOption = 'i', LongOption = "ignorecase", NumberOfParams = 0 },
                    // duplicate i:
                    new Options.OptionSpec { ShortOption = 'i', LongOption = "input", NumberOfParams = 1 },
                    new Options.OptionSpec { ShortOption = 'f', LongOption = "file", NumberOfParams = 1 },
                };

            Assert.ThrowsException<ArgumentException>(() => new Options(dupOptions1));

            Options.OptionSpec[] dupOptions2 = new Options.OptionSpec[] {
                    new Options.OptionSpec { ShortOption = 'a', LongOption = "all", NumberOfParams = 0 },
                    new Options.OptionSpec { ShortOption = 'i', LongOption = "ignorecase", NumberOfParams = 0 },
                    new Options.OptionSpec { ShortOption = 'j', LongOption = "ignorecase", NumberOfParams = 1 },
                    new Options.OptionSpec { ShortOption = 'f', LongOption = "file", NumberOfParams = 1 },
                };

            Assert.ThrowsException<ArgumentException>(() => new Options(dupOptions2));
        }

        [TestMethod()]
        public void ParseOptionsTestNoSpaceSingleParam()
        {
            var opts = new Options(testOptions);
            var args = new[] { "--all", "-i", "-frequest.xml" };
            opts.ParseCommandLine(args);
            opts.IsOptionPresent("ignorecase").Should().BeTrue();
            opts.IsOptionPresent('i').Should().BeTrue();
            opts.IsOptionPresent("all").Should().BeTrue();
            opts.IsOptionPresent('a').Should().BeTrue();
            opts.IsOptionPresent('f').Should().BeTrue();
            opts.IsOptionPresent("file").Should().BeTrue();
            opts.IsOptionPresent('z').Should().BeFalse();
            opts.IsOptionPresent("zed").Should().BeFalse();
            var filename = opts.GetParam<char>('f');
            filename.Should().Be("request.xml");
        }

        [TestMethod()]
        public void ParseOptionsTestSpaceSingleDashParam()
        {
            var opts = new Options(testOptions);
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
            var opts = new Options(testOptions);
            var args = new[] { "--all", "-i", "--file=request.xml" };
            opts.ParseCommandLine(args);
            Assert.IsTrue(opts.IsOptionPresent("ignorecase"));
            Assert.IsTrue(opts.IsOptionPresent('i'));
            Assert.IsTrue(opts.IsOptionPresent("all"));
            Assert.IsTrue(opts.IsOptionPresent('a'));
            Assert.IsTrue(opts.IsOptionPresent('f'));
            Assert.IsTrue(opts.IsOptionPresent("file"));

            var filename = opts.GetParam("file");
            Assert.AreEqual(filename, "request.xml");
            var filename2 = opts.GetParam('f');
            Assert.AreEqual(filename, "request.xml");
        }

        /// <summary>
        /// Example command line: --file page.html
        /// </summary>
        [TestMethod()]

        public void ParseOptionsTestDoubleDashParam()
        {
            var opts = new Options(testOptions);
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
            var opts = new Options(testOptions);
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
            var opts = new Options(testOptions);
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
            var opts = new Options(testOptions);
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
            Options.OptionSpec[] testOptions = new Options.OptionSpec[] {
                new Options.OptionSpec { ShortOption = 'a', LongOption = "all", NumberOfParams = 0 },
                new Options.OptionSpec { ShortOption = 'i', LongOption = "ignorecase", NumberOfParams = 0 },
                new Options.OptionSpec { ShortOption = 'f', LongOption = "file", NumberOfParams = 1, MaxOccurs=255 },
                };

            var opts = new Options(testOptions);
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


        // several parameters for a single option - e.g. -f specifies a lis of files:
        [TestMethod()]
        public void MoreThanOneParamTest()
        {
            Options.OptionSpec[] testOptions = new Options.OptionSpec[] {
                new Options.OptionSpec { ShortOption = 'a', LongOption = "all", NumberOfParams = 0 },
                new Options.OptionSpec { ShortOption = 'i', LongOption = "ignorecase", NumberOfParams = 0 },
                new Options.OptionSpec { ShortOption = 'f', LongOption = "file", NumberOfParams = 3},
                };

            var opts = new Options(testOptions);
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
            string[] arr = { "file1.cpp", "file2.cpp", "file3.cpp" };
            CollectionAssert.AreEqual(arr, strArray);
            var nonOpts = opts.NonOptions;
            Assert.AreEqual(1, nonOpts.Length);
            Assert.AreEqual("file4.cpp", nonOpts[0].arg);
            Assert.AreEqual(4, nonOpts[0].index);
        }

        /// <summary>
        /// One good option (-f) and three illegal options (-eqy)
        /// </summary>
        [TestMethod]
        public void IllegalSingleDashOptions()
        {
            Options.OptionSpec[] testOptions = new Options.OptionSpec[] {
                new Options.OptionSpec { ShortOption = 'a', LongOption = "all", NumberOfParams = 0 },
                new Options.OptionSpec { ShortOption = 'i', LongOption = "ignorecase", NumberOfParams = 0 },
                new Options.OptionSpec { ShortOption = 'f', LongOption = "file", NumberOfParams = 1},
                };

            var opts = new Options(testOptions);
            var args = new[] { "-f", "file1.cpp", "-eqy" };
            var result = opts.ParseCommandLine(args);
            Assert.IsFalse(result);
            var illegalOptions = opts.IllegalOptions;
            Assert.AreEqual(illegalOptions.Length, 3);
        }

        /// <summary>
        /// an option that can be specified more than once on the command line AND which takes more than one parameter for each occurrence
        /// In this example, consider a --ll option that allows you to specify latitude and longitude but you must specify -ll for each latitude
        /// longitude pair:
        /// --ll 72.682 31.997 --ll 45.978 10.421 --ll 49.678 39.766 --ll 6.550 64.499 
        /// </summary>
        [TestMethod]
        public void MultiOptionMultiParam()
        {
            Options.OptionSpec[] testOptions = new Options.OptionSpec[] {
                new Options.OptionSpec { ShortOption = 'l', LongOption = "ll", NumberOfParams = 2, MaxOccurs = 100 },
                };

            var opts = new Options(testOptions);

            int occurrences = 10;
            var rnd = new Random();
            var pairs = Enumerable.Range(1, occurrences).Select(x => ($"{rnd.NextDouble() * 100:F3}", $"{rnd.NextDouble() * 100:F3}")).ToList();
            var args = pairs.Select(x => new[] { "--ll", x.Item1, x.Item2 }).SelectMany(y => y).ToArray();

            Debug.WriteLine(string.Join(" ", args));

            var result = opts.ParseCommandLine(args);
            Assert.IsTrue(result);
            var illegalOptions = opts.IllegalOptions;
            Assert.AreEqual(illegalOptions.Length, 0);

            var llOptionCount = opts.GetOptionCount("ll");
            Assert.AreEqual(llOptionCount, occurrences);
            for (int i = 0; i < occurrences; i++)
            {
                var optResult = opts.TryGetList("ll", out var pair, i);
                Assert.IsTrue(optResult);
                Assert.AreEqual(pair[0], pairs[i].Item1, pairs[i].Item2);
            }
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