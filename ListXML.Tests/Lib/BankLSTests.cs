﻿#region License
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
#endregion

using Microsoft.VisualStudio.TestTools.UnitTesting;

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