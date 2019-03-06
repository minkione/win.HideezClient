using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using HideezSafe.Models.Settings;

namespace HideezSafe.Modules.SettingsManager
{
    /// <summary>
    /// This class keeps track of program settings
    /// Settings should loaded on the application start and are kept in cache until updated
    /// See <seealso cref="ISettings"/> and <seealso cref="Settings"/> for information about available settings
    /// </summary>
    class SettingsManager : ISettingsManager
    {
        /// <summary>
        /// Program settings cache
        /// </summary>
        public ISettings Settings { get; private set; } = null;

        /// <summary>
        /// Get settings from cache. If not available, load from file
        /// </summary>
        /// <param name="settingsFilePath">Path to settings file</param>
        /// <returns>Returns loaded program settings</returns>
        public Task<ISettings> GetSettingsAsync(string settingsFilePath)
        {
            if (Settings == null)
                return LoadSettingsAsync(settingsFilePath);
            else
                return Task.FromResult(Settings);
        }

        /// <summary>
        /// Asynchronously load program settings from file
        /// </summary>
        /// <param name="settingsFilePath">Path to settings file</param>
        /// <returns>Returns program settings loaded from file</returns>
        public Task<ISettings> LoadSettingsAsync(string settingsFilePath)
        {
            return Task.Run(() => { return LoadSettings(settingsFilePath); });
        }

        /// <summary>
        /// Save program options into file
        /// </summary>
        /// <param name="settingsFileName">Path to settings file</param>
        /// <param name="optionsModel">Settings that will be saved</param>
        /// <returns>Returns true if successfully saved settings into file; Otherwise throws exception</returns>
        public bool SaveSettings(string settingsFilePath, ISettings settings)
        {
            XmlWriterSettings xws = new XmlWriterSettings()
            {
                NewLineOnAttributes = true,
                Indent = true,
                IndentChars = "  ",
                Encoding = System.Text.Encoding.UTF8
            };

            // Create a new file stream to write the serialized object to a file
            using (XmlWriter xw = XmlWriter.Create(settingsFilePath, xws))
            {
                // Create a new XmlSerializer instance with the type of the test class
                XmlSerializer serializerObj = new XmlSerializer(typeof(Settings));

                serializerObj.Serialize(xw, settings);
            }

            return true;
        }

        /// <summary>
        /// Synchronously load program settings from file
        /// </summary>
        /// <param name="settingsFileName">Path to settings file</param>
        /// <returns>Returns program settings loaded from file</returns>
        private ISettings LoadSettings(string settingsFilePath)
        {
            Settings loadedModel = null;

            try
            {
                if (File.Exists(settingsFilePath))
                {
                    // Create a new file stream for reading the XML file
                    using (FileStream readFileStream = new FileStream(settingsFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        try
                        {
                            // Create a new XmlSerializer instance with the type of the test class
                            XmlSerializer serializerObj = new XmlSerializer(typeof(Settings));

                            // Load the object saved above by using the Deserialize function
                            loadedModel = (Settings)serializerObj.Deserialize(readFileStream);
                        }
                        catch (Exception e)
                        {
                            // Todo: log error with logger
                        }

                        // Cleanup
                        readFileStream.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                // Todo: log error with logger
            }
            finally
            {
                // Load default settings if deserilization failed
                if (loadedModel == null)
                    loadedModel = new Settings();
            }

            return loadedModel;
        }
    }
}
