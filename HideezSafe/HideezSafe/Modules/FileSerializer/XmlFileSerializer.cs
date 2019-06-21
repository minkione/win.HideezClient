using NLog;
using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace HideezSafe.Modules.FileSerializer
{
    class XmlFileSerializer : IFileSerializer
    {
        ILogger _log = LogManager.GetCurrentClassLogger();

        public T Deserialize<T>(string filePath) where T : new()
        {
            T model = default(T);

            try
            {
                if (File.Exists(filePath))
                {
                    // Create a new file stream for reading the XML file
                    using (FileStream readFileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
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
                            _log.Error(e);
                        }

                        // Cleanup
                        readFileStream.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex);
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

            // Create a new file stream to write the serialized object to a file
            using (XmlWriter xw = XmlWriter.Create(filePath, xws))
            {
                // Create a new XmlSerializer instance with the type of the test class
                XmlSerializer serializerObj = new XmlSerializer(typeof(T));

                serializerObj.Serialize(xw, serializedObject);
            }

            return true;
        }
    }
}
