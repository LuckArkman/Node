using System.Text.Json;
using Dtos;
using Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Controllers;

[Route("api/[controller]")]
[ApiController]
public class ChatController : ControllerBase
{
    private readonly IChatBot _chatBot;
    private readonly IConfiguration _configuration;
    
    public ChatController(IChatBot chatBot, IConfiguration configuration)
    {
        _chatBot = chatBot;
        _configuration = configuration;
    }
    
    [HttpGet("Train")]
    public async Task<IActionResult> OnTrainer()
    {
        var result = await _chatBot.Train();
        return Ok(result);
    }
    
    [HttpGet("TokenAccess")]
    public async Task<IActionResult> OnTokenAccess()
    {
        var token = await GetTokenAccess() as NodeAuthResponse;
        _chatBot.SetToken<NodeAuthResponse>(token);
        return Ok(token.testToken);
    }

    private async Task<NodeAuthResponse> GetTokenAccess()
    {
        string url = _configuration["Dyson:BaseUrl"];

        using (HttpClientHandler handler = new HttpClientHandler())
        {
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
            using (HttpClient client = new HttpClient(handler))
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();
                    var token = JsonSerializer.Deserialize<NodeAuthResponse>(responseBody);
                    return token!;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error No Data: {ex.Message}");
                }
            }
        }
        return null;
    }

    [HttpGet("Connect")]
    public async Task<IActionResult> OnConnectServer()
    {
        var result = await _chatBot.OnConnectServerAsync();
        return Ok(result);
    }
    
    [HttpPost("Respond")] 
    public async Task<IActionResult> OnResponse([FromBody] Input input)
    {
        if (string.IsNullOrWhiteSpace(input.text))
            return BadRequest("O texto de entrada não pode estar vazio.");

        var response = await _chatBot.Respond(input.text);
        return Ok(response);
    }
}