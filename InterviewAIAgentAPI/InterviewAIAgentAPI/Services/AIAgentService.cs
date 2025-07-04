using System.Threading.Tasks;

namespace InterviewAIAgentAPI.Services
{
    public class AIAgentService
    {
        private readonly ElevenLabsService _elevenLabsService;
        private readonly ILLMService _llmService; // Interface for your LLM (e.g., OpenAI)
        // You may need to inject Twilio media stream handler here

        public AIAgentService(ElevenLabsService elevenLabsService, ILLMService llmService)
        {
            _elevenLabsService = elevenLabsService;
            _llmService = llmService;
        }

        // This method simulates a single conversational turn
        public async Task<byte[]> ConverseAsync(byte[] candidateAudio, string voiceId)
        {
            // 1. Transcribe candidate's audio to text
            string candidateText = await _elevenLabsService.SpeechToTextAsync(candidateAudio);

            // 2. Generate AI response using LLM
            string aiResponse = await _llmService.GenerateResponseAsync(candidateText);

            // 3. Convert AI response to speech
            byte[] aiAudio = await _elevenLabsService.TextToSpeechAsync(aiResponse, voiceId);

            // 4. Return audio to be sent to Twilio room
            return aiAudio;
        }
    }
}