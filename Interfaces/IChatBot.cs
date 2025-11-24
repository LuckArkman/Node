namespace Interfaces;

public interface IChatBot
{
    Task<string> Train();
    Task<string> Respond(string inputText);
    Task<string> OnConnectServerAsync();
    void SetToken<T>(T token);
}