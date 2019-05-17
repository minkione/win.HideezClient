using Microsoft.VisualStudio.TestTools.UnitTesting;
using HideezSafe.Modules.DeviceManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Messaging;
using HideezSafe.Modules.ServiceProxy;
using Moq;
using HideezSafe.HideezServiceReference;
using HideezSafe.Messages;
using System.Threading;

namespace HideezSafe.Modules.DeviceManager.Tests
{
    [TestClass()]
    public class DeviceManagerTests
    {
        private readonly List<DeviceDTO> devices = new List<DeviceDTO>();

        [TestMethod()]
        public async Task OpenClose()
        {
            IHideezService hideezService = GetHideezService();
            IServiceProxy serviceProxy = GetServiceProxy(hideezService);
            IMessenger messenger = GetMessenger();
            IDeviceManager deviceManager = GetDeviceManager(messenger, serviceProxy);

            devices.Clear();
            devices.Add(new DeviceDTO
            {
                Id = "CD4D46777E19",
                Name = "8989",
            });

            await serviceProxy.ConnectAsync();
            await Task.Delay(500);
            Assert.IsTrue(deviceManager.Devices.Count == 1);
            await serviceProxy.DisconnectAsync();
            await Task.Delay(500);
            Assert.IsTrue(deviceManager.Devices.Count == 1);

            await serviceProxy.ConnectAsync();
            await Task.Delay(500);
            Assert.IsTrue(deviceManager.Devices.Count == 1);
            await serviceProxy.DisconnectAsync();
            await Task.Delay(500);
            Assert.IsTrue(deviceManager.Devices.Count == 1);
        }

        [TestMethod()]
        public async Task AddDevices()
        {
            IHideezService hideezService = GetHideezService();
            IServiceProxy serviceProxy = GetServiceProxy(hideezService);
            IMessenger messenger = GetMessenger();
            IDeviceManager deviceManager = GetDeviceManager(messenger, serviceProxy);

            devices.Clear();

            await serviceProxy.ConnectAsync();
            Assert.IsTrue(deviceManager.Devices.Count == 0);

            await Task.Run(() =>
            {
                devices.Add(new DeviceDTO
                {
                    Id = "DCD777B8882D",
                    Name = "8877",
                });
                devices.Add(new DeviceDTO
                {
                    Id = "000777B8882D",
                    Name = "8447",
                });
                devices.Add(new DeviceDTO
                {
                    Id = "0007HJK8882D",
                    Name = "8421",
                });
                messenger.Send(new PairedDevicesCollectionChangedMessage(devices.ToArray()));
            });

            await Task.Delay(500);
            Assert.IsTrue(deviceManager.Devices.Count == 3);

            await Task.Run(() =>
            {
                devices.RemoveAt(0);
            });
            messenger.Send(new PairedDevicesCollectionChangedMessage(devices.ToArray()));

            await Task.Delay(500);
            Assert.IsTrue(deviceManager.Devices.Count == 2);

            await Task.Run(() =>
            {
                devices.Clear();
            });
            messenger.Send(new PairedDevicesCollectionChangedMessage(devices.ToArray()));

            await Task.Delay(500);
            Assert.IsTrue(deviceManager.Devices.Count == 0);

            await serviceProxy.DisconnectAsync();
            Assert.IsTrue(deviceManager.Devices.Count == 0);
        }

        [TestMethod()]
        public async Task DeviceManagerTest()
        {
            IHideezService hideezService = GetHideezService();
            IServiceProxy serviceProxy = GetServiceProxy(hideezService);
            IMessenger messenger = GetMessenger();
            IDeviceManager deviceManager = GetDeviceManager(messenger, serviceProxy);

            devices.Clear();
            devices.Add(new DeviceDTO
            {
                Id = "CD4D46777E19",
                Name = "8989",
            });

            bool res = await serviceProxy.ConnectAsync();

            await Task.Delay(500);
            Assert.IsTrue(deviceManager.Devices.Count == 1);

            devices.Add(new DeviceDTO
            {
                Id = "DCD777B8D52D",
                Name = "7777",
            });
            messenger.Send(new PairedDevicesCollectionChangedMessage(devices.ToArray()));

            await Task.Run(() =>
            {
                devices.Add(new DeviceDTO
                {
                    Id = "DCD777B8882D",
                    Name = "8877",
                });
                devices.Add(new DeviceDTO
                {
                    Id = "000777B8882D",
                    Name = "8447",
                });
                messenger.Send(new PairedDevicesCollectionChangedMessage(devices.ToArray()));
            });

            await Task.Delay(500);
            Assert.IsTrue(deviceManager.Devices.Count == 4);

            devices.RemoveAt(0);
            messenger.Send(new PairedDevicesCollectionChangedMessage(devices.ToArray()));
            await Task.Delay(500);
            Assert.IsTrue(deviceManager.Devices.Count == 3);

            await Task.Run(() =>
            {
                devices.RemoveAt(0);
            });
            messenger.Send(new PairedDevicesCollectionChangedMessage(devices.ToArray()));
            await Task.Delay(500);
            Assert.IsTrue(deviceManager.Devices.Count == 2);

            await serviceProxy.DisconnectAsync();
            await Task.Delay(500);
            Assert.IsTrue(deviceManager.Devices.Count == 2);
        }

        private IHideezService GetHideezService()
        {
            var hideezService = new Mock<IHideezService>();
            hideezService.Setup(s => s.GetPairedDevicesAsync()).ReturnsAsync(() => devices.ToArray());
            return hideezService.Object;
        }

        private IServiceProxy GetServiceProxy(IHideezService hideezService)
        {
            bool isConnected = false;
            var serviceProxy = new Mock<IServiceProxy>();
            serviceProxy.Setup(a => a.GetService()).Returns(hideezService);
            serviceProxy.SetupGet(a => a.IsConnected).Returns(() => isConnected);
            serviceProxy.Setup(i => i.ConnectAsync())
                        .Callback(() => isConnected = true)
                        .Returns(Task.FromResult(true))
                        .Raises(i => i.Connected += null, this, EventArgs.Empty);
            serviceProxy.Setup(i => i.DisconnectAsync())
                        .Callback(() => isConnected = false)
                        .Returns(Task.CompletedTask)
                        .Raises(i => i.Disconnected += null, this, EventArgs.Empty);
            return serviceProxy.Object;
        }

        private IDeviceManager GetDeviceManager(IMessenger messanger, IServiceProxy serviceProxy)
        {
            return new DeviceManager(messanger, serviceProxy, new Mock<IWindowsManager>().Object);
        }

        private IMessenger GetMessenger()
        {
            return new Messenger();
        }
    }
}