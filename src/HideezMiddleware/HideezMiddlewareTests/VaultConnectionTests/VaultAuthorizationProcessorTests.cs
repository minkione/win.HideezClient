using Hideez.SDK.Communication.Device;
using Hideez.SDK.Communication.HES.Client;
using Hideez.SDK.Communication.HES.DTO;
using Hideez.SDK.Communication.Interfaces;
using Hideez.SDK.Communication.Log;
using HideezMiddleware.DeviceConnection.Workflow;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;


namespace HideezMiddleware.Tests.VaultConnectionTests
{
    [TestClass]
    public class VaultAuthorizationProcessorTests
    {
        readonly AccessLevel NEED_AUTH_ACCESSLEVEL = new AccessLevel(false, false, true, false, false, false);
        readonly AccessLevel ALL_OK_ACCESSLEVEL = new AccessLevel(false, false, false, false, false, false);

        Mock<IDevice> GetVaultMock(AccessLevel accessLevel)
        {
            var vaultMock = new Mock<IDevice>();
            vaultMock.SetupAllProperties();
            vaultMock.SetupGet(x => x.FirmwareVersion).Returns(new Version());
            vaultMock.Setup(x => x.GetUserProperty<HwVaultConnectionState>(It.IsAny<string>())).Returns(HwVaultConnectionState.Initializing);
            vaultMock.SetupGet(x => x.AccessLevel).Returns(accessLevel);
            vaultMock.Setup(x => x.RefreshDeviceInfo()).Returns(Task.CompletedTask);

            return vaultMock;
        }

        Mock<IHesAppConnection> GetHesAppConnectionMock(HesConnectionState connectionState, Mock<IDevice> vaultMock)
        {
            var hesAppConnectionMock = new Mock<IHesAppConnection>();
            hesAppConnectionMock.SetupGet(x => x.State).Returns(connectionState);

            // When IHesAppConnectio.AuthHwVault is called, we update Vault mock so that
            // when IVault.RefreshDeviceInfo() is called, the AccessLevel of vault will be updated to reflect 
            // successfull execution of IHesAppConnectio.AuthHwVault
            hesAppConnectionMock
                .Setup(x => x.AuthHwVault(It.IsAny<string>()))
                .Returns(Task.CompletedTask)
                .Callback(() =>
                {
                    vaultMock.Setup(x => x.RefreshDeviceInfo()).Callback(() =>
                    {
                        vaultMock.SetupGet(d => d.AccessLevel).Returns(ALL_OK_ACCESSLEVEL);
                    });
                });

            return hesAppConnectionMock;
        }

        [TestMethod]
        public async Task UpdateVaultStatus_AuthRequired_AuthPerformed()
        {
            // Arrange
            var logMock = new Mock<ILog>();

            var uiMock = new Mock<IClientUiManager>();

            var vaultMock = GetVaultMock(NEED_AUTH_ACCESSLEVEL);

            var hesAppConnectionMock = GetHesAppConnectionMock(HesConnectionState.Connected, vaultMock);

            var vaultAuthProcessor = new VaultAuthorizationProcessor(hesAppConnectionMock.Object, uiMock.Object, logMock.Object);

            // Act
            await vaultAuthProcessor.AuthVault(vaultMock.Object, CancellationToken.None);

            // Assert
        }

        [TestMethod]
        [ExpectedException(typeof(WorkflowException))]
        public async Task UpdateVaultStatus_AuthRequired_NoNetwork_ExceptionThrown()
        {
            // Arrange
            var logMock = new Mock<ILog>();

            var uiMock = new Mock<IClientUiManager>();

            var vaultMock = GetVaultMock(NEED_AUTH_ACCESSLEVEL);

            var hesAppConnectionMock = GetHesAppConnectionMock(HesConnectionState.Disconnected, vaultMock);

            var vaultAuthProcessor = new VaultAuthorizationProcessor(hesAppConnectionMock.Object, uiMock.Object, logMock.Object);

            // Act
            await vaultAuthProcessor.AuthVault(vaultMock.Object, CancellationToken.None);

            // Assert
            Assert.Fail("WorkflowException was expected");
        }

        [TestMethod]
        [DataRow(HesConnectionState.Connected)]
        [DataRow(HesConnectionState.Disconnected)]
        public async Task UpdateVaultStatus_AuthNotRequired_StatusUnchanged(HesConnectionState hesConnectionState)
        {
            // Arrange
            var logMock = new Mock<ILog>();

            var uiMock = new Mock<IClientUiManager>();

            var vaultMock = GetVaultMock(ALL_OK_ACCESSLEVEL);

            var hesAppConnectionMock = GetHesAppConnectionMock(hesConnectionState, vaultMock);
            hesAppConnectionMock
                .Setup(x => x.AuthHwVault(It.IsAny<string>()))
                .Throws(new InvalidOperationException("Unexpected request to server"));

            var vaultAuthProcessor = new VaultAuthorizationProcessor(hesAppConnectionMock.Object, uiMock.Object, logMock.Object);

            // Act
            await vaultAuthProcessor.AuthVault(vaultMock.Object, CancellationToken.None);

            // Assert
        }

        [TestMethod]
        [ExpectedException(typeof(WorkflowException))]
        public async Task UpdateVaultStatus_AuthRequired_HesAlgorithmError_ExceptionThrown()
        {
            // Arrange
            var logMock = new Mock<ILog>();

            var uiMock = new Mock<IClientUiManager>();

            var vaultMock = GetVaultMock(NEED_AUTH_ACCESSLEVEL);

            var hesAppConnectionMock = new Mock<IHesAppConnection>();
            hesAppConnectionMock.SetupGet(x => x.State).Returns(HesConnectionState.Connected);
            hesAppConnectionMock
                .Setup(x => x.AuthHwVault(It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var vaultAuthProcessor = new VaultAuthorizationProcessor(hesAppConnectionMock.Object, uiMock.Object, logMock.Object);

            // Act
            await vaultAuthProcessor.AuthVault(vaultMock.Object, CancellationToken.None);

            // Assert
            Assert.Fail("WorkflowException was expected");
        }
    }
}
