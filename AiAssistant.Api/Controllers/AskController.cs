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
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new
            {
                error = ex.Message,
                innerError = ex.InnerException?.Message
            });
        }
    }
}
