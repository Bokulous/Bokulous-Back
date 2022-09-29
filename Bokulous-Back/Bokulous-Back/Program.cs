using Bokulous_Back.Models;
using Bokulous_Back.Services;

var AllowAnyOrigin = "_allowAnyOrigin";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: AllowAnyOrigin, policy => { policy.AllowAnyOrigin(); });
});

// Add services to the container.
builder.Services
    .Configure<BokulousDatabaseSettings>(builder.Configuration.GetSection("BokulousDatabase"))
    .Configure<BokulousMailSettings>(builder.Configuration.GetSection("BokulousMailSettings"));

builder.Services
    .AddSingleton<IBokulousDbService, BokulousDbService>()
    .AddSingleton<IBokulousMailService, BokulousMailService>();

builder.Services.AddMvc();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(AllowAnyOrigin);

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();