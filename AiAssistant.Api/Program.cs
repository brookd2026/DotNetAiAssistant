using System.ClientModel;
using AiAssistant.Core;
using Microsoft.Extensions.AI;
using OllamaSharp;
using OpenAI;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

if (builder.Environment.IsDevelopment())
{
    var endpoint = new Uri(builder.Configuration["AI:OllamaEndpoint"] ?? "http://localhost:11434");
    var model = builder.Configuration["AI:OllamaModel"] ?? "llama3.2:3b";
    builder.Services.AddChatClient(new OllamaApiClient(endpoint, model));
}
else
{
    var apiKey = builder.Configuration["AI:GeminiApiKey"]
        ?? throw new InvalidOperationException("AI:GeminiApiKey is not configured.");
    var model = builder.Configuration["AI:GeminiModel"] ?? "gemini-2.5-flash";

    var openAiOptions = new OpenAIClientOptions
    {
        Endpoint = new Uri("https://generativelanguage.googleapis.com/v1beta/openai/")
    };

    IChatClient geminiClient = new OpenAIClient(new ApiKeyCredential(apiKey), openAiOptions)
        .GetChatClient(model)
        .AsIChatClient();

    builder.Services.AddChatClient(geminiClient);
}

builder.Services.AddScoped<IAssistantService, AssistantService>();

var app = builder.Build();

app.MapOpenApi();
app.UseAuthorization();
app.MapControllers();

app.Run();
