using AiAssistant.Api.Controllers;
using AiAssistant.Core;
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
    var model = builder.Configuration["AI:GeminiModel"] ?? "gemini-3.8-flash";

    var openAiOptions = new OpenAIClientOptions
    {
        Endpoint = new Uri("https://generativelanguage.googleapis.com/v1beta/openai/"),
        NetworkTimeout = TimeSpan.FromMinutes(2)
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

app.MapPost("/chat", async (AskController.AskRequest request, IAssistantService assistantService, CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(request.Question))
        return Results.BadRequest("Question is required.");

    var answer = await assistantService.AskAsync(request.Question, ct);
    return Results.Ok(new AskController.AskResponse(answer));
});

app.MapControllers();

app.Run();
