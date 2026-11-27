using Microsoft.Extensions.AI;

namespace AiAssistant.Core;

public record ConversationTurn(string Role, string Content);

public interface IAssistantService
{
    Task<string> AskAsync(IReadOnlyList<ConversationTurn> history, CancellationToken cancellationToken = default);
}

public class AssistantService(IChatClient chatClient) : IAssistantService
{
    public async Task<string> AskAsync(IReadOnlyList<ConversationTurn> history, CancellationToken cancellationToken = default)
    {
        if (history is null || history.Count == 0 || string.IsNullOrWhiteSpace(history[^1].Content))
        {
            throw new ArgumentException("Conversation history cannot be empty.", nameof(history));
        }

        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, "You are a concise, helpful software engineering assistant.")
        };

        foreach (var turn in history)
        {
            var role = turn.Role.Equals("assistant", StringComparison.OrdinalIgnoreCase)
                ? ChatRole.Assistant
                : ChatRole.User;

            messages.Add(new ChatMessage(role, turn.Content));
        }

        var response = await chatClient.GetResponseAsync(messages, cancellationToken: cancellationToken);
        return response.Text ?? string.Empty;
    }
}
