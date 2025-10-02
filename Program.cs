using FitnessPT.Components;

var builder = WebApplication.CreateBuilder(args);

// .NET 8 Blazor Web App 서비스 추가
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

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

// .NET 8 Blazor Web App을 위한 라우팅 설정
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// 포트 설정
app.Urls.Add("http://0.0.0.0:5100");

app.Run();