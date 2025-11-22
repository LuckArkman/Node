using Dtos;
using Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Controllers;

[Route("api/[controller]")]
[ApiController]
public class ChatController : ControllerBase
{
    private readonly IChatBot _chatBot;
    public ChatController(IChatBot chatBot)
    {
        _chatBot = chatBot;
    }

    [HttpGet("Trainer")]
    public async Task<IActionResult?> OnTrainer()
    {
        var on = await _chatBot.Train();
        return Ok(on);
    }
    
    [HttpGet("Trainer")]
    public async Task<IActionResult?> OnResponse([FromBody] Input input)
    {
        var on = await _chatBot.Respond(input.text);
        return Ok(on);
    }
}