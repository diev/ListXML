//------------------------------------------------------------------------------
// Copyright (c) Dmitrii Evdokimov
// Source https://github.com/diev/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//------------------------------------------------------------------------------

using Lib;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ListXML.Tests
{
    [TestClass]
    public class AppTest
    {
        [TestMethod]
        public void IsSet_WithValue_True()
        {
            string test;
            if (App.IsSet("Email", out test))
            {
                Assert.AreEqual("admin@bank.ru", test, true, "Value is wrong");
            }
            else
            {
                Assert.Fail("IsSet is false");
            }
        }

        [TestMethod]
        public void IsSet_WithWhiteValue_False()
        {
            string test;
            if (App.IsSet("ED243", out test))
            {
                Assert.Fail("IsSet is true");
            }
        }

        [TestMethod]
        public void IsSet_WithNoValue_False()
        {
            string test;
            if (App.IsSet("ED273", out test))
            {
                Assert.Fail("IsSet is true");
            }
        }

        [TestMethod]
        public void IsSet_WithoutValue_False()
        {
            string test;
            if (App.IsSet("ED999", out test))
            {
                Assert.Fail("IsSet is true");
            }
        }
    }
}
