using LocalScreenShare.Hubs;
using NSubstitute;
using Xunit;
using Microsoft.AspNetCore.SignalR;

namespace LocalScreenShare.Test.Unit;

public class Class1
{
    [Fact]
    public async Task test()
    {
        var mockClients = Substitute.For<IHubCallerClients>();
        var mockClient = Substitute.For<ISingleClientProxy>();
        mockClients.All.Returns(mockClient);

        var hub = new ChatHub
        {
            Clients = mockClients
        };

        await hub.SendMessage("TestUser", "TestMessage");
        await mockClient.Received(1).SendCoreAsync("ReceiveMessage", Arg.Is<object[]>(obj => obj.Contains("TestMessage")));
    }
}
