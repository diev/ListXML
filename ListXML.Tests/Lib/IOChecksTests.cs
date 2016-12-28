using Microsoft.VisualStudio.TestTools.UnitTesting;
using Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Lib.Tests
{
    [TestClass()]
    public class IOChecksTests
    {
        [TestMethod()]
        public void CheckDirectoryTest()
        {
            string test = IOChecks.CheckDirectory(App.Dir);
            if (!Directory.Exists(test))
            {
                Assert.Fail("App.Dir doesn't exist");
            }
        }

        [TestMethod()]
        public void CheckFileDirectoryTest()
        {
            string test = IOChecks.CheckFileDirectory(App.Exe);
            if (!Directory.Exists(test))
            {
                Assert.Fail("App.Dir doesn't exist");
            }
        }
    }
}