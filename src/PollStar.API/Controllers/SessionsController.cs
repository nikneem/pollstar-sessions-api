using Azure.Messaging.WebPubSub;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PollStar.Core.Configuration;
using PollStar.Polls.Abstractions.Services;
using PollStar.Sessions.Abstractions.DataTransferObjects;
using PollStar.Sessions.Abstractions.Services;
using PollStar.Sessions.ErrorCodes;
using PollStar.Sessions.Exceptions;

namespace PollStar.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SessionsController : ControllerBase
    {
        private readonly IPollStarSessionsService _service;
        private readonly IPollStarPollsService _pollsService;
        private readonly IOptions<AzureConfiguration> _cloudConfiguration;

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

        [HttpGet("{id}/polls")]
        public async Task<IActionResult> Get(Guid id)
        {
            try
            {
                var pollsList = await _pollsService.GetPollsListAsync(id);
                return Ok(pollsList);
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
            IPollStarPollsService pollsService,
            IOptions<AzureConfiguration> cloudConfiguration)
        {
            _service = service;
            _pollsService = pollsService;
            _cloudConfiguration = cloudConfiguration;
        }

    }
}
