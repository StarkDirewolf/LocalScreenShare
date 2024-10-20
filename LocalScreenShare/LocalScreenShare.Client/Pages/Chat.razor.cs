using LocalScreenShare.Client.Constants;
using LocalScreenShare.Client.Proxy;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Security;
using System.Reflection;
using System.Runtime.Intrinsics.Arm;

namespace LocalScreenShare.Client.Pages
{
    public partial class Chat
    {
        internal static IJSObjectReference module = null!;

        internal static IHubConnectionProxy? hubConnectionProxy;
        internal string? userInput;
        internal string? messageInput;


        [Inject]
        internal IJSRuntime JSRuntime { get; set; } = null!;

        protected override async Task OnInitializedAsync()
        {
            
            hubConnectionProxy = new HubConnectionProxy
            {
                HubConnection = new HubConnectionBuilder()
                .WithUrl(Navigation.ToAbsoluteUri("/chathub"))
                .Build()
            };

            hubConnectionProxy.On<string>("ReceiveAllSdpJsons", async sdps =>
            {
                if (!string.IsNullOrEmpty(sdps))
                    await module.InvokeVoidAsync("connectRemoteSdp", sdps);
            });

            hubConnectionProxy.On<string>("ReceiveSdpAnswerJson", async sdp =>
            {
                if (!string.IsNullOrEmpty(sdp))
                    await module.InvokeVoidAsync("receiveAnswer", sdp);
            });

            await hubConnectionProxy.StartAsync();
        }

        protected async override Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                /*
                    Change the {PATH} placeholder in the next line to the path of
                    the collocated JS file in the app. Examples:

                    ./Components/Pages/JsCollocation2.razor.js (.NET 8 or later)
                    ./Pages/JsCollocation2.razor.js (.NET 7 or earlier)
                */
                module = await JSRuntime.InvokeAsync<IJSObjectReference>("import",
                "./Pages/Chat.razor.js");

                hubConnectionProxy?.SendAsync("GetSdps");
            }
        }

        internal async Task Send()
        {
            var sdpJson = await module.InvokeAsync<string>(JSMethod.Chat.CaptureScreen);
            await hubConnectionProxy?.SendAsync("StoreSdpJson", sdpJson)!;
        }

        [JSInvokable]
        public static Task ReceiveLocalSdpAnswerAsync(string sdpAnswerJson)
        {
            return hubConnectionProxy?.SendAsync("ReturnAnswer", sdpAnswerJson)!;
        }

        //[JSInvokable]
        //public static Task ReceiveLocalSdp(string sdpJson)
        //{
        //    return hubConnectionProxy?.SendAsync("StoreSdpJson", sdpJson)!;
        //}

        public bool IsConnected =>
            hubConnectionProxy?.State == HubConnectionState.Connected;

        public async ValueTask DisposeAsync()
        {
            if (hubConnectionProxy is not null)
            {
                await hubConnectionProxy.DisposeAsync();
            }

            if (module is not null)
            {
                await module.DisposeAsync();
            }
        }
    }
}