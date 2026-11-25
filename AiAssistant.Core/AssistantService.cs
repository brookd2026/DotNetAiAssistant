using Microsoft.Extensions.AI;

namespace AiAssistant.Core;

public interface IAssistantService
{
    Task<string> AskAsync(string question, CancellationToken cancellationToken = default);
}

public class AssistantService(IChatClient chatClient) : IAssistantService
{
    public async Task<string> AskAsync(string question, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(question))
        {
            throw new ArgumentException("Question cannot be empty.", nameof(question));
        }

        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, "You are a concise, helpful software engineering assistant."),
            new(ChatRole.User, question)
        };

        var response = await chatClient.GetResponseAsync(messages, cancellationToken: cancellationToken);
        return response.Text ?? string.Empty;
    }
}
