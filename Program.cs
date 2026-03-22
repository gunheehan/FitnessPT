using FitnessPT.Components;
using FitnessPT.Components.Controllers;
using FitnessPT.Services;
using FitnessPT.Services.Http;
using FitnessPT.WebSocket;
using FitnessPT.WebSocket.Chat;
using ISession = FitnessPT.Services.ISession;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
var apiBaseUrl = builder.Configuration["FITNESSPT:ApiSettings:BaseUrl"] 
                 ?? "http://localhost:5117";

builder.Services.AddScoped(sp => new HttpClient 
{ 
    BaseAddress = new Uri(apiBaseUrl)
});
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ExerciseController>();
builder.Services.AddScoped<RoutineController>();
builder.Services.AddScoped<IApiClient, ApiClient>();
builder.Services.AddScoped<IExerciseService, ExerciseService>();
builder.Services.AddScoped<IRoutineService, RoutineService>();
builder.Services.AddScoped<ISession, SessionService>();
builder.Services.AddScoped<AlertService>();

// WebSocket 인프라 등록
builder.Services.AddSingleton<WebSocketConnectionManager>();
builder.Services.AddSingleton<WebSocketMiddleware>();

// 채팅 서비스 등록
builder.Services.AddSingleton<ChatRoomManager>();
builder.Services.AddSingleton<ChatWebSocketHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

// WebSocket 활성화 + 경로 매핑
app.UseWebSockets();
app.UseMiddleware<WebSocketMiddleware>();

var wsMiddleware = app.Services.GetRequiredService<WebSocketMiddleware>();
wsMiddleware.MapHandler("/ws/chat", typeof(ChatWebSocketHandler));

app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        // Service Worker와 Manifest 파일의 캐시 헤더 설정
        if (ctx.File.Name == "sw.js" || ctx.File.Name == "manifest.json")
        {
            ctx.Context.Response.Headers.CacheControl = "no-cache, no-store, must-revalidate";
            ctx.Context.Response.Headers.Pragma = "no-cache";
            ctx.Context.Response.Headers.Expires = "0";
        }
    }
});
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Urls.Add("http://0.0.0.0:5100");

app.Run();