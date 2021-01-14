using Hideez.SDK.Communication.Connection;
using Hideez.SDK.Communication.Interfaces;
using Moq;
using System;

namespace HideezMiddleware.Tests
{
    internal class MockFactory
    {
        internal static Mock<IConnectionController> GetConnectionControllerMock(string id = "")
        {
            if (string.IsNullOrWhiteSpace(id))
                id = Guid.NewGuid().ToString();

            var connectionId = new ConnectionId(id, 0);

            var connectionMock = new Mock<IConnection>();
            connectionMock.Setup(x => x.ConnectionId).Returns(connectionId);

            var controllerMock = new Mock<IConnectionController>();
            controllerMock.Setup(x => x.Connection).Returns(connectionMock.Object);

            return controllerMock;
        }
    }
}
