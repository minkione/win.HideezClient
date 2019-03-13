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
            var settingsManager = new SettingsManager();
            var absolutePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            // Act
            settingsManager.SettingsFilePath = absolutePath;

            // Assert
            Assert.AreEqual(settingsManager.SettingsFilePath, absolutePath);
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
            var settingsManager = new SettingsManager();

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
        public void Settings_LoadSettings_SettingsLoaded()
        {
            // Arrange
            var path = "somepath";
            var defaultSettings = new Settings();

            var serializedSettings = new Settings() { FirstLaunch = false, LaunchOnStartup = false, SelectedLanguage = "pl-PL" };
            var serializerMock = new Mock<IFileSerializer>();
            serializerMock.Setup(m => m.Deserialize<Settings>(path)).Returns(serializedSettings);

            var settingsManager = new SettingsManager(path);
            settingsManager.FileSerializer = serializerMock.Object;

            // Act
            var loadedSettings = settingsManager.LoadSettingsAsync().Result; // Synchronous call

            // Assert
            Assert.IsNotNull(settingsManager.Settings);
            Assert.AreEqual(loadedSettings, serializedSettings);
            Assert.AreEqual(loadedSettings, settingsManager.Settings);
            Assert.AreEqual(serializedSettings, settingsManager.Settings);
        }

        public void Settings_SaveSettings_SettingsSaved()
        {
            // Todo: Settings saving
        }
    }
}
