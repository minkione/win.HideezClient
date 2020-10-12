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
    public class StateUpdateProcessorTests
    {
        readonly AccessLevel NEED_UPDATE_ACCESSLEVEL = new AccessLevel(true, false, false, false, false, false);
        readonly AccessLevel ALL_OK_ACCESSLEVEL = new AccessLevel(false, false, false, false, false, false);

        readonly HwVaultInfoFromHesDto NEED_UPDATE_INFO = new HwVaultInfoFromHesDto() { NeedStateUpdate = true };
        readonly HwVaultInfoFromHesDto ALL_OK_INFO = new HwVaultInfoFromHesDto();

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

            // When IHesAppConnectio.UpdateHwVaultStatus is called, we update Vault mock so that
            // when IVault.RefreshDeviceInfo() is called, the AccessLevel of vault will be updated to reflect 
            // successfull execution of IHesAppConnectio.UpdateHwVaultStatus
            hesAppConnectionMock
                .Setup(x => x.UpdateHwVaultStatus(It.IsAny<HwVaultInfoFromClientDto>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(ALL_OK_INFO))
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
        public async Task UpdateVaultStatus_LinkRequired_StatusUpdated()
        {
            // Arrange
            var logMock = new Mock<ILog>();

            var vaultMock = GetVaultMock(NEED_UPDATE_ACCESSLEVEL);

            var hesAppConnectionMock = GetHesAppConnectionMock(HesConnectionState.Connected, vaultMock);

            var stateUpdateProcessors = new StateUpdateProcessor(hesAppConnectionMock.Object, logMock.Object);

            // Act
            var newVaultInfo = await stateUpdateProcessors.UpdateVaultStatus(vaultMock.Object, NEED_UPDATE_INFO, CancellationToken.None);

            // Assert
            Assert.AreNotEqual(NEED_UPDATE_INFO, newVaultInfo);
        }

        [TestMethod]
        [ExpectedException(typeof(WorkflowException))]
        public async Task UpdateVaultStatus_LinkRequired_NoNetwork_ExceptionThrown()
        {
            // Arrange
            var logMock = new Mock<ILog>();

            var vaultMock = GetVaultMock(NEED_UPDATE_ACCESSLEVEL);

            var hesAppConnectionMock = GetHesAppConnectionMock(HesConnectionState.Disconnected, vaultMock);

            var stateUpdateProcessors = new StateUpdateProcessor(hesAppConnectionMock.Object, logMock.Object);

            // Act
            await stateUpdateProcessors.UpdateVaultStatus(vaultMock.Object, NEED_UPDATE_INFO, CancellationToken.None);

            // Assert
            Assert.Fail("WorkflowException was expected");
        }

        [TestMethod]
        [DataRow(HesConnectionState.Connected)]
        [DataRow(HesConnectionState.Disconnected)]
        public async Task UpdateVaultStatus_LinkNotRequired_StatusUnchanged(HesConnectionState hesConnectionState)
        {
            // Arrange
            var logMock = new Mock<ILog>();

            var vaultMock = GetVaultMock(ALL_OK_ACCESSLEVEL);

            var hesAppConnectionMock = GetHesAppConnectionMock(hesConnectionState, vaultMock);
            hesAppConnectionMock
                .Setup(x => x.UpdateHwVaultStatus(It.IsAny<HwVaultInfoFromClientDto>(), It.IsAny<CancellationToken>()))
                .Throws(new InvalidOperationException("Unexpected request to server"));

            var stateUpdateProcessors = new StateUpdateProcessor(hesAppConnectionMock.Object, logMock.Object);

            // Act
            var newVaultInfo = await stateUpdateProcessors.UpdateVaultStatus(vaultMock.Object, ALL_OK_INFO, CancellationToken.None);

            // Assert
            Assert.AreEqual(ALL_OK_INFO, newVaultInfo);
        }

        [TestMethod]
        [ExpectedException(typeof(WorkflowException))]
        public async Task UpdateVaultStatus_LinkRequired_HesAlgorithmError_ExceptionThrown()
        {
            // Arrange
            var logMock = new Mock<ILog>();

            var vaultMock = GetVaultMock(NEED_UPDATE_ACCESSLEVEL);

            var hesAppConnectionMock = new Mock<IHesAppConnection>();
            hesAppConnectionMock.SetupGet(x => x.State).Returns(HesConnectionState.Connected);
            hesAppConnectionMock
                .Setup(x => x.UpdateHwVaultStatus(It.IsAny<HwVaultInfoFromClientDto>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(ALL_OK_INFO));

            var stateUpdateProcessors = new StateUpdateProcessor(hesAppConnectionMock.Object, logMock.Object);

            // Act
            await stateUpdateProcessors.UpdateVaultStatus(vaultMock.Object, NEED_UPDATE_INFO, CancellationToken.None);

            // Assert
            Assert.Fail("WorkflowException was expected");
        }
    }
}
