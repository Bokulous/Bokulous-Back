using Bokulous_Back.Models;
using Bokulous_Back.MongoDb;
using Bokulous_Back.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.Configure<BooksDatabaseSettings>(
    builder.Configuration.GetSection("BokulousDatabase"));

builder.Services.AddSingleton<BooksService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

var db = new DbConnect();
db.Connect();

app.Run();