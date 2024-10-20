using LocalScreenShare.Client.Pages;
using LocalScreenShare.Components;
using Microsoft.AspNetCore.ResponseCompression;
using LocalScreenShare.Hubs;
using LocalScreenShare.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();
builder.Services.AddSingleton<ISdpStore>(new SdpStore());

builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        ["application/octet-stream"]);
});

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

builder.WebHost.UseUrls("http://*:5045");

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseResponseCompression();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

//app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(LocalScreenShare.Client._Imports).Assembly);

app.MapHub<ChatHub>("/chathub");

app.Run();
