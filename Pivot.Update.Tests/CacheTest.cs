using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Pivot.Update.Tests
{
    /// <summary>
    /// Tests the Cache class.
    /// </summary>
    [TestClass]
    public class CacheTest
    {
        [TestMethod]
        public void TestCreationGlobal()
        {
            Cache c = new Cache(false);
            Assert.IsNotNull(c);
        }

        [TestMethod]
        public void TestCreationLocal()
        {
            Cache c = new Cache(true);
            Assert.IsNotNull(c);
        }

        [TestMethod]
        public void TestNotExists()
        {
            Cache local = new Cache(true);
            Cache global = new Cache(false);
            Assert.IsFalse(local.Exists("tests/should_not_exist"));
            Assert.IsFalse(global.Exists("tests/should_not_exist"));
        }

        [TestMethod]
        public void TestSetGetExistsDelete()
        {
            Cache local = new Cache(true);
            Cache global = new Cache(false);
            local.Set<string>("tests/temporary_value", "My Local Value!");
            global.Set<string>("tests/temporary_value", "My Global Value!");
            Assert.AreEqual(local.Get<string>("tests/temporary_value"), "My Local Value!");
            Assert.AreEqual(global.Get<string>("tests/temporary_value"), "My Global Value!");
            Assert.IsTrue(local.Exists("tests/temporary_value"));
            Assert.IsTrue(global.Exists("tests/temporary_value"));
            local.Delete("tests/temporary_value");
            global.Delete("tests/temporary_value");
        }

        [TestMethod]
        public void TestSetExistsDelete()
        {
            Cache local = new Cache(true);
            Cache global = new Cache(false);
            local.Set<string>("tests/temporary_value", "My Local Value!");
            global.Set<string>("tests/temporary_value", "My Global Value!");
            Assert.IsTrue(local.Exists("tests/temporary_value"));
            Assert.IsTrue(global.Exists("tests/temporary_value"));
            local.Delete("tests/temporary_value");
            global.Delete("tests/temporary_value");
        }
    }
}
