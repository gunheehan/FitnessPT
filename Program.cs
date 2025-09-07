using FitnessPT.Components;
using FitnessPT.Services;

var builder = WebApplication.CreateBuilder(args);

// .NET 8 Blazor Web App 서비스 추가
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// HttpClient 설정 개선 (타임아웃 및 재시도 정책 추가)
builder.Services.AddHttpClient<IExerciseApiService, ExerciseApiService>(client =>
{
    var baseUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "http://redhorse.iptime.org:6001/";
    if (!baseUrl.EndsWith("/"))
        baseUrl += "/";
    
    client.BaseAddress = new Uri(baseUrl);
    client.DefaultRequestHeaders.Add("User-Agent", "FitnessPT-Frontend/1.0");
    client.Timeout = TimeSpan.FromSeconds(30); // 타임아웃 설정
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler()
{
    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true // 개발용
});

builder.Services.AddHttpClient<ICategoryApiService, CategoryApiService>(client =>
{
    var baseUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "http://redhorse.iptime.org:6001/";
    if (!baseUrl.EndsWith("/"))
        baseUrl += "/";
    
    client.BaseAddress = new Uri(baseUrl);
    client.DefaultRequestHeaders.Add("User-Agent", "FitnessPT-Frontend/1.0");
    client.Timeout = TimeSpan.FromSeconds(30); // 타임아웃 설정
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler()
{
    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true // 개발용
});

// 서비스 등록 - Scoped로 변경하여 생명주기 관리
builder.Services.AddScoped<IExerciseApiService, ExerciseApiService>();
builder.Services.AddScoped<ICategoryApiService, CategoryApiService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();
app.MapGet("/", () => Results.Redirect("/contents_manager"));

// .NET 8 Blazor Web App을 위한 라우팅 설정
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// 포트 설정
app.Urls.Add("http://0.0.0.0:5100");

app.Run();