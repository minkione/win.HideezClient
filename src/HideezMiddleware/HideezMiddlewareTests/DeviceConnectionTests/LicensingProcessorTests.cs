using Hideez.SDK.Communication.Device;
using Hideez.SDK.Communication.HES.Client;
using Hideez.SDK.Communication.HES.DTO;
using Hideez.SDK.Communication.Interfaces;
using Hideez.SDK.Communication.Log;
using HideezMiddleware.DeviceConnection.Workflow;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HideezMiddleware.Tests.DeviceConnectionTests
{
    [TestClass]
    public class LicensingProcessorTests
    {
        [TestMethod]
        public async Task CheckLicense_ServerReturnLicenseForUpdate_OnHwVaultLicenseAppliedCalled()
        {
            // Arrange
            string serialNo = Guid.NewGuid().ToString();
            var deviceMock = new Mock<IDevice>();
            deviceMock.SetupGet(d => d.SerialNo).Returns(serialNo);
            deviceMock.SetupGet(d => d.Mac).Returns("32:B4:D2:R3");
            deviceMock.SetupGet(d => d.AccessLevel).Returns(new AccessLevel(true, false, false, false, false, false));
            deviceMock.SetupGet(d => d.LicenseInfo).Returns(1);

            IList<HwVaultLicenseDto> licensesList = new List<HwVaultLicenseDto>() 
            { 
                new HwVaultLicenseDto() { Data = new byte[] { }, Id = "LicenseId_1" }
            };
            Task<IList<HwVaultLicenseDto>> resultTask = Task.FromResult(licensesList);
            var connectionMock = new Mock<IHesAppConnection>();
            connectionMock.SetupGet(x => x.State).Returns(HesConnectionState.Connected);
            connectionMock.Setup(x => x.GetHwVaultLicenses(serialNo, CancellationToken.None)).Returns(resultTask);

            var uiManager = new Mock<IClientUiManager>();
            var logMock = new Mock<ILog>();
            LicensingProcessor licensing = new LicensingProcessor(connectionMock.Object, uiManager.Object, logMock.Object);
            string licenseId = licensesList.FirstOrDefault().Id;

            // Act
            await licensing.CheckLicense(deviceMock.Object, new HwVaultInfoFromHesDto(), CancellationToken.None);

            // Assert
            connectionMock.Verify(x => x.OnHwVaultLicenseApplied(serialNo, licenseId));
        }

        [TestMethod]
        [ExpectedException(typeof(WorkflowException))]
        public async Task CheckLicense_NoLicensesAvailable_WorkflowException()
        {
            // Arrange
            var deviceMock = new Mock<IDevice>();
            deviceMock.SetupGet(d => d.AccessLevel).Returns(new AccessLevel(false, false, false, false, false, false));

            var connectionMock = new Mock<IHesAppConnection>();
            connectionMock.SetupGet(x => x.State).Returns(HesConnectionState.Connected);

            var uiManager = new Mock<IClientUiManager>();
            var logMock = new Mock<ILog>();
            LicensingProcessor licensing = new LicensingProcessor(connectionMock.Object, uiManager.Object, logMock.Object);

            // Act
            await licensing.CheckLicense(deviceMock.Object, new HwVaultInfoFromHesDto(), CancellationToken.None);

            // Assert phase is empty: expecting exception, nothing to assert
        }

        [TestMethod]
        [ExpectedException(typeof(WorkflowException))]
        public async Task CheckLicense_CannotDownloadLicense_WorkflowException()
        {
            // Arrange
            var deviceMock = new Mock<IDevice>();
            deviceMock.SetupGet(d => d.AccessLevel).Returns(new AccessLevel(true, false, false, false, false, false));

            var connectionMock = new Mock<IHesAppConnection>();
            connectionMock.SetupGet(x => x.State).Returns(HesConnectionState.Disconnected);

            var uiManager = new Mock<IClientUiManager>();
            var logMock = new Mock<ILog>();
            LicensingProcessor licensing = new LicensingProcessor(connectionMock.Object, uiManager.Object, logMock.Object);

            // Act
            await licensing.CheckLicense(deviceMock.Object, new HwVaultInfoFromHesDto(), CancellationToken.None);

            // Assert phase is empty: expecting exception, nothing to assert
        }

        [TestMethod]
        [ExpectedException(typeof(WorkflowException))]
        public async Task CheckLicense_EmptyLicenseData_WorkflowException()
        {
            // Arrange
            string serialNo = Guid.NewGuid().ToString();
            var deviceMock = new Mock<IDevice>();
            deviceMock.SetupGet(d => d.SerialNo).Returns(serialNo);
            deviceMock.SetupGet(d => d.Mac).Returns("32:B4:D2:R3");
            deviceMock.SetupGet(d => d.AccessLevel).Returns(new AccessLevel(true, false, false, false, false, false));

            var list = new List<HwVaultLicenseDto>() { new HwVaultLicenseDto() { Data = null, Id = "LicenseId_1" } } as IList<HwVaultLicenseDto>;
            Task<IList<HwVaultLicenseDto>> resultTask = Task.FromResult(list);
            var connectionMock = new Mock<IHesAppConnection>();
            connectionMock.SetupGet(x => x.State).Returns(HesConnectionState.Connected);
            connectionMock.Setup(x => x.GetHwVaultLicenses(serialNo, CancellationToken.None)).Returns(resultTask);

            var uiManager = new Mock<IClientUiManager>();
            var logMock = new Mock<ILog>();
            LicensingProcessor licensing = new LicensingProcessor(connectionMock.Object, uiManager.Object, logMock.Object);

            // Act
            await licensing.CheckLicense(deviceMock.Object, new HwVaultInfoFromHesDto(), CancellationToken.None);

            // Assert phase is empty: expecting exception, nothing to assert
        }

        [TestMethod]
        public async Task CheckLicense_NeedUpdateLicense_Updated3NewLicenses()
        {
            // Arrange
            string serialNo = Guid.NewGuid().ToString();
            var deviceMock = new Mock<IDevice>();
            deviceMock.SetupGet(d => d.SerialNo).Returns(serialNo);
            deviceMock.SetupGet(d => d.Mac).Returns("32:B4:D2:R3");
            deviceMock.SetupGet(d => d.AccessLevel).Returns(new AccessLevel(false, false, false, false, false, false));
            deviceMock.SetupGet(d => d.LicenseInfo).Returns(1);

            IList<HwVaultLicenseDto> licensesList = new List<HwVaultLicenseDto>() 
            { 
                new HwVaultLicenseDto() { Data = new byte[] { }, Id = "LicenseId_1" },
                new HwVaultLicenseDto() { Data = new byte[] { }, Id = "LicenseId_2" },
                new HwVaultLicenseDto() { Data = new byte[] { }, Id = "LicenseId_3" }
            };
            Task<IList<HwVaultLicenseDto>> resultTask = Task.FromResult(licensesList);
            var connectionMock = new Mock<IHesAppConnection>();
            connectionMock.SetupGet(x => x.State).Returns(HesConnectionState.Connected);
            connectionMock.Setup(x => x.GetNewHwVaultLicenses(serialNo, CancellationToken.None)).Returns(resultTask);

            var uiManager = new Mock<IClientUiManager>();
            var logMock = new Mock<ILog>();
            LicensingProcessor licensing = new LicensingProcessor(connectionMock.Object, uiManager.Object, logMock.Object);

            // Act
            await licensing.CheckLicense(deviceMock.Object, new HwVaultInfoFromHesDto() { NeedUpdateLicense = true }, CancellationToken.None);

            // Assert
            foreach(var license in licensesList)
            {
                connectionMock.Verify(x => x.OnHwVaultLicenseApplied(serialNo, license.Id), Times.Once);
            }
        }
    }
}
