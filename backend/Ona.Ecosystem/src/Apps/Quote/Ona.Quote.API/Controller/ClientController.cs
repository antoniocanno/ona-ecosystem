using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ona.Core.Common.Extensions;
using Ona.Quote.Application.DTOs.Request;
using Ona.Quote.Application.Interfaces.Services;
using Ona.ServiceDefaults.ApiExtensions;

namespace Ona.Quote.API.Controller
{
    [ApiController]
    [Route("api/clients")]
    public class ClientController : ControllerBase
    {
        private readonly IClientAppService _clientAppService;

        public ClientController(IClientAppService clientAppService)
        {
            _clientAppService = clientAppService;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateClient(ClientCreateRequest request)
        {
            var userId = User.GetUserId().ToGuid();
            var client = await _clientAppService.CreateAsync(userId, request);
            return Ok(client);
        }

        [HttpPatch]
        [Authorize]
        public async Task<IActionResult> UpdateCurrentClient(ClientUpdateRequest request)
        {
            var userId = User.GetUserId().ToGuid();
            var client = await _clientAppService.UpdateAsync(userId, request);
            return Ok(client);
        }
    }
}
