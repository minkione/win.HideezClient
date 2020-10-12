using Hideez.SDK.Communication.HES.Client;
using Hideez.SDK.Communication.HES.DTO;
using Hideez.SDK.Communication.Interfaces;
using Hideez.SDK.Communication.Log;
using HideezMiddleware.DeviceConnection.Workflow;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;

namespace HideezMiddleware.Tests.DeviceConnectionTests
{
    [TestClass]
    public class AccountsUpdateProcessorTests
    {
        [TestMethod]
        public async Task UpdateAccounts_DeviceNeedUpdateOSAccounts_UpdateHwVaultAccountsReceived()
        {
            // Arrange
            string serialNo = Guid.NewGuid().ToString();
            var deviceMock = new Mock<IDevice>();
            deviceMock.SetupGet(d => d.SerialNo).Returns(serialNo);

            var connectionMock = new Mock<IHesAppConnection>();
            connectionMock.SetupGet(x => x.State).Returns(HesConnectionState.Connected);

            var logMock = new Mock<ILog>();
            AccountsUpdateProcessor accountsUpdateProcessor = new AccountsUpdateProcessor(connectionMock.Object, logMock.Object);

            // Act
            await accountsUpdateProcessor.UpdateAccounts(deviceMock.Object, new HwVaultInfoFromHesDto() { NeedUpdateOSAccounts = true }, true);

            // Assert
            connectionMock.Verify(x => x.UpdateHwVaultAccounts(serialNo, true), Times.Once);
        }
        
        [TestMethod]
        public async Task UpdateAccounts_DeviceNeedUpdateNonOSAccounts_UpdateHwVaultAccountsReceived()
        {
            // Arrange
            string serialNo = Guid.NewGuid().ToString();
            var deviceMock = new Mock<IDevice>();
            deviceMock.SetupGet(d => d.SerialNo).Returns(serialNo);

            var connectionMock = new Mock<IHesAppConnection>();
            connectionMock.SetupGet(x => x.State).Returns(HesConnectionState.Connected);

            var logMock = new Mock<ILog>();
            AccountsUpdateProcessor accountsUpdateProcessor = new AccountsUpdateProcessor(connectionMock.Object, logMock.Object);

            // Act
            await accountsUpdateProcessor.UpdateAccounts(deviceMock.Object, new HwVaultInfoFromHesDto() { NeedUpdateNonOSAccounts = true }, false);

            // Assert
            connectionMock.Verify(x => x.UpdateHwVaultAccounts(serialNo, false), Times.Once);
        }

        [TestMethod]
        public async Task UpdateAccounts_ServerDiconnected_AccountsNotUpdated()
        {
            // Arrange
            string serialNo = Guid.NewGuid().ToString();
            var deviceMock = new Mock<IDevice>();
            deviceMock.SetupGet(d => d.SerialNo).Returns(serialNo);

            var connectionMock = new Mock<IHesAppConnection>();
            connectionMock.SetupGet(x => x.State).Returns(HesConnectionState.Disconnected);

            var logMock = new Mock<ILog>();
            AccountsUpdateProcessor accountsUpdateProcessor = new AccountsUpdateProcessor(connectionMock.Object, logMock.Object);

            // Act
            await accountsUpdateProcessor.UpdateAccounts(deviceMock.Object, new HwVaultInfoFromHesDto() { NeedUpdateNonOSAccounts = true }, false);

            // Assert
            deviceMock.Verify(x => x.RefreshDeviceInfo(), Times.Never);
        }
    }
}
