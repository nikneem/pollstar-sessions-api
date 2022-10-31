using Azure.Messaging.WebPubSub;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PollStar.Core.Configuration;
using PollStar.Sessions.Abstractions.DataTransferObjects;
using PollStar.Sessions.Abstractions.Services;
using PollStar.Sessions.ErrorCodes;
using PollStar.Sessions.Exceptions;

namespace PollStar.Sessions.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SessionsController : ControllerBase
    {
        private readonly IPollStarSessionsService _service;
        private readonly IOptions<AzureConfiguration> _cloudConfiguration;

        [HttpGet]
        public async Task<IActionResult> List([FromQuery] Guid userId)
        {
            try
            {
                var service = await _service.ListSessionsAsync(userId);
                return Ok(service);
            }
            catch (PollStarSessionException psEx)
            {
                if (psEx.ErrorCode == PollStarSessionErrorCode.SessionNotFound)
                {
                    return new NotFoundResult();
                }
            }

            return BadRequest();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id, [FromQuery] Guid userId)
        {
            try
            {
                var service = await _service.GetSessionAsync(id, userId);
                return Ok(service);
            }
            catch (PollStarSessionException psEx)
            {
                if (psEx.ErrorCode == PollStarSessionErrorCode.SessionNotFound)
                {
                    return new NotFoundResult();
                }
            }

            return BadRequest();
        }

        [HttpGet("{code}/details")]
        public async Task<IActionResult> GetByCode(string code, [FromQuery] Guid userId)
        {
            try
            {
                var service = await _service.GetSessionByCodeAsync(code, userId);
                return Ok(service);
            }
            catch (PollStarSessionException psEx)
            {
                if (psEx.ErrorCode == PollStarSessionErrorCode.SessionNotFound)
                {
                    return new NotFoundResult();
                }
            }

            return BadRequest();
        }

        [HttpGet("{id}/realtime")]
        public async Task<IActionResult> GetRealtimeConnection(Guid id, [FromQuery] Guid userId)
        {
            try
            {
                var pubsubClient = new WebPubSubServiceClient(_cloudConfiguration.Value.WebPubSub,
                    _cloudConfiguration.Value.PollStarHub);
                var clientAccess = await pubsubClient.GetClientAccessUriAsync(
                    userId: userId.ToString(),
                    roles: new[]
                    {
                        $"webpubsub.sendToGroup.{id}",
                        $"webpubsub.joinLeaveGroup.{id}"
                    });
                return Ok(new {pubsubClientUrl = clientAccess});
            }
            catch (PollStarSessionException psEx)
            {
                if (psEx.ErrorCode == PollStarSessionErrorCode.SessionNotFound)
                {
                    return new NotFoundResult();
                }
            }

            return BadRequest();
        }

        [HttpPost]
        public async Task<IActionResult> Post(CreateSessionDto dto)
        {
                var createdService = await _service.CreateSessionAsync(dto);
                return Ok(createdService);
        }



        public SessionsController(
            IPollStarSessionsService service, 
            IOptions<AzureConfiguration> cloudConfiguration)
        {
            _service = service;
            _cloudConfiguration = cloudConfiguration;
        }

    }
}
