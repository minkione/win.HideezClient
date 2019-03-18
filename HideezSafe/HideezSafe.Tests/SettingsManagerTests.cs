using System;
using System.IO;
using System.Threading.Tasks;
using HideezSafe.Models.Settings;
using HideezSafe.Modules.FileSerializer;
using HideezSafe.Modules.SettingsManager;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace HideezSafe.Tests
{
    // Todo: Remove duplicate code from arrangement and assertions
    [TestClass]
    public class SettingsManagerTests
    {
        private readonly string absoluteFormattedPath;

        public SettingsManagerTests()
        {
            absoluteFormattedPath = $"C:\\{Path.GetRandomFileName()}\\{Path.GetRandomFileName()}.xml";
        }

        [TestMethod]
        public void SettingsManager_DefaultSettingsFilePath()
        {
            // Arrange
            var settingsManager = new SettingsManager<ApplicationSettings>();

            // Act phase is empty, default path is set in default constructor

            // Assert
            Assert.IsFalse(string.IsNullOrWhiteSpace(settingsManager.SettingsFilePath));
        }

        [TestMethod]
        public void SettingsFilePath_SetAbsolutePath_Success()
        {
            // Arrange
#pragma warning disable IDE0017 // Simplify object initialization
            var settingsManager = new SettingsManager<ApplicationSettings>();
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
            var settingsManager = new SettingsManager<ApplicationSettings>();
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
            var settingsManager = new SettingsManager<ApplicationSettings>();
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
            var settingsManager = new SettingsManager<ApplicationSettings>();
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
            var settingsManager = new SettingsManager<ApplicationSettings>();
            var path = Path.GetTempPath();

            // Act
            settingsManager.SettingsFilePath = path;

            // Assert phase is empty: expecting exception, nothing to assert
        }

        [TestMethod]
        public async Task SettingsManager_LoadSettings_SettingsDeserialized()
        {
            // Arrange
            var serializedSettings = new ApplicationSettings() { IsFirstLaunch = false, LaunchApplicationOnStartup = false, SelectedUiLanguage = "pl-PL" };

            var serializerMock = new Mock<IFileSerializer>();
            serializerMock.Setup(m => m.Deserialize<ApplicationSettings>(absoluteFormattedPath)).Returns(serializedSettings);

            var settingsManager = new SettingsManager<ApplicationSettings>(absoluteFormattedPath)
            {
                FileSerializer = serializerMock.Object
            };

            // Act
            var saveSettingsResult = await settingsManager.LoadSettingsAsync();

            // Assert
            serializerMock.Verify(m => m.Deserialize<ApplicationSettings>(absoluteFormattedPath));
        }

        [TestMethod]
        public async Task LoadSettings_SettingsNotLoaded_SettingsLoaded()
        {
            // Arrange
            var defaultSettings = new ApplicationSettings();

            var serializedSettings = new ApplicationSettings() { IsFirstLaunch = false, LaunchApplicationOnStartup = false, SelectedUiLanguage = "pl-PL" };
            var serializerMock = new Mock<IFileSerializer>();
            serializerMock.Setup(m => m.Deserialize<ApplicationSettings>(absoluteFormattedPath)).Returns(serializedSettings);

            var settingsManager = new SettingsManager<ApplicationSettings>(absoluteFormattedPath)
            {
                FileSerializer = serializerMock.Object
            };

            // Act
            var loadSettingsResult = await settingsManager.LoadSettingsAsync();

            // Assert
            Assert.IsNotNull(settingsManager.Settings);
            Assert.IsNotNull(loadSettingsResult);
            Assert.AreEqual(loadSettingsResult, serializedSettings);
            Assert.AreEqual(loadSettingsResult, settingsManager.Settings);
            Assert.AreEqual(serializedSettings, settingsManager.Settings);
        }
        
        [TestMethod]
        public void SettingsManager_SaveSettings_SettingsSerialized()
        {
            // Arrange
            var serializedSettings = new ApplicationSettings() { IsFirstLaunch = false, LaunchApplicationOnStartup = false, SelectedUiLanguage = "pl-PL" };

            var serializerMock = new Mock<IFileSerializer>();
            serializerMock.Setup(m => m.Serialize(absoluteFormattedPath, serializedSettings)).Returns(true);

            var settingsManager = new SettingsManager<ApplicationSettings>(absoluteFormattedPath)
            {
                FileSerializer = serializerMock.Object
            };

            // Act
            var saveSettingsResult = settingsManager.SaveSettings(serializedSettings);

            // Assert
            serializerMock.Verify(m => m.Serialize(absoluteFormattedPath, serializedSettings));
        }

        [TestMethod]
        public void SaveSettings_SettingsNotLoaded_SettingsUpdated()
        {
            // Arrange
            var defaultSettings = new ApplicationSettings();
            var serializedSettings = new ApplicationSettings() { IsFirstLaunch = false, LaunchApplicationOnStartup = false, SelectedUiLanguage = "pl-PL" };

            var serializerMock = new Mock<IFileSerializer>();
            serializerMock.Setup(m => m.Serialize(absoluteFormattedPath, serializedSettings)).Returns(true);
            serializerMock.Setup(m => m.Deserialize<ApplicationSettings>(absoluteFormattedPath)).Returns(serializedSettings);

            var settingsManager = new SettingsManager<ApplicationSettings>(absoluteFormattedPath)
            {
                FileSerializer = serializerMock.Object
            };

            // Act
            var saveSettingsResult = settingsManager.SaveSettings(serializedSettings);

            // Assert
            Assert.IsNotNull(settingsManager.Settings);
            Assert.IsNotNull(saveSettingsResult);
            Assert.AreNotEqual(defaultSettings, settingsManager.Settings);
            Assert.AreEqual(saveSettingsResult, settingsManager.Settings);
            Assert.AreEqual(saveSettingsResult, serializedSettings);
            Assert.AreEqual(serializedSettings, settingsManager.Settings);
        }

        [TestMethod]
        public async Task SaveSettings_SettingsLoaded_SettingsUpdated()
        {
            // Arrange
            var defaultSettings = new ApplicationSettings();
            var serializedSettings = new ApplicationSettings() { IsFirstLaunch = false, LaunchApplicationOnStartup = false, SelectedUiLanguage = "pl-PL" };
            
            var serializerMock = new Mock<IFileSerializer>();
            serializerMock.SetupSequence(m => m.Deserialize<ApplicationSettings>(absoluteFormattedPath))
                .Returns(defaultSettings)
                .Returns(serializedSettings);
            serializerMock.Setup(m => m.Serialize(absoluteFormattedPath, serializedSettings)).Returns(true);

            var settingsManager = new SettingsManager<ApplicationSettings>(absoluteFormattedPath)
            {
                FileSerializer = serializerMock.Object
            };

            var loadSettingsResult = await settingsManager.LoadSettingsAsync();

            // Act
            var saveSettingsResult = settingsManager.SaveSettings(serializedSettings);

            // Assert
            Assert.IsNotNull(settingsManager.Settings);
            Assert.IsNotNull(saveSettingsResult);
            Assert.AreEqual(loadSettingsResult, defaultSettings);
            Assert.AreNotEqual(defaultSettings, settingsManager.Settings);
            Assert.AreNotEqual(saveSettingsResult, defaultSettings);
            Assert.AreEqual(saveSettingsResult, settingsManager.Settings);
            Assert.AreEqual(saveSettingsResult, serializedSettings);
            Assert.AreEqual(serializedSettings, settingsManager.Settings);
        }

        [TestMethod]
        public async Task GetSettings_SettingsNotLoaded_SettingsUpdated()
        {
            // Arrange
            var defaultSettings = new ApplicationSettings();
            var serializedSettings = new ApplicationSettings() { IsFirstLaunch = false, LaunchApplicationOnStartup = false, SelectedUiLanguage = "pl-PL" };

            var serializerMock = new Mock<IFileSerializer>();
            serializerMock.Setup(m => m.Serialize(absoluteFormattedPath, serializedSettings)).Returns(true);
            serializerMock.Setup(m => m.Deserialize<ApplicationSettings>(absoluteFormattedPath)).Returns(serializedSettings);

            var settingsManager = new SettingsManager<ApplicationSettings>(absoluteFormattedPath)
            {
                FileSerializer = serializerMock.Object
            };

            // Act
            var loadSettingsResult = await settingsManager.GetSettingsAsync();

            // Assert
            Assert.IsNotNull(settingsManager.Settings);
            Assert.IsNotNull(loadSettingsResult);
            Assert.AreNotEqual(defaultSettings, loadSettingsResult);
            Assert.AreEqual(loadSettingsResult, settingsManager.Settings);
            Assert.AreNotEqual(defaultSettings, settingsManager.Settings);
        }

        [TestMethod]
        public async Task GetSettings_SettingsLoaded_SettingsNotUpdated()
        {
            // Arrange
            var defaultSettings = new ApplicationSettings();
            var serializedSettings = new ApplicationSettings() { IsFirstLaunch = false, LaunchApplicationOnStartup = false, SelectedUiLanguage = "pl-PL" };

            var serializerMock = new Mock<IFileSerializer>();
            serializerMock.Setup(m => m.Serialize(absoluteFormattedPath, serializedSettings)).Returns(true);
            serializerMock.Setup(m => m.Deserialize<ApplicationSettings>(absoluteFormattedPath)).Returns(serializedSettings);

            var settingsManager = new SettingsManager<ApplicationSettings>(absoluteFormattedPath)
            {
                FileSerializer = serializerMock.Object
            };

            var loadSettingsResult = await settingsManager.LoadSettingsAsync();

            // Act
            var getSettingsResult = await settingsManager.GetSettingsAsync();

            // Assert
            Assert.IsNotNull(settingsManager.Settings);
            Assert.IsNotNull(getSettingsResult);
            Assert.AreNotEqual(defaultSettings, settingsManager.Settings);
            Assert.AreNotEqual(defaultSettings, loadSettingsResult);
            Assert.AreNotEqual(defaultSettings, getSettingsResult);
            Assert.AreEqual(loadSettingsResult, settingsManager.Settings);
            Assert.AreEqual(getSettingsResult, settingsManager.Settings);
            Assert.AreEqual(loadSettingsResult, getSettingsResult);
        }

        [TestMethod]
        public async Task LoadSettings_DeserializationFailed_DefaultReturned()
        {
            // Arrange
            var defaultSettings = new ApplicationSettings();

            var serializerMock = new Mock<IFileSerializer>();
            serializerMock.Setup(m => m.Deserialize<ApplicationSettings>(absoluteFormattedPath)).Returns(default(ApplicationSettings));

            var settingsManager = new SettingsManager<ApplicationSettings>(absoluteFormattedPath)
            {
                FileSerializer = serializerMock.Object
            };

            // Act
            var loadSettingsResult = await settingsManager.LoadSettingsAsync();

            // Assert
            Assert.IsNotNull(settingsManager.Settings);
            Assert.IsNotNull(loadSettingsResult);
            Assert.AreEqual(defaultSettings, settingsManager.Settings);
            Assert.AreEqual(loadSettingsResult, settingsManager.Settings);
            Assert.AreEqual(defaultSettings, loadSettingsResult);
        }

        [TestMethod]
        public async Task GetSettings_DeserializationFailed_DefaultReturned()
        {
            // Arrange
            var defaultSettings = new ApplicationSettings();

            var serializerMock = new Mock<IFileSerializer>();
            serializerMock.Setup(m => m.Deserialize<ApplicationSettings>(absoluteFormattedPath)).Returns(default(ApplicationSettings));

            var settingsManager = new SettingsManager<ApplicationSettings>(absoluteFormattedPath)
            {
                FileSerializer = serializerMock.Object
            };

            // Act
            var getSettingsResult = await settingsManager.GetSettingsAsync();

            // Assert
            Assert.IsNotNull(settingsManager.Settings);
            Assert.IsNotNull(getSettingsResult);
            Assert.AreEqual(defaultSettings, settingsManager.Settings);
            Assert.AreEqual(getSettingsResult, settingsManager.Settings);
            Assert.AreEqual(defaultSettings, getSettingsResult);
        }

        [TestMethod]
        public void Settings_NotLoaded_Default()
        {
            // Arrange
            var serializerMock = new Mock<IFileSerializer>();
            serializerMock.Setup(m => m.Deserialize<ApplicationSettings>(absoluteFormattedPath)).Returns(default(ApplicationSettings));

            var settingsManager = new SettingsManager<ApplicationSettings>(absoluteFormattedPath)
            {
                FileSerializer = serializerMock.Object
            };

            // Act
            var settings = settingsManager.Settings;

            // Assert
            Assert.AreEqual(settings, default(ApplicationSettings));
        }

        [TestMethod]
        public async Task Settings_DefaultSettingsLoaded_DeepCopyCreated()
        {
            // Arrange
            var serializerMock = new Mock<IFileSerializer>();
            serializerMock.Setup(m => m.Deserialize<ApplicationSettings>(absoluteFormattedPath)).Returns(default(ApplicationSettings));

            var settingsManager = new SettingsManager<ApplicationSettings>(absoluteFormattedPath)
            {
                FileSerializer = serializerMock.Object
            };

            await settingsManager.LoadSettingsAsync();
            
            // Act
            var settingsReference1 = settingsManager.Settings;
            var settingsReference2 = settingsManager.Settings;

            // Assert
            Assert.IsNotNull(settingsReference1);
            Assert.IsNotNull(settingsReference2);
            Assert.AreNotSame(settingsReference1, settingsReference2);
        }

        [TestMethod]
        public async Task GetSettings_DefaultSettingsLoaded_DeepCopyCreated()
        {
            // Arrange
            var serializerMock = new Mock<IFileSerializer>();
            serializerMock.Setup(m => m.Deserialize<ApplicationSettings>(absoluteFormattedPath)).Returns(default(ApplicationSettings));

            var settingsManager = new SettingsManager<ApplicationSettings>(absoluteFormattedPath)
            {
                FileSerializer = serializerMock.Object
            };

            await settingsManager.LoadSettingsAsync();

            // Act
            var settingsReference1 = await settingsManager.GetSettingsAsync();
            var settingsReference2 = await settingsManager.GetSettingsAsync();

            // Assert
            Assert.IsNotNull(settingsReference1);
            Assert.IsNotNull(settingsReference2);
            Assert.AreNotSame(settingsReference1, settingsReference2);
        }


    }
}
