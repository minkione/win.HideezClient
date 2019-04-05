using System;
using System.IO;
using System.Xml.Serialization;
using HideezSafe.Modules.FileSerializer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HideezSafe.Tests
{
    [TestClass]
    public class XmlFileSerializerTests
    {
        /// <summary>
        /// Serializable dummy class
        /// </summary>
        [Serializable]
        [XmlRoot(ElementName = "SerializableBoolDummy", IsNullable = false)]
        public class SerializableBoolDummy
        {
            [XmlElement(ElementName = "BoolDummyProperty")]
            public bool BoolDummyProperty { get; set; } = false;

            public override bool Equals(object obj)
            {
                return (obj is SerializableBoolDummy) && ((SerializableBoolDummy)obj).BoolDummyProperty == BoolDummyProperty;
            }

            public override int GetHashCode()
            {
                // No reason to implement this correctly
                return base.GetHashCode();
            }
        }

        /// <summary>
        /// Serializable dummy class 
        /// </summary>
        [Serializable]
        [XmlRoot(ElementName = "SerializableIntDummy", IsNullable = false)]
        public class SerializableIntDummy
        {
            [XmlElement(ElementName = "IntDummyProperty")]
            public int IntDummyProperty { get; set; } = 44;

            public override bool Equals(object obj)
            {
                return (obj is SerializableIntDummy) && ((SerializableIntDummy)obj).IntDummyProperty == IntDummyProperty;
            }

            public override int GetHashCode()
            {
                // No reason to implement this correctly
                return base.GetHashCode();
            }
        }


        [TestMethod]
        public void Serialize_ExistingPath_FileCreated()
        {
            // Arrange
            var fileName = $"{Path.GetRandomFileName()}.xml";
            var path = Path.GetTempPath();
            var filePath = Path.Combine(path, fileName);

            Directory.CreateDirectory(path);

            if (File.Exists(filePath))
                File.Delete(filePath);

            var data = new SerializableBoolDummy();

            var xmlSerializer = new XmlFileSerializer();

            // Act
            var serializationResult = xmlSerializer.Serialize(filePath, data);

            // Assert
            Assert.IsTrue(File.Exists(filePath));
            Assert.IsTrue(serializationResult);
        }

        [TestMethod]
        [ExpectedException(typeof(DirectoryNotFoundException))]
        public void Serialize_NonexistingPath_ExceptionThrown()
        {
            // Arrange
            var fileName = $"{Path.GetRandomFileName()}.xml";
            var path = Path.Combine(Path.GetTempPath(), Path.Combine(Path.GetRandomFileName(), Path.GetRandomFileName()));
            var filePath = Path.Combine(path, fileName);

            if (Directory.Exists(path))
                Directory.Delete(path, true);

            if (File.Exists(filePath))
                File.Delete(filePath);

            var data = new SerializableBoolDummy();

            var xmlSerializer = new XmlFileSerializer();

            // Act
            var serializationResult = xmlSerializer.Serialize(filePath, data);

            // Assert phase is empty: expecting exception, nothing to assert
        }

        [TestMethod]
        public void Deserialize_NonexistingPath_DefaultReturned()
        {
            // Arrange
            var fileName = $"{Path.GetRandomFileName()}.xml";
            var path = Path.Combine(Path.GetTempPath(), Path.Combine(Path.GetRandomFileName(), Path.GetRandomFileName()));
            var filePath = Path.Combine(path, fileName);

            if (Directory.Exists(path))
                Directory.Delete(path, true);

            if (File.Exists(filePath))
                File.Delete(filePath);

            var data = new SerializableBoolDummy();
            data.BoolDummyProperty = true;

            var xmlSerializer = new XmlFileSerializer();

            // Act
            var deserializedData = xmlSerializer.Deserialize<SerializableBoolDummy>(filePath);

            // Assert
            Assert.IsTrue(deserializedData == null);
        }

        [TestMethod]
        public void Deserialize_IncorrectDeserializationType_DefaultReturned()
        {
            // Arrange
            var fileName = $"{Path.GetRandomFileName()}.xml";
            var path = Path.GetTempPath();
            var filePath = Path.Combine(path, fileName);

            Directory.CreateDirectory(path);

            if (File.Exists(filePath))
                File.Delete(filePath);

            var data = new SerializableBoolDummy();
            data.BoolDummyProperty = true;

            var xmlSerializer = new XmlFileSerializer();

            // Act
            var serializationResult = xmlSerializer.Serialize(filePath, data);
            var deserializedData = xmlSerializer.Deserialize<SerializableIntDummy>(filePath);

            // Assert
            Assert.IsTrue(deserializedData == null);
        }

        [TestMethod]
        public void SerializeDeserialize_ExistingPath_Deserialized()
        {
            // Arrange
            var fileName = $"{Path.GetRandomFileName()}.xml";
            var path = Path.GetTempPath();
            var filePath = Path.Combine(path, fileName);

            Directory.CreateDirectory(path);

            if (File.Exists(filePath))
                File.Delete(filePath);

            var data = new SerializableBoolDummy();
            data.BoolDummyProperty = true;
            var defaultData = default(SerializableBoolDummy);

            var xmlSerializer = new XmlFileSerializer();


            // Act
            var serializationResult = xmlSerializer.Serialize(filePath, data);
            var deserializedData = xmlSerializer.Deserialize<SerializableBoolDummy>(filePath);

            // Assert
            Assert.IsTrue(serializationResult);
            Assert.IsFalse(deserializedData.Equals(defaultData));
            Assert.IsTrue(deserializedData.Equals(data));
        }
    }
}
