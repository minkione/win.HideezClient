using HideezMiddleware.Settings;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.IO;
using System.Threading.Tasks;

namespace HideezMiddleware.Tests
{
    // todo: Update tests to use TempTestFolder
    [TestClass]
    public class SettingsManagerTests
    {
        private class TestSettings : BaseSettings
        {
            [Setting]
            public int IntProperty { get; set; } = 0;

            public override object Clone()
            {
                return new TestSettings { IntProperty = IntProperty };
            }
        }

        // Arrange step helpers
        private string GetAbsoluteFormattedPath()
        {
            return $"A:\\Directory\\filename.xml";
        }

        private TestSettings GetDefaultSettings()
        {
            return new TestSettings();
        }

        private TestSettings GetSerializedSettings()
        {
            return new TestSettings { IntProperty = 854 };
        }

        private SettingsManager<TestSettings> SetupSettingsManager()
        {
            return new SettingsManager<TestSettings>(GetAbsoluteFormattedPath(), null);
        }

        private SettingsManager<TestSettings> SetupSettingsManager(IFileSerializer fileSerializer)
        {
            return new SettingsManager<TestSettings>(GetAbsoluteFormattedPath(), fileSerializer);
        }

        private Mock<IFileSerializer> SetupFileSerializerMock()
        {
            var serializerMock = new Mock<IFileSerializer>();

            serializerMock.Setup(m => m.Serialize(GetAbsoluteFormattedPath(), GetSerializedSettings())).Returns(true);

            serializerMock.Setup(m => m.Deserialize<TestSettings>(GetAbsoluteFormattedPath())).Returns(GetSerializedSettings());

            return serializerMock;
        }

        // Assert step helpers
        private void AssertAreAllNotNull(params object[] objects)
        {
            for (int i = 0; i < objects.Length; i++)
            {
                Assert.IsNotNull(objects[i]);
            }
        }

        private void AssertAreAllEqual(bool allowNull, params object[] objects)
        {
            if (!allowNull)
                AssertAreAllNotNull(objects);

            for (int i = 0; i < objects.Length - 1; i++)
            {
                for (int j = i + 1; j < objects.Length; j++)
                {
                    Assert.AreEqual(objects[i], objects[j]);
                }
            }
        }


        [TestMethod]
        public void SettingsManager_DefaultSettingsFilePath()
        {
            // Arrange
            var settingsManager = SetupSettingsManager();

            // Act phase is empty, default path is set in default constructor

            // Assert
            Assert.IsFalse(string.IsNullOrWhiteSpace(settingsManager.SettingsFilePath));
        }

        [TestMethod]
        public void SettingsFilePath_SetAbsolutePath_Success()
        {
            // Arrange
            var settingsManager = SetupSettingsManager();

            // Act
            settingsManager.SettingsFilePath = GetAbsoluteFormattedPath();

            // Assert
            Assert.AreEqual(settingsManager.SettingsFilePath, GetAbsoluteFormattedPath());
        }

        [TestMethod]
        public void SettingsFilePath_SetRelativePath_Success()
        {
            // Arrange
            var settingsManager = SetupSettingsManager();
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
            var settingsManager = SetupSettingsManager();

            // Act
            settingsManager.SettingsFilePath = string.Empty;

            // Assert phase is empty: expecting exception, nothing to assert
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void SettingsFilePath_SetIncorrectFileName_Exception()
        {
            // Arrange
            var settingsManager = SetupSettingsManager();
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
            var settingsManager = SetupSettingsManager();
            var path = Path.GetTempPath();

            // Act
            settingsManager.SettingsFilePath = path;

            // Assert phase is empty: expecting exception, nothing to assert
        }

        [TestMethod]
        public async Task SettingsManager_LoadSettings_SettingsDeserialized()
        {
            // Arrange
            var serializerMock = SetupFileSerializerMock();
            var settingsManager = SetupSettingsManager(serializerMock.Object);

            // Act
            var saveSettingsResult = await settingsManager.LoadSettingsAsync();

            // Assert
            serializerMock.Verify(m => m.Deserialize<TestSettings>(GetAbsoluteFormattedPath()));
        }

        [TestMethod]
        public async Task LoadSettings_SettingsNotLoaded_SettingsLoaded()
        {
            // Arrange
            var serializedSettings = GetSerializedSettings();
            var settingsManager = SetupSettingsManager(SetupFileSerializerMock().Object);

            // Act
            var loadSettingsResult = await settingsManager.LoadSettingsAsync();

            // Assert
            AssertAreAllEqual(false, settingsManager.Settings, loadSettingsResult, serializedSettings);
            //Assert.IsNotNull(settingsManager.Settings);
            //Assert.IsNotNull(loadSettingsResult);
            //Assert.AreEqual(loadSettingsResult, serializedSettings);
            //Assert.AreEqual(loadSettingsResult, settingsManager.Settings);
            //Assert.AreEqual(serializedSettings, settingsManager.Settings);
        }

        [TestMethod]
        public void SettingsManager_SaveSettings_SettingsSerialized()
        {
            // Arrange
            var serializerMock = SetupFileSerializerMock();
            var settingsManager = SetupSettingsManager(serializerMock.Object);

            // Act
            var saveSettingsResult = settingsManager.SaveSettings(GetSerializedSettings());

            // Assert
            serializerMock.Verify(m => m.Serialize(GetAbsoluteFormattedPath(), GetSerializedSettings()));
        }

        [TestMethod]
        public void SaveSettings_SettingsNotLoaded_SettingsUpdated()
        {
            // Arrange
            var defaultSettings = GetDefaultSettings();
            var serializedSettings = GetSerializedSettings();
            var settingsManager = SetupSettingsManager(SetupFileSerializerMock().Object);

            // Act
            var saveSettingsResult = settingsManager.SaveSettings(GetSerializedSettings());

            // Assert
            //Assert.IsNotNull(settingsManager.Settings);
            //Assert.IsNotNull(saveSettingsResult);

            Assert.AreNotEqual(defaultSettings, settingsManager.Settings);

            AssertAreAllEqual(false, settingsManager.Settings, serializedSettings, saveSettingsResult);
            //Assert.AreEqual(saveSettingsResult, settingsManager.Settings);
            //Assert.AreEqual(saveSettingsResult, serializedSettings);
            //Assert.AreEqual(settingsManager.Settings, serializedSettings);
        }

        [TestMethod]
        public async Task SaveSettings_SettingsLoaded_SettingsUpdated()
        {
            // Arrange
            var defaultSettings = GetDefaultSettings();
            var serializedSettings = GetSerializedSettings();

            var serializerMock = new Mock<IFileSerializer>();
            serializerMock.SetupSequence(m => m.Deserialize<TestSettings>(GetAbsoluteFormattedPath()))
                .Returns(defaultSettings)
                .Returns(serializedSettings);
            serializerMock.Setup(m => m.Serialize(GetAbsoluteFormattedPath(), serializedSettings)).Returns(true);

            var settingsManager = SetupSettingsManager(serializerMock.Object);

            var loadSettingsResult = await settingsManager.LoadSettingsAsync();

            // Act
            var saveSettingsResult = settingsManager.SaveSettings(serializedSettings);

            // Assert
            //Assert.IsNotNull(settingsManager.Settings);
            //Assert.IsNotNull(saveSettingsResult);

            Assert.AreEqual(defaultSettings, loadSettingsResult);

            Assert.AreNotEqual(defaultSettings, settingsManager.Settings);
            Assert.AreNotEqual(defaultSettings, saveSettingsResult);

            AssertAreAllEqual(false, settingsManager.Settings, serializedSettings, saveSettingsResult);
            //Assert.AreEqual(saveSettingsResult, settingsManager.Settings);
            //Assert.AreEqual(saveSettingsResult, serializedSettings);
            //Assert.AreEqual(settingsManager.Settings, serializedSettings);
        }

        [TestMethod]
        public async Task GetSettings_SettingsNotLoaded_SettingsUpdated()
        {
            // Arrange
            var defaultSettings = GetDefaultSettings();
            var serializedSettings = GetSerializedSettings();
            var settingsManager = SetupSettingsManager(SetupFileSerializerMock().Object);

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
            var defaultSettings = GetDefaultSettings();
            var serializedSettings = GetSerializedSettings();
            var settingsManager = SetupSettingsManager(SetupFileSerializerMock().Object);

            var loadSettingsResult = await settingsManager.LoadSettingsAsync();

            // Act
            var getSettingsResult = await settingsManager.GetSettingsAsync();

            // Assert
            //Assert.IsNotNull(settingsManager.Settings);
            //Assert.IsNotNull(getSettingsResult);

            Assert.AreNotEqual(defaultSettings, settingsManager.Settings);
            Assert.AreNotEqual(defaultSettings, loadSettingsResult);
            Assert.AreNotEqual(defaultSettings, getSettingsResult);

            AssertAreAllEqual(false, settingsManager.Settings, getSettingsResult, loadSettingsResult);
            //Assert.AreEqual(getSettingsResult, settingsManager.Settings);
            //Assert.AreEqual(loadSettingsResult, settingsManager.Settings);
            //Assert.AreEqual(loadSettingsResult, getSettingsResult);
        }

        [TestMethod]
        public async Task LoadSettings_DeserializationFailed_DefaultReturned()
        {
            // Arrange
            var defaultSettings = GetDefaultSettings();

            var serializerMock = new Mock<IFileSerializer>();
            serializerMock.Setup(m => m.Deserialize<TestSettings>(GetAbsoluteFormattedPath())).Returns(default(TestSettings));

            var settingsManager = SetupSettingsManager(serializerMock.Object);

            // Act
            var loadSettingsResult = await settingsManager.LoadSettingsAsync();

            // Assert
            AssertAreAllEqual(false, defaultSettings, settingsManager.Settings, loadSettingsResult);
            //Assert.IsNotNull(settingsManager.Settings);
            //Assert.IsNotNull(loadSettingsResult);

            //Assert.AreEqual(defaultSettings, settingsManager.Settings);
            //Assert.AreEqual(defaultSettings, loadSettingsResult);
            //Assert.AreEqual(settingsManager.Settings, loadSettingsResult);
        }

        [TestMethod]
        public async Task GetSettings_DeserializationFailed_DefaultReturned()
        {
            // Arrange
            var defaultSettings = GetDefaultSettings();

            var serializerMock = new Mock<IFileSerializer>();
            serializerMock.Setup(m => m.Deserialize<TestSettings>(GetAbsoluteFormattedPath())).Returns(default(TestSettings));

            var settingsManager = SetupSettingsManager(serializerMock.Object);

            // Act
            var getSettingsResult = await settingsManager.GetSettingsAsync();

            // Assert
            AssertAreAllEqual(false, defaultSettings, settingsManager.Settings, getSettingsResult);
            //Assert.IsNotNull(settingsManager.Settings);
            //Assert.IsNotNull(getSettingsResult);

            //Assert.AreEqual(settingsManager.Settings, getSettingsResult);
            //Assert.AreEqual(defaultSettings, settingsManager.Settings);
            //Assert.AreEqual(defaultSettings, getSettingsResult);
        }

        [TestMethod]
        public void Settings_NotLoaded_Default()
        {
            // Arrange
            var defaultSettings = GetDefaultSettings();
            var serializedSettings = GetSerializedSettings();
            var settingsManager = SetupSettingsManager(SetupFileSerializerMock().Object);

            // Act
            var settings = settingsManager.Settings;

            // Assert
            Assert.AreEqual(settings, new TestSettings()); // default(ApplicationSettings)
        }

        [TestMethod]
        public async Task Settings_DefaultSettingsLoaded_DeepCopyCreated()
        {
            // Arrange
            var defaultSettings = GetDefaultSettings();
            var serializedSettings = GetSerializedSettings();
            var settingsManager = SetupSettingsManager(SetupFileSerializerMock().Object);

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
            var defaultSettings = GetDefaultSettings();
            var serializedSettings = GetSerializedSettings();
            var settingsManager = SetupSettingsManager(SetupFileSerializerMock().Object);

            await settingsManager.LoadSettingsAsync();

            // Act
            var settingsReference1 = await settingsManager.GetSettingsAsync();
            var settingsReference2 = await settingsManager.GetSettingsAsync();

            // Assert
            Assert.IsNotNull(settingsReference1);
            Assert.IsNotNull(settingsReference2);

            Assert.AreNotSame(settingsReference1, settingsReference2);
        }

        [TestMethod]
        public async Task LoadSettings_SettingsNotLoaded_MessengerCalled()
        {
            // Arrange
            var serializer = SetupFileSerializerMock();
            var settingsManager = SetupSettingsManager(serializer.Object);
            bool isSettingsChanged = false;
            settingsManager.SettingsChanged += (s, a) => isSettingsChanged = true;

            // Act
            await settingsManager.LoadSettingsAsync();

            // Assert
            Assert.IsTrue(isSettingsChanged);
        }

        [TestMethod]
        public async Task GetSettings_SettingsNotLoaded_MessengerCalled()
        {
            // Arrange
            var serializer = SetupFileSerializerMock();
            var settingsManager = SetupSettingsManager(serializer.Object);
            bool isSettingsChanged = false;
            settingsManager.SettingsChanged += (s, a) => isSettingsChanged = true;

            // Act
            await settingsManager.GetSettingsAsync();

            // Assert
            Assert.IsTrue(isSettingsChanged);
        }

        [TestMethod]
        public void InitializeFileStruct_CorrectPath_DirectoryCreated()
        {
            // Arrange
            var serializer = SetupFileSerializerMock();
            var settingsManager = SetupSettingsManager(serializer.Object);

            var directory = "c:\\SettingsManagerTest";
            var file = "Setting.dat";
            var path = Path.Combine(directory, file);

            settingsManager.SettingsFilePath = path;

            try
            {
                if (Directory.Exists(directory))
                    Directory.Delete(directory);

                // Act
                settingsManager.InitializeFileStruct();

                // Assert
                Assert.IsTrue(Directory.Exists(directory));
            }
            finally
            {
                // Cleanup
                if (Directory.Exists(directory))
                    Directory.Delete(directory);
            }
        }
    }
}