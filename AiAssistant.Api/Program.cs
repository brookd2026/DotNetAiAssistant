using AiAssistant.Api.Controllers;
using AiAssistant.Api.Middleware;
using AiAssistant.Core;
using Google.GenAI;
using Microsoft.Extensions.AI;
using OllamaSharp;
using OpenAI;
using System.ClientModel;

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
    var model = builder.Configuration["AI:GeminiModel"] ?? "gemini-3.1-flash-lite";

    IChatClient geminiClient = new Client(apiKey: apiKey).AsIChatClient(model);
    builder.Services.AddChatClient(geminiClient);
}

builder.Services.AddScoped<IAssistantService, AssistantService>();

var app = builder.Build();

app.MapOpenApi();
app.UseMiddleware<ApiKeyMiddleware>();
app.UseAuthorization();

app.MapPost("/chat", async (AskController.AskRequest request, IAssistantService assistantService, CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(request.Question))
        return Results.BadRequest("Question is required.");

    var answer = await assistantService.AskAsync(request.Question, ct);
    return Results.Ok(new AskController.AskResponse(answer));
});

app.MapControllers();

app.Run();
