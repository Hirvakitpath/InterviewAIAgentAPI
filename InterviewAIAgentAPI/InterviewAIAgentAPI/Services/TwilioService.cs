using InterviewAIAgentAPI.Models;
using Microsoft.Extensions.Options;
using Twilio.Jwt.AccessToken;
using Twilio;
using Twilio.Rest.Video.V1;

namespace InterviewAIAgentAPI.Services
{
    public class TwilioService
    {
        private readonly TwilioSettings _settings;

        public TwilioService(IOptions<TwilioSettings> options)
        {
            _settings = options.Value;
            TwilioClient.Init(_settings.AccountSid, _settings.AuthToken);
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

        public string GenerateVideoRoomLink(Guid candidateId)
        {
            // Create a unique room name using the candidate ID
            string roomName = $"interview_{candidateId}";

            // Create the room if it doesn't exist (idempotent)
            var room = RoomResource.Create(
                uniqueName: roomName,
                type: RoomResource.RoomTypeEnum.Group
            );

            // Construct a join link (for demonstration, you may need a frontend to handle this)
            // Replace with your actual frontend URL that handles Twilio video rooms
            string joinUrl = $"https://your-frontend-app.com/video/{roomName}?candidateId={candidateId}";
            return joinUrl;
        }
    }
}
