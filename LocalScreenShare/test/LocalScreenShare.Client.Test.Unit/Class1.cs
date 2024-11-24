using Bunit;
using FluentAssertions;
using LocalScreenShare.Client.Constants;
using LocalScreenShare.Client.Pages;
using LocalScreenShare.Client.Proxy;
using Microsoft.JSInterop;
using NSubstitute;
using Xunit;

namespace LocalScreenShare.Client.Test.Unit;

public class Class1 : TestContext
{
	private IRenderedComponent<StreamPage> _cut;
	private StreamPage _streamComponent;


	public Class1()
	{
		JSInterop.SetupModule(JSMethod.Stream.Filename).Setup<IJSStreamReference>(JSMethod.Stream.CaptureScreen);

		_cut = RenderComponent<StreamPage>();
		_streamComponent = _cut.Instance;

		var mockHubConnection = Substitute.For<IHubConnectionProxy>();

		StreamPage.hubConnectionProxy = mockHubConnection;
	}

	[Fact]
	public async Task Test()
	{
		Func<Task> method = StreamPage.Start;
		//await method.Should().NotThrowAsync()

		//await mockHubConnection.Received(1).SendAsync("SendMessage", Arg.Is("TestUser"), Arg.Is("TestMessage"));
	}
}