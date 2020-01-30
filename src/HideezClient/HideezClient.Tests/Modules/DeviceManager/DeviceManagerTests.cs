using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Messaging;
using HideezClient.Modules.ServiceProxy;
using Moq;
using HideezClient.HideezServiceReference;
using HideezClient.Messages;
using NUnit.Framework;

namespace HideezClient.Modules.DeviceManager.Tests
{
    [Parallelizable(ParallelScope.All)]
    public class DeviceManagerTests
    {
        [Test]
        public async Task EnumerateDevices_FluctuatingServiceConnection_DevicesEnumerated()
        {
            var devices = new List<DeviceDTO>();
            var hideezServiceMock = new Mock<IHideezService>();
            hideezServiceMock.Setup(s => s.GetDevicesAsync()).ReturnsAsync(() => devices.ToArray());
            IHideezService hideezService = hideezServiceMock.Object;
            IServiceProxy serviceProxy = GetServiceProxy(hideezService);
            IMessenger messenger = GetMessenger();
            IDeviceManager deviceManager = GetDeviceManager(messenger, serviceProxy);

            int devicesCount = 100;

            for (int i = 0; i < devicesCount; i++)
                devices.Add(GetRandomDeviceDTO());

            int serviceReconnectsCount = 10;
            for (int i = 0; i < serviceReconnectsCount; i++)
            {
                await serviceProxy.ConnectAsync();
                await Task.Delay(200);
                Assert.AreEqual(devicesCount, deviceManager.Devices.Count());

                await serviceProxy.DisconnectAsync();
                await Task.Delay(200);
                Assert.AreEqual(0, deviceManager.Devices.Count());
            }
        }

        [Test]
        public async Task DeviceCollectionChanged_AddDevices_DevicesEnumerated()
        {
            var devices = new List<DeviceDTO>();
            var hideezServiceMock = new Mock<IHideezService>();
            hideezServiceMock.Setup(s => s.GetDevicesAsync()).ReturnsAsync(() => devices.ToArray());
            IHideezService hideezService = hideezServiceMock.Object;
            IServiceProxy serviceProxy = GetServiceProxy(hideezService);
            IMessenger messenger = GetMessenger();
            IDeviceManager deviceManager = GetDeviceManager(messenger, serviceProxy);

            await serviceProxy.ConnectAsync();
            await Task.Delay(200);
            Assert.AreEqual(0, deviceManager.Devices.Count());

            await Task.Run(() =>
            {
                devices.Add(GetRandomDeviceDTO());
                devices.Add(GetRandomDeviceDTO());
                devices.Add(GetRandomDeviceDTO());
                messenger.Send(new DevicesCollectionChangedMessage(devices.ToArray()));
            });

            await Task.Delay(200);
            Assert.AreEqual(3, deviceManager.Devices.Count());

            await Task.Run(() =>
            {
                devices.RemoveAt(0);
            });
            messenger.Send(new DevicesCollectionChangedMessage(devices.ToArray()));
            await Task.Delay(200);
            Assert.AreEqual(2, deviceManager.Devices.Count());

            await Task.Run(() =>
            {
                devices.Clear();
            });
            messenger.Send(new DevicesCollectionChangedMessage(devices.ToArray()));
            await Task.Delay(200);
            Assert.AreEqual(0, deviceManager.Devices.Count());

            await serviceProxy.DisconnectAsync();
            await Task.Delay(200);
            Assert.AreEqual(0, deviceManager.Devices.Count());
        }

        [Test]
        public async Task DeviceCollectionChanged_AddDevicesAsync_DevicesEnumerated()
        {
            var devices = new List<DeviceDTO>();
            var hideezServiceMock = new Mock<IHideezService>();
            hideezServiceMock.Setup(s => s.GetDevicesAsync()).ReturnsAsync(() => devices.ToArray());
            IHideezService hideezService = hideezServiceMock.Object;
            IServiceProxy serviceProxy = GetServiceProxy(hideezService);
            IMessenger messenger = GetMessenger();
            IDeviceManager deviceManager = GetDeviceManager(messenger, serviceProxy);

            devices.Add(GetRandomDeviceDTO());

            await serviceProxy.ConnectAsync();
            await Task.Delay(200);
            Assert.AreEqual(1, deviceManager.Devices.Count());

            devices.Add(GetRandomDeviceDTO());
            messenger.Send(new DevicesCollectionChangedMessage(devices.ToArray()));

            await Task.Run(() =>
            {
                devices.Add(GetRandomDeviceDTO());
                devices.Add(GetRandomDeviceDTO());
                messenger.Send(new DevicesCollectionChangedMessage(devices.ToArray()));
            });

            await Task.Delay(200);
            Assert.AreEqual(4, deviceManager.Devices.Count());

            devices.RemoveAt(0);
            messenger.Send(new DevicesCollectionChangedMessage(devices.ToArray()));
            await Task.Delay(200);
            Assert.AreEqual(3, deviceManager.Devices.Count());

            await Task.Run(() =>
            {
                devices.RemoveAt(0);
            });
            messenger.Send(new DevicesCollectionChangedMessage(devices.ToArray()));
            await Task.Delay(200);
            Assert.AreEqual(2, deviceManager.Devices.Count());


            await serviceProxy.DisconnectAsync();
            await Task.Delay(200);
            Assert.AreEqual(0, deviceManager.Devices.Count());
        }

        [Test]
        public async Task DeviceCollectionChanged_AsyncLoadTest()
        {
            int taskCount = 20;
            int devicesPerTask = 50;

            var devices = new List<DeviceDTO>();
            var hideezServiceMock = new Mock<IHideezService>();
            hideezServiceMock.Setup(s => s.GetDevicesAsync()).ReturnsAsync(() => devices.ToArray());
            IHideezService hideezService = hideezServiceMock.Object;
            IServiceProxy serviceProxy = GetServiceProxy(hideezService);
            IMessenger messenger = GetMessenger();
            IDeviceManager deviceManager = GetDeviceManager(messenger, serviceProxy);

            bool res = await serviceProxy.ConnectAsync();

            object addLock = new object();
            Action AddRandomDevice = () =>
            {
                lock (addLock)
                {
                    devices.Add(GetRandomDeviceDTO());
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
            await Task.Delay(400 * taskCount);
            Assert.AreEqual(devicesPerTask * taskCount, deviceManager.Devices.Count());
        }

        [Test]
        public async Task EnumerateDevices_ClearDevices_DevicesCollectionCleared()
        {
            var devices = new List<DeviceDTO>();
            var hideezServiceMock = new Mock<IHideezService>();
            hideezServiceMock.Setup(s => s.GetDevicesAsync()).ReturnsAsync(() => devices.ToArray());
            IHideezService hideezService = hideezServiceMock.Object;
            IServiceProxy serviceProxy = GetServiceProxy(hideezService);
            IMessenger messenger = GetMessenger();
            IDeviceManager deviceManager = GetDeviceManager(messenger, serviceProxy);

            await serviceProxy.ConnectAsync();

            int devicesCount = 1000;
            for (int i = 0; i < devicesCount; i++)
                devices.Add(GetRandomDeviceDTO());

            messenger.Send(new DevicesCollectionChangedMessage(devices.ToArray()));
            await serviceProxy.DisconnectAsync();
            await Task.Delay(2000);
            Assert.AreEqual(0, deviceManager.Devices.Count());
        }

        [Test]
        public async Task EnumerateDevices_QuickReconnect_DevicesCollectionEnumerated()
        {
            var devices = new List<DeviceDTO>();
            var hideezServiceMock = new Mock<IHideezService>();
            hideezServiceMock.Setup(s => s.GetDevicesAsync()).ReturnsAsync(() => devices.ToArray());
            IHideezService hideezService = hideezServiceMock.Object;
            IServiceProxy serviceProxy = GetServiceProxy(hideezService);
            IMessenger messenger = GetMessenger();
            IDeviceManager deviceManager = GetDeviceManager(messenger, serviceProxy);

            int devicesCount = 1000;
            for (int i = 0; i < devicesCount; i++)
                devices.Add(GetRandomDeviceDTO());

            var connectionTask = Task.Factory.StartNew(serviceProxy.ConnectAsync);
            var disconnectionTask = Task.Factory.StartNew(serviceProxy.DisconnectAsync);
            var reconnectionTask = Task.Factory.StartNew(serviceProxy.ConnectAsync);

            await Task.WhenAll(connectionTask, disconnectionTask, reconnectionTask);
            await Task.Delay(2000);
            Assert.AreEqual(devicesCount, deviceManager.Devices.Count());
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

        private DeviceDTO GetRandomDeviceDTO()
        {
            return new DeviceDTO
            {
                Id = Guid.NewGuid().ToString(),
                Name = Guid.NewGuid().ToString(),
                SerialNo = Guid.NewGuid().ToString(),
                IsConnected = true,
            };
        }

    }
}