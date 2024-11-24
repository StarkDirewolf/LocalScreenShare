namespace LocalScreenShare.Client.Test.Unit;

public class Class1 : TestContext
{
    private IRenderedComponent<Chat> _cut;
    private Chat _chatComponent;

    public Class1()
    {
        JSInterop.SetupModule("./Pages/Chat.razor.js").Setup<IJSStreamReference>(JSMethod.Chat.CaptureScreen);

        _cut = RenderComponent<Chat>();
        _chatComponent = _cut.Instance;

        var mockHubConnection = Substitute.For<IHubConnectionProxy>();

        Chat.hubConnectionProxy = mockHubConnection;
    }

    [Fact]
    public async Task Test()
    {
        var method = _chatComponent.Send;
        await method.Should().NotThrowAsync();

        //await mockHubConnection.Received(1).SendAsync("SendMessage", Arg.Is("TestUser"), Arg.Is("TestMessage"));
    }
}