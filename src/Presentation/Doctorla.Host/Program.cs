using Doctorla.Host;
using Doctorla.Host.Configurations;
using ServiceStack;

var builder = WebApplication.CreateBuilder(args);
builder.AddConfigurations();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseServiceStack(new AppHost());

app.Run();
