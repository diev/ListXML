using Microsoft.VisualStudio.TestTools.UnitTesting;
using Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ListXML;

namespace Tests
{
    [TestClass()]
    public class SettingsTests
    {
        [TestMethod()]
        public void IsSetTest()
        {
            string test;
            if (!Settings.IsSet("Log", out test))
            {
                Assert.Fail();
            }
        }

        [TestMethod()]
        public void GetPathTest()
        {
            string test = @"%TEMP%";
            Settings.Add("TEMP", test);
            if (Settings.GetPath("TEMP").Contains(test))
            {
                Assert.Fail();
            }
        }
    }
}