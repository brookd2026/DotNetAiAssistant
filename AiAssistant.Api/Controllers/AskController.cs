using AiAssistant.Core;
using Microsoft.AspNetCore.Mvc;

namespace AiAssistant.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class AskController(IAssistantService assistantService) : ControllerBase
{
    public record AskRequest(List<ConversationTurn>? Messages, string? Question = null);
    public record AskResponse(string Answer);

    [HttpPost]
    public async Task<ActionResult<AskResponse>> Post([FromBody] AskRequest request, CancellationToken cancellationToken)
    {
        var turns = request.Messages is { Count: > 0 }
            ? request.Messages
            : !string.IsNullOrWhiteSpace(request.Question)
                ? [new ConversationTurn("user", request.Question)]
                : [];

        if (turns.Count == 0)
        {
            return BadRequest("Messages or Question is required.");
        }

        try
        {
            var answer = await assistantService.AskAsync(turns, cancellationToken);
            return Ok(new AskResponse(answer));
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { error = ex.Message });
        }
    }
}
