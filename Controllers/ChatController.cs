using Dtos; // Certifique-se que este namespace existe e contem a classe Input
using Interfaces;
using Microsoft.AspNetCore.Mvc;

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

    // Rota para treinar o modelo
    [HttpGet("Train")] // Renomeado para evitar conflito
    public async Task<IActionResult> OnTrainer()
    {
        var result = await _chatBot.Train();
        return Ok(result);
    }
    
    // Rota para conversar
    // Alterado para HttpPost porque recebe um objeto [FromBody]
    [HttpPost("Respond")] 
    public async Task<IActionResult> OnResponse([FromBody] Input input)
    {
        if (string.IsNullOrWhiteSpace(input.text))
            return BadRequest("O texto de entrada não pode estar vazio.");

        var response = await _chatBot.Respond(input.text);
        return Ok(response);
    }
}