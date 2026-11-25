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
    public async Task AskAsync_ValidQuestion_ReturnsModelText()
    {
        var expectedResponse = new ChatResponse(new ChatMessage(ChatRole.Assistant, "Dependency injection decouples components."));
        _mockChatClient
            .Setup(c => c.GetResponseAsync(
                It.IsAny<IEnumerable<ChatMessage>>(),
                It.IsAny<ChatOptions?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var result = await _sut.AskAsync("What is dependency injection?");

        Assert.That(result, Is.EqualTo("Dependency injection decouples components."));
    }

    [Test]
    public void AskAsync_EmptyQuestion_ThrowsArgumentException()
    {
        Assert.ThrowsAsync<ArgumentException>(() => _sut.AskAsync("   "));
    }
}
