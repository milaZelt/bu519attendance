using attendance.Components;
using attendance.Services;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.Extensions.DependencyInjection.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.TryAddEnumerable(
    ServiceDescriptor.Scoped<CircuitHandler, BlazorNonceService>(sp => sp.GetRequiredService<BlazorNonceService>()));

builder.Services.AddScoped<BlazorNonceService>();
builder.Services.AddHttpContextAccessor(); // Register IHttpContextAccessor
builder.Services.AddScoped<HttpContextAccessorService>(); // Register HttpContextAccessorService
builder.Services.AddSingleton<AttendanceNotifier>(); // Pushes live attendance changes to every open page

// Add Application Insights telemetry services
builder.Services.AddApplicationInsightsTelemetry();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();