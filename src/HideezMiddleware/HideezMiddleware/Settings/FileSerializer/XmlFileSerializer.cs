using Hideez.SDK.Communication.Log;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace HideezMiddleware.Settings
{
    public class XmlFileSerializer : IFileSerializer
    {
        private readonly object lockObj = new object();
        private readonly ILog log;

        public XmlFileSerializer(ILog log)
        {
            this.log = log;
        }

        public T Deserialize<T>(string filePath) where T : new()
        {
            T model = default(T);

            try
            {
                if (File.Exists(filePath))
                {
                    lock (lockObj)
                    {
                        // Create a new file stream for reading the XML file
                        using (FileStream readFileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        {
                            try
                            {
                                // Create a new XmlSerializer instance with the type of the test class
                                XmlSerializer serializerObj = new XmlSerializer(typeof(T));

                                // Load the object saved above by using the Deserialize function
                                model = (T)serializerObj.Deserialize(readFileStream);
                            }
                            catch (Exception e)
                            {
                                log?.WriteLine(nameof(XmlFileSerializer), e);
                            }

                            // Cleanup
                            readFileStream.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log?.WriteLine(nameof(XmlFileSerializer), ex);
            }

            return model;
        }

        public bool Serialize<T>(string filePath, T serializedObject) where T : new()
        {
            XmlWriterSettings xws = new XmlWriterSettings()
            {
                NewLineOnAttributes = true,
                Indent = true,
                IndentChars = "  ",
                Encoding = Encoding.UTF8
            };

            lock (lockObj)
            {
                // Create a new file stream to write the serialized object to a file
                using (XmlWriter xw = XmlWriter.Create(filePath, xws))
                {
                    // Create a new XmlSerializer instance with the type of the test class
                    XmlSerializer serializerObj = new XmlSerializer(typeof(T));

                    serializerObj.Serialize(xw, serializedObject);
                }
            }

            return true;
        }
    }
}

