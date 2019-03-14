using System;
using System.IO;
using HideezSafe.Models.Settings;
using HideezSafe.Modules.FileSerializer;
using HideezSafe.Modules.SettingsManager;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace HideezSafe.Tests
{
    [TestClass]
    public class SettingsManagerTests
    {
        private readonly string absoluteFormattedPath = $"C:\\{Path.GetRandomFileName()}\\{Path.GetRandomFileName()}.xml";

        [TestMethod]
        public void SettingsManager_DefaultSettingsFilePath()
        {
            // Arrange
            var settingsManager = new SettingsManager();

            // Act phase is empty, default path is set in default constructor

            // Assert
            Assert.IsFalse(string.IsNullOrWhiteSpace(settingsManager.SettingsFilePath));
        }

        [TestMethod]
        public void SettingsFilePath_SetAbsolutePath_Success()
        {
            // Arrange
#pragma warning disable IDE0017 // Simplify object initialization
            var settingsManager = new SettingsManager();
#pragma warning restore IDE0017 // Simplify object initialization

            // Act
            settingsManager.SettingsFilePath = absoluteFormattedPath;

            // Assert
            Assert.AreEqual(settingsManager.SettingsFilePath, absoluteFormattedPath);
        }

        [TestMethod]
        public void SettingsFilePath_SetRelativePath_Success()
        {
            // Arrange
            var settingsManager = new SettingsManager();
            var relativePath = "settings.txt";

            // Act
            settingsManager.SettingsFilePath = relativePath;

            // Assert
            Assert.AreEqual(settingsManager.SettingsFilePath, relativePath);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SettingsFilePath_SetEmptyPath_Exception()
        {
            // Arrange
#pragma warning disable IDE0017 // Simplify object initialization
            var settingsManager = new SettingsManager();
#pragma warning restore IDE0017 // Simplify object initialization

            // Act
            settingsManager.SettingsFilePath = string.Empty;

            // Assert phase is empty: expecting exception, nothing to assert
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void SettingsFilePath_SetIncorrectFileName_Exception()
        {
            // Arrange
            var settingsManager = new SettingsManager();
            var incorrectPath = "S:\\SomeMagicalPath\\some<><>file.xml";

            // Act
            settingsManager.SettingsFilePath = incorrectPath;

            // Assert phase is empty: expecting exception, nothing to assert
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void SettingsFilePath_NoFilenamePath_Exception()
        {
            // Arrange
            var settingsManager = new SettingsManager();
            var path = Path.GetTempPath();

            // Act
            settingsManager.SettingsFilePath = path;

            // Assert phase is empty: expecting exception, nothing to assert
        }

        [TestMethod]
        public void SettingsManager_LoadSettings_SettingsDeserialized()
        {
            // Arrange
            var defaultSettings = new Settings();
            var serializedSettings = new Settings() { FirstLaunch = false, LaunchOnStartup = false, SelectedLanguage = "pl-PL" };

            var serializerMock = new Mock<IFileSerializer>();
            bool settingsDeserialized = false;
            serializerMock.Setup(m => m.Deserialize<Settings>(absoluteFormattedPath)).Callback(() => { settingsDeserialized = true; }).Returns(serializedSettings);

            var settingsManager = new SettingsManager(absoluteFormattedPath)
            {
                FileSerializer = serializerMock.Object
            };

            // Act
            var saveSettingsResult = settingsManager.LoadSettingsAsync().Result;

            // Assert
            Assert.IsTrue(settingsDeserialized);
        }

        [TestMethod]
        public void LoadSettings_SettingsNotLoaded_SettingsLoaded()
        {
            // Arrange
            var defaultSettings = new Settings();

            var serializedSettings = new Settings() { FirstLaunch = false, LaunchOnStartup = false, SelectedLanguage = "pl-PL" };
            var serializerMock = new Mock<IFileSerializer>();
            serializerMock.Setup(m => m.Deserialize<Settings>(absoluteFormattedPath)).Returns(serializedSettings);

            var settingsManager = new SettingsManager(absoluteFormattedPath)
            {
                FileSerializer = serializerMock.Object
            };

            // Act
            var loadSettingsResult = settingsManager.LoadSettingsAsync().Result; // Synchronous call

            // Assert
            Assert.IsNotNull(settingsManager.Settings);
            Assert.AreEqual(loadSettingsResult, serializedSettings);
            Assert.AreEqual(loadSettingsResult, settingsManager.Settings);
            Assert.AreEqual(serializedSettings, settingsManager.Settings);
        }
        
        [TestMethod]
        public void SettingsManager_SaveSettings_SettingsSerialized()
        {
            // Arrange
            var defaultSettings = new Settings();
            var serializedSettings = new Settings() { FirstLaunch = false, LaunchOnStartup = false, SelectedLanguage = "pl-PL" };

            var serializerMock = new Mock<IFileSerializer>();
            bool settingsSerialized = false;
            serializerMock.Setup(m => m.Serialize(absoluteFormattedPath, serializedSettings)).Callback(() => { settingsSerialized = true; }).Returns(true);

            var settingsManager = new SettingsManager(absoluteFormattedPath)
            {
                FileSerializer = serializerMock.Object
            };

            // Act
            var saveSettingsResult = settingsManager.SaveSettings(serializedSettings);

            // Assert
            Assert.IsTrue(settingsSerialized);
        }

        [TestMethod]
        public void SaveSettings_SettingsNotLoaded_SettingsUpdated()
        {
            // Arrange
            var defaultSettings = new Settings();
            var serializedSettings = new Settings() { FirstLaunch = false, LaunchOnStartup = false, SelectedLanguage = "pl-PL" };

            var serializerMock = new Mock<IFileSerializer>();
            serializerMock.Setup(m => m.Serialize(absoluteFormattedPath, serializedSettings)).Returns(true);
            serializerMock.Setup(m => m.Deserialize<Settings>(absoluteFormattedPath)).Returns(serializedSettings);

            var settingsManager = new SettingsManager(absoluteFormattedPath)
            {
                FileSerializer = serializerMock.Object
            };

            // Act
            var saveSettingsResult = settingsManager.SaveSettings(serializedSettings);

            // Assert
            Assert.IsNotNull(settingsManager.Settings);
            Assert.AreNotEqual(defaultSettings, settingsManager.Settings);
            Assert.AreEqual(saveSettingsResult, settingsManager.Settings);
            Assert.AreEqual(saveSettingsResult, serializedSettings);
            Assert.AreEqual(serializedSettings, settingsManager.Settings);
        }

        [TestMethod]
        public void SaveSettings_SettingsLoaded_SettingsUpdated()
        {
            // Arrange
            var defaultSettings = new Settings();
            var serializedSettings = new Settings() { FirstLaunch = false, LaunchOnStartup = false, SelectedLanguage = "pl-PL" };
            
            var serializerMock = new Mock<IFileSerializer>();
            serializerMock.SetupSequence(m => m.Deserialize<Settings>(absoluteFormattedPath))
                .Returns(defaultSettings)
                .Returns(serializedSettings);
            serializerMock.Setup(m => m.Serialize(absoluteFormattedPath, serializedSettings)).Returns(true);

            var settingsManager = new SettingsManager(absoluteFormattedPath)
            {
                FileSerializer = serializerMock.Object
            };

            var loadSettingsResult = settingsManager.LoadSettingsAsync().Result;

            // Act
            var saveSettingsResult = settingsManager.SaveSettings(serializedSettings);

            // Assert
            Assert.IsNotNull(settingsManager.Settings);
            Assert.AreEqual(loadSettingsResult, defaultSettings);
            Assert.AreNotEqual(defaultSettings, settingsManager.Settings);
            Assert.AreNotEqual(saveSettingsResult, defaultSettings);
            Assert.AreEqual(saveSettingsResult, settingsManager.Settings);
            Assert.AreEqual(saveSettingsResult, serializedSettings);
            Assert.AreEqual(serializedSettings, settingsManager.Settings);
        }

        [TestMethod]
        public void GetSettings_SettingsNotLoaded_SettingsUpdated()
        {
            // Arrange
            var defaultSettings = new Settings();
            var serializedSettings = new Settings() { FirstLaunch = false, LaunchOnStartup = false, SelectedLanguage = "pl-PL" };

            var serializerMock = new Mock<IFileSerializer>();
            serializerMock.Setup(m => m.Serialize(absoluteFormattedPath, serializedSettings)).Returns(true);
            serializerMock.Setup(m => m.Deserialize<Settings>(absoluteFormattedPath)).Returns(serializedSettings);

            var settingsManager = new SettingsManager(absoluteFormattedPath)
            {
                FileSerializer = serializerMock.Object
            };

            // Act
            var loadSettingsResult = settingsManager.GetSettingsAsync().Result;

            // Assert
            Assert.IsNotNull(settingsManager.Settings);
            Assert.AreNotEqual(defaultSettings, loadSettingsResult);
            Assert.AreEqual(loadSettingsResult, settingsManager.Settings);
            Assert.AreNotEqual(defaultSettings, settingsManager.Settings);
        }

        [TestMethod]
        public void GetSettings_SettingsLoaded_SettingsNotUpdated()
        {
            // Arrange
            var defaultSettings = new Settings();
            var serializedSettings = new Settings() { FirstLaunch = false, LaunchOnStartup = false, SelectedLanguage = "pl-PL" };

            var serializerMock = new Mock<IFileSerializer>();
            serializerMock.Setup(m => m.Serialize(absoluteFormattedPath, serializedSettings)).Returns(true);
            serializerMock.Setup(m => m.Deserialize<Settings>(absoluteFormattedPath)).Returns(serializedSettings);

            var settingsManager = new SettingsManager(absoluteFormattedPath)
            {
                FileSerializer = serializerMock.Object
            };

            var loadSettingsResult = settingsManager.LoadSettingsAsync().Result;

            // Act
            var getSettingsResult = settingsManager.GetSettingsAsync().Result;

            // Assert
            Assert.IsNotNull(settingsManager.Settings);
            Assert.AreNotEqual(defaultSettings, settingsManager.Settings);
            Assert.AreNotEqual(defaultSettings, loadSettingsResult);
            Assert.AreNotEqual(defaultSettings, getSettingsResult);
            Assert.AreEqual(loadSettingsResult, settingsManager.Settings);
            Assert.AreEqual(getSettingsResult, settingsManager.Settings);
            Assert.AreEqual(loadSettingsResult, getSettingsResult);
        }

    }
}
