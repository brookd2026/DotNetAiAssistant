using AiAssistant.Core;
using Microsoft.AspNetCore.Mvc;

namespace AiAssistant.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class AskController(IAssistantService assistantService) : ControllerBase
{
    public record AskRequest(string Question);
    public record AskResponse(string Answer);

    [HttpPost]
    public async Task<ActionResult<AskResponse>> Post([FromBody] AskRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Question))
        {
            return BadRequest("Question is required.");
        }

        try
        {
            var answer = await assistantService.AskAsync(request.Question, cancellationToken);
            return Ok(new AskResponse(answer));
        }
        catch (System.ClientModel.ClientResultException clientEx)
        {
            var rawBody = clientEx.GetRawResponse()?.Content?.ToString();
            return StatusCode(StatusCodes.Status502BadGateway, new
            {
                status = clientEx.Status,
                error = clientEx.Message,
                googleResponse = rawBody
            });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new
            {
                error = ex.ToString(),
            });
        }
    }

    [HttpGet("ping")]
    public IActionResult Ping([FromServices] IConfiguration config)
    {
        var hasKey = !string.IsNullOrWhiteSpace(config["AI:GeminiApiKey"]);
        return Ok(new { status = "AskController is live", geminiKeyConfigured = hasKey });
    }
}
