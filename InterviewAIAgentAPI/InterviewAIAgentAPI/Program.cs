using InterviewAIAgentAPI.Data;
using InterviewAIAgentAPI.Models;
using InterviewAIAgentAPI.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddHttpClient();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddSingleton<GmailServiceHelper>();
builder.Services.Configure<TwilioSettings>(builder.Configuration.GetSection("TwilioSettings"));
builder.Services.Configure<ElevenlabSettings>(builder.Configuration.GetSection("ElevenlabSettings"));
builder.Services.AddSingleton<TwilioService>();

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

app.Run();
