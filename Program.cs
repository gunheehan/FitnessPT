using FitnessPT.Components;
using FitnessPT.Components.Pages.Exercises;
using FitnessPT.Services;
using FitnessPT.Services.Http;

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
builder.Services.AddScoped<IApiClient, ApiClient>();
builder.Services.AddScoped<IExerciseService, ExerciseService>();

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

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapFallbackToPage("/_Host");

app.Urls.Add("http://0.0.0.0:5100");

app.Run();