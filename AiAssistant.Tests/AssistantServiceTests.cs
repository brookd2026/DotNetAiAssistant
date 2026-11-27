using AiAssistant.Core;
using Microsoft.Extensions.AI;
using Moq;
using NUnit.Framework;

namespace AiAssistant.Tests;

[TestFixture]
public class AssistantServiceTests
{
    private Mock<IChatClient> _mockChatClient = null!;
    private AssistantService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _mockChatClient = new Mock<IChatClient>();
        _sut = new AssistantService(_mockChatClient.Object);
    }

    [Test]
    public async Task AskAsync_MultiTurnHistory_SendsSystemAndAllTurns()
    {
        IEnumerable<ChatMessage>? sentMessages = null;
        _mockChatClient
            .Setup(c => c.GetResponseAsync(
                It.IsAny<IEnumerable<ChatMessage>>(),
                It.IsAny<ChatOptions?>(),
                It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<ChatMessage>, ChatOptions?, CancellationToken>((msgs, _, _) => sentMessages = msgs.ToList())
            .ReturnsAsync(new ChatResponse(new ChatMessage(ChatRole.Assistant, "It stands for Single Responsibility.")));

        var history = new List<ConversationTurn>
        {
            new("user", "What is SOLID in C#?"),
            new("assistant", "SOLID is five object-oriented design principles."),
            new("user", "What does the S stand for?")
        };

        var result = await _sut.AskAsync(history);

        Assert.That(result, Is.EqualTo("It stands for Single Responsibility."));
        Assert.That(sentMessages!.Count(), Is.EqualTo(4)); // 1 System + 3 conversation turns
    }

    [Test]
    public void AskAsync_EmptyHistory_ThrowsArgumentException()
    {
        Assert.ThrowsAsync<ArgumentException>(() => _sut.AskAsync([]));
    }
}
