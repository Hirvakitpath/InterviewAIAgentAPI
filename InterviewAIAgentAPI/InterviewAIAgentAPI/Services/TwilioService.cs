using InterviewAIAgentAPI.Models;
using Microsoft.Extensions.Options;
using Twilio.Jwt.AccessToken;

namespace InterviewAIAgentAPI.Services
{
    public class TwilioService
    {
        private readonly TwilioSettings _settings;

        public TwilioService(IOptions<TwilioSettings> options)
        {
            _settings = options.Value;
        }

        public string GenerateAccessToken(string identity, string roomName)
        {
            var videoGrant = new VideoGrant
            {
                Room = roomName
            };

            var token = new Token(
                _settings.AccountSid,
                _settings.ApiKeySid,
                _settings.ApiKeySecret,
                identity,
                grants: new HashSet<IGrant> { videoGrant }
            );

            return token.ToJwt();
        }
    }
}
