using Microsoft.VisualStudio.TestTools.UnitTesting;
using HideezClient.Modules.DeviceManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Messaging;
using HideezClient.Modules.ServiceProxy;
using Moq;
using HideezClient.HideezServiceReference;
using HideezClient.Messages;
using System.Threading;

namespace HideezClient.Modules.DeviceManager.Tests
{
    [TestClass()]
    public class DeviceManagerTests
    {
        private readonly List<DeviceDTO> devices = new List<DeviceDTO>();

        [TestMethod()]
        public async Task EnumerateDevices_FluctuatingServiceConnection_DevicesEnumerated()
        {
            IHideezService hideezService = GetHideezService();
            IServiceProxy serviceProxy = GetServiceProxy(hideezService);
            IMessenger messenger = GetMessenger();
            IDeviceManager deviceManager = GetDeviceManager(messenger, serviceProxy);

            devices.Clear();
            devices.Add(new DeviceDTO
            {
                Id = "CD4D46777E19",
                Name = "0001",
            });
            devices.Add(new DeviceDTO
            {
                Id = "CD4D46777E29",
                Name = "0002",
            });
            devices.Add(new DeviceDTO
            {
                Id = "CD4D46777E39",
                Name = "0003",
            });

            await serviceProxy.ConnectAsync();
            await Task.Delay(500);
            Assert.IsTrue(deviceManager.Devices.Count() == 1);
            await serviceProxy.DisconnectAsync();
            await Task.Delay(500);
            Assert.IsTrue(deviceManager.Devices.Count() == 0);

            await serviceProxy.ConnectAsync();
            await Task.Delay(500);
            Assert.IsTrue(deviceManager.Devices.Count() == 1);
            await serviceProxy.DisconnectAsync();
            await Task.Delay(500);
            Assert.IsTrue(deviceManager.Devices.Count() == 0);
        }

        [TestMethod()]
        public async Task DeviceCollectionChanged_AddDevices_DevicesEnumerated()
        {
            IHideezService hideezService = GetHideezService();
            IServiceProxy serviceProxy = GetServiceProxy(hideezService);
            IMessenger messenger = GetMessenger();
            IDeviceManager deviceManager = GetDeviceManager(messenger, serviceProxy);

            devices.Clear();

            await serviceProxy.ConnectAsync();
            Assert.IsTrue(deviceManager.Devices.Count() == 0);

            await Task.Run(() =>
            {
                devices.Add(new DeviceDTO
                {
                    Id = "DCD777B8882D",
                    Name = "0000",
                    SerialNo = "0"
                });
                devices.Add(new DeviceDTO
                {
                    Id = "000777B8882D",
                    Name = "0001",
                    SerialNo = "1"
                });
                devices.Add(new DeviceDTO
                {
                    Id = "0007HJK8882D",
                    Name = "0002",
                    SerialNo = "2"
                });
                messenger.Send(new DevicesCollectionChangedMessage(devices.ToArray()));
            });

            await Task.Delay(500);
            Assert.IsTrue(deviceManager.Devices.Count() == 3);

            await Task.Run(() =>
            {
                devices.RemoveAt(0);
            });
            messenger.Send(new DevicesCollectionChangedMessage(devices.ToArray()));

            await Task.Delay(500);
            Assert.IsTrue(deviceManager.Devices.Count() == 2);

            await Task.Run(() =>
            {
                devices.Clear();
            });
            messenger.Send(new DevicesCollectionChangedMessage(devices.ToArray()));

            await Task.Delay(500);
            Assert.IsTrue(deviceManager.Devices.Count() == 0);

            await serviceProxy.DisconnectAsync();
            Assert.IsTrue(deviceManager.Devices.Count() == 0);
        }

        [TestMethod()]
        public async Task DeviceCollectionChanged_AddDevicesAsync_DevicesEnumerated()
        {
            IHideezService hideezService = GetHideezService();
            IServiceProxy serviceProxy = GetServiceProxy(hideezService);
            IMessenger messenger = GetMessenger();
            IDeviceManager deviceManager = GetDeviceManager(messenger, serviceProxy);

            devices.Clear();
            devices.Add(new DeviceDTO
            {
                Id = "CD4D46777E1A",
                Name = "0000",
                SerialNo = "0"
            });

            bool res = await serviceProxy.ConnectAsync();

            await Task.Delay(500);
            Assert.IsTrue(deviceManager.Devices.Count() == 1);

            devices.Add(new DeviceDTO
            {
                Id = "DCD777B8D52A",
                Name = "0001",
                SerialNo = "1"
            });
            messenger.Send(new DevicesCollectionChangedMessage(devices.ToArray()));

            await Task.Run(() =>
            {
                devices.Add(new DeviceDTO
                {
                    Id = "DCD777B8883A",
                    Name = "0002",
                    SerialNo = "2"
                });
                devices.Add(new DeviceDTO
                {
                    Id = "000777B8884A",
                    Name = "0003",
                    SerialNo = "3"
                });
                messenger.Send(new DevicesCollectionChangedMessage(devices.ToArray()));
            });

            await Task.Delay(500);
            Assert.IsTrue(deviceManager.Devices.Count() == 4);

            devices.RemoveAt(0);
            messenger.Send(new DevicesCollectionChangedMessage(devices.ToArray()));
            await Task.Delay(500);
            Assert.IsTrue(deviceManager.Devices.Count() == 3);

            await Task.Run(() =>
            {
                devices.RemoveAt(0);
            });
            messenger.Send(new DevicesCollectionChangedMessage(devices.ToArray()));
            await Task.Delay(500);
            Assert.IsTrue(deviceManager.Devices.Count() == 2);

            await serviceProxy.DisconnectAsync();
            await Task.Delay(500);
            Assert.IsTrue(deviceManager.Devices.Count() == 0);
        }

        [TestMethod()]
        public async Task DeviceCollectionChanged_AsyncLoadTest()
        {
            int taskCount = 20;
            int devicesPerTask = 30;

            IHideezService hideezService = GetHideezService();
            IServiceProxy serviceProxy = GetServiceProxy(hideezService);
            IMessenger messenger = GetMessenger();
            IDeviceManager deviceManager = GetDeviceManager(messenger, serviceProxy);

            devices.Clear();
            bool res = await serviceProxy.ConnectAsync();

            object addLock = new object();
            Action AddRandomDevice = () =>
            {
                lock (addLock)
                {
                    var guid = Guid.NewGuid().ToString();
                    devices.Add(new DeviceDTO
                    {
                        Id = guid,
                        Name = guid,
                    });
                    messenger.Send(new DevicesCollectionChangedMessage(devices.ToArray()));
                }
            };

            Task[] creationTasks = new Task[taskCount];

            for (int j = 0; j < creationTasks.Length; j++)
            {
                creationTasks[j] = Task.Run(() =>
                {
                    for (int i = 0; i < devicesPerTask; i++)
                        AddRandomDevice();
                });
            }

            Task.WaitAll(creationTasks);

            await Task.Delay(500); // There is a slight delay between adding device and when its available in collection

            Assert.IsTrue(deviceManager.Devices.Count() == devicesPerTask * taskCount);
        }

        private IHideezService GetHideezService()
        {
            var hideezService = new Mock<IHideezService>();
            hideezService.Setup(s => s.GetDevicesAsync()).ReturnsAsync(() => devices.ToArray());
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
            return new DeviceManager(messanger, serviceProxy, new Mock<IWindowsManager>().Object, new Mock<IRemoteDeviceFactory>().Object);
        }

        private IMessenger GetMessenger()
        {
            return new Messenger();
        }
    }
}