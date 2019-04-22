using HideezSafe.Models.Settings;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace HideezSafe.Tests
{
    [TestClass]
    public class BaseSettingsTests
    {
        private class TestSettings1 : BaseSettings
        {
            [Setting]
            public string StringPropert { get; set; }

            [Setting]
            public int IntProperty { get; set; }

            public override object Clone()
            {
                throw new NotImplementedException();
            }
        }

        private class TestSettings2 : BaseSettings
        {
            [Setting]
            public string StringPropert { get; set; }

            [Setting]
            public int IntProperty { get; set; }

            public override object Clone()
            {
                throw new NotImplementedException();
            }
        }

        [TestMethod]
        public void Equals_SameValues_True()
        {
            // Arrange
            var intValue = 317;
            var strValue = "s";
            var fooA = new TestSettings1
            {
                IntProperty = intValue,
                StringPropert = strValue
            };
            var fooB = new TestSettings1
            {
                IntProperty = intValue,
                StringPropert = strValue
            };

            // Act
            var equalsAB = fooA.Equals(fooB);
            var equalsBA = fooB.Equals(fooA);

            // Assert
            Assert.IsTrue(equalsAB);
            Assert.IsTrue(equalsBA);
        }

        [TestMethod]
        public void Equals_DifferentValues_False()
        {
            // Arrange
            var fooA = new TestSettings1
            {
                IntProperty = 123,
                StringPropert = "ms"
            };
            var fooB = new TestSettings1
            {
                IntProperty = 321,
                StringPropert = "sdfe"
            };

            // Act
            var equalsAB = fooA.Equals(fooB);
            var equalsBA = fooB.Equals(fooA);

            // Assert
            Assert.IsFalse(equalsAB);
            Assert.IsFalse(equalsBA);
        }

        [TestMethod]
        public void Equals_DifferentClasses_False()
        {
            // Arrange
            var intValue = 317;
            var strValue = "s";
            var fooA = new TestSettings1
            {
                IntProperty = intValue,
                StringPropert = strValue
            };
            var fooB = new TestSettings2
            {
                IntProperty = intValue,
                StringPropert = strValue
            };

            // Act
            var equalsAB = fooA.Equals(fooB);
            var equalsBA = fooB.Equals(fooA);

            // Assert
            Assert.IsFalse(equalsAB);
            Assert.IsFalse(equalsBA);

        }

        [TestMethod]
        public void GetHashCode_SameValues_SameHash()
        {
            // Arrange
            var intValue = 317;
            var strValue = "s";
            var fooA = new TestSettings1
            {
                IntProperty = intValue,
                StringPropert = strValue
            };
            var fooB = new TestSettings1
            {
                IntProperty = intValue,
                StringPropert = strValue
            };

            // Act
            var hashA = fooA.GetHashCode();
            var hashB = fooB.GetHashCode();

            // Assert
            Assert.AreEqual(hashA, hashB);
        }

        [TestMethod]
        public void GetHashCode_DifferentValues_DifferentHash()
        {
            // Arrange
            var fooA = new TestSettings1
            {
                IntProperty = 123,
                StringPropert = "ms"
            };
            var fooB = new TestSettings1
            {
                IntProperty = 321,
                StringPropert = "sdfe"
            };

            // Act
            var hashA = fooA.GetHashCode();
            var hashB = fooB.GetHashCode();

            // Assert
            Assert.AreNotEqual(hashA, hashB);
        }

        [TestMethod]
        public void GetHashCode_DifferentClasses_DifferentHash()
        {
            // Arrange
            var intValue = 317;
            var strValue = "s";
            var fooA = new TestSettings1
            {
                IntProperty = intValue,
                StringPropert = strValue
            };
            var fooB = new TestSettings2
            {
                IntProperty = intValue,
                StringPropert = strValue
            };

            // Act
            var hashA = fooA.GetHashCode();
            var hashB = fooB.GetHashCode();

            // Assert
            Assert.AreNotEqual(hashA, hashB);
        }
    }
}
