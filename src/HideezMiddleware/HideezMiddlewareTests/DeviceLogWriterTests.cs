using Hideez.SDK.Communication.Interfaces;
using Hideez.SDK.Communication.Utils;
using HideezMiddleware.DeviceLogging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HideezMiddleware.Tests
{
    public class DeviceLogWriterTests
    {
        const int LOG_BLOCK_LENGTH = 2;

        List<DeviceLogEntry> GenerateEmptryEntries(int count)
        {
            var entries = new List<DeviceLogEntry>();
            for (int i = 0; i < count; i++)
                entries.Add(new DeviceLogEntry());
            return entries;
        }

        List<DeviceLogEntry> GenerateDatedEntries(int count, DateTime startDate)
        {
            var entries = new List<DeviceLogEntry>();
            for (int i = 0; i < count; i++)
                entries.Add(new DeviceLogEntry()
                {
                    Entry = $" | {Guid.NewGuid()}",
                    Time = startDate.AddSeconds(i)
                });
            return entries;
        }

        DailyDeviceLog CombineToDailyLog(params List<DeviceLogEntry>[] entriesParam)
        {
            var dailyLog = new DailyDeviceLog() { Date = DateTime.Today };
            foreach (var entries in entriesParam)
                dailyLog.Entries.AddRange(entries);
            return dailyLog;
        }

        List<string> ReadLogFile(string logFilePath)
        {
            var fileContent = new List<string>();

            using(var fs = File.OpenRead(logFilePath))
            {
                using(var sr = new StreamReader(fs))
                {
                    while (!sr.EndOfStream)
                    {
                        var line = sr.ReadLine();
                        if (!line.StartsWith("="))
                            fileContent.Add(line);
                    }
                }
            }

            return fileContent;
        }

        IDevice GetDeviceMock()
        {
            var mock = new Mock<IDevice>();

            mock.Setup(m => m.Name).Returns("Test Vault");
            mock.Setup(m => m.SerialNo).Returns("703254476205643");
            mock.Setup(m => m.Mac).Returns("1CFDB7F516A8");
            mock.Setup(m => m.BootloaderVersion).Returns(new Version(1, 0, 0));
            mock.Setup(m => m.FirmwareVersion).Returns(new Version(1, 0, 0));
            mock.Setup(m => m.Battery).Returns(100);

            return mock.Object;
        }

        [Test]
        public void SaveLog_SaveNoDatedEntries_LogReprintedAfterSaved()
        {
            using (var tf = new TempTestFolder(nameof(DeviceLogWriterTests)))
            {
                // Arrange
                var deviceLogWriter = new DeviceLogWriter();
                var device = GetDeviceMock();

                var data_A = GenerateEmptryEntries(LOG_BLOCK_LENGTH);
                var data_B = GenerateEmptryEntries(LOG_BLOCK_LENGTH);

                var daily_A = CombineToDailyLog(data_A);
                var daily_B = CombineToDailyLog(data_B);
                var daily_AB = CombineToDailyLog(data_A, data_B);

                var logPath = deviceLogWriter.SaveLog(tf.FolderPath, daily_A, device, true);
                var expected = daily_AB.Entries.Select(d => d.ToString());

                // Act
                deviceLogWriter.SaveLog(tf.FolderPath, daily_B, device, true);

                // Assert
                var actual = ReadLogFile(logPath);

                Assert.IsTrue(actual.SequenceEqual(expected));
            }
        }

        [Test]
        public void SaveLog_SaveNewDatedEntries_LogAppendedToSaved()
        {
            using (var tf = new TempTestFolder(nameof(DeviceLogWriterTests)))
            {
                // Arrange
                var deviceLogWriter = new DeviceLogWriter();
                var device = GetDeviceMock();

                var data_A = GenerateDatedEntries(LOG_BLOCK_LENGTH, DateTime.Today.AddHours(1));
                var data_B = GenerateDatedEntries(LOG_BLOCK_LENGTH, DateTime.Today.AddHours(2));

                var daily_A = CombineToDailyLog(data_A);
                var daily_B = CombineToDailyLog(data_B);
                var daily_AB = CombineToDailyLog(data_A, data_B);

                var logPath = deviceLogWriter.SaveLog(tf.FolderPath, daily_A, device, true);
                var expected = daily_AB.Entries.Select(d => d.ToString());

                // Act
                deviceLogWriter.SaveLog(tf.FolderPath, daily_B, device, true);

                // Assert
                var actual = ReadLogFile(logPath);

                Assert.IsTrue(actual.SequenceEqual(expected));
            }
        }

        [Test]
        public void SaveLog_SaveWithDuplicatedBeginning_ExtendedSinceLastEntry()
        {
            using (var tf = new TempTestFolder(nameof(DeviceLogWriterTests)))
            {
                // Arrange
                var deviceLogWriter = new DeviceLogWriter();
                var device = GetDeviceMock();

                var data_0 = GenerateEmptryEntries(LOG_BLOCK_LENGTH);
                var data_A = GenerateDatedEntries(LOG_BLOCK_LENGTH, DateTime.Today.AddHours(1));
                var data_B = GenerateDatedEntries(LOG_BLOCK_LENGTH, DateTime.Today.AddHours(2));

                var daily_A0 = CombineToDailyLog(data_A, data_0);
                var daily_A0B = CombineToDailyLog(data_A, data_0, data_B);
                // Extended log starts after last dated entry, so one undated block is expected to be duplicated
                var daily_A00B = CombineToDailyLog(data_A, data_0, data_0, data_B); 

                var logPath = deviceLogWriter.SaveLog(tf.FolderPath, daily_A0, device, true);
                var expected = daily_A00B.Entries.Select(d => d.ToString());

                // Act
                deviceLogWriter.SaveLog(tf.FolderPath, daily_A0B, device, true);

                // Assert
                var actual = ReadLogFile(logPath);

                Assert.IsTrue(actual.SequenceEqual(expected));
            }
        }

        [Test]
        public void SaveLog_SaveWithSomeDuplicates_LogExtendedSinceLastEntry()
        {
            using (var tf = new TempTestFolder(nameof(DeviceLogWriterTests)))
            {
                // Arrange
                var deviceLogWriter = new DeviceLogWriter();
                var device = GetDeviceMock();

                var data_0 = GenerateEmptryEntries(LOG_BLOCK_LENGTH);
                var data_A = GenerateDatedEntries(LOG_BLOCK_LENGTH, DateTime.Today.AddHours(1));
                var data_B = GenerateDatedEntries(LOG_BLOCK_LENGTH, DateTime.Today.AddHours(2));
                var data_C = GenerateDatedEntries(LOG_BLOCK_LENGTH, DateTime.Today.AddHours(3));

                var daily_A0B = CombineToDailyLog(data_A, data_0, data_B);
                var daily_B0C = CombineToDailyLog(data_B, data_0, data_C);
                var daily_A0B0C = CombineToDailyLog(data_A, data_0, data_B, data_0, data_C);

                var logPath = deviceLogWriter.SaveLog(tf.FolderPath, daily_A0B, device, true);
                var expected = daily_A0B0C.Entries.Select(d => d.ToString());

                // Act
                deviceLogWriter.SaveLog(tf.FolderPath, daily_B0C, device, true);

                // Assert
                var actual = ReadLogFile(logPath);

                Assert.IsTrue(actual.SequenceEqual(expected));
            }
        }
    }
}
