public interface ILLMService
{
    Task<string> GenerateResponseAsync(string prompt);
}