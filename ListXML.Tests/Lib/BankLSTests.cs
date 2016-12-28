using Microsoft.VisualStudio.TestTools.UnitTesting;
using Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lib.Tests
{
    [TestClass()]
    public class BankLSTests
    {
        public BankLSTests()
        {
            BankLS.BIC3 = "702";
        }

        [TestMethod()]
        public void KeyedTest()
        {
            Equals(BankLS.Keyed(1224), "40702810600000001224");
            Equals(BankLS.Keyed("4000"), "40702810300000004000");
        }
    }
}