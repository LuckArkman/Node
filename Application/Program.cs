using Core;
using Interfaces;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// 1. Serviços
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// CORREÇÃO 1: Alterado de Scoped para Singleton
// Isso garante que o treinamento do bot seja mantido na memória entre as requisições.
builder.Services.AddSingleton<IChatBot>(sp =>
{
    const int inputSize = 100;
    const int hiddenSize = 200;
    const int outputSize = 12;
    return new ChatBot(inputSize, hiddenSize, outputSize);
});

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        // CORREÇÃO 2: Metadados atualizados para refletir o projeto real
        Title = "ChatBot API", 
        Version = "v1", 
        Description = "API de ChatBot com Rede Neural para responder perguntas gerais."
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.SetIsOriginAllowed(origin => true)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

// 2. Pipeline de Requisição

// CORREÇÃO 3: Adicionado o Middleware do Swagger
// Sem isso, a página /swagger retorna 404.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.MapControllers();

app.Run();