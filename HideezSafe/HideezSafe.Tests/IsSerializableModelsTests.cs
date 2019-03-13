using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HideezSafe.Models.Settings;

namespace HideezSafe.Tests
{
    /// <note>
    /// All models must override Equals
    /// Otherwise assertion will fail during comparison
    /// </note>
    [TestClass]
    public class IsSerializableModelsTests
    {
        /// <summary>
        /// Generic class for serializable objects asserting
        /// </summary>
        /// <typeparam name="T">Serializable Type</typeparam>
        class GenericSerializeAsserter<T> where T : new()
        {
            /// <summary>
            /// Serialize object into stream
            /// </summary>
            /// <param name="source">Object to serialize</param>
            /// <returns>Returns stream with serialized object</returns>
            public static Stream Serialize(object source)
            {
                IFormatter formatter = new BinaryFormatter();

                Stream stream = new MemoryStream();

                formatter.Serialize(stream, source);

                return stream;

            }

            /// <summary>
            /// Deserialize object from stream
            /// </summary>
            /// <param name="stream">Stream with serialized object</param>
            /// <returns>Returns deserialized object</returns>
            public static T Deserialize(Stream stream)
            {
                IFormatter formatter = new BinaryFormatter();
                stream.Position = 0;
                return (T)formatter.Deserialize(stream);

            }

            /// <summary>
            /// Creates a hard copy of object by serializing and deserializing it from stream
            /// </summary>
            /// <param name="source">Object to copy</param>
            /// <returns>Returns hard copy of an object</returns>
            public static T CloneBySerialization(T source)
            {
                return Deserialize(Serialize(source));
            }

            /// <summary>
            /// Asserts that object is serializable and serialization deserialization is working properly
            /// </summary>
            /// <param name="source">Object to assert</param>
            public static void AssertRoundTripSerializationIsPossible(T source)
            {
                if (!typeof(T).IsSerializable && !(typeof(ISerializable).IsAssignableFrom(typeof(T))))
                    throw new InvalidOperationException("A serializable Type is required");

                // assumes T implements Equals
                T clone = CloneBySerialization(source);
                Assert.AreEqual(source, clone, "Failed round-trip serialization, clone not equal to source");
                Assert.IsFalse(ReferenceEquals(source, clone), "Failed round-trip serialization, clone points to souce");
            }
        }

        [TestMethod]
        public void Settings_IsSerializable()
        {
            var settingsObject = new Settings();
            GenericSerializeAsserter<Settings>.AssertRoundTripSerializationIsPossible(settingsObject);
        }
    }
}
