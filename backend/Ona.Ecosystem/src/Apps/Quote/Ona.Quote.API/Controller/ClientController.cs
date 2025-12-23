using Microsoft.AspNetCore.Mvc;
using Ona.Core.Common.Enums;
using Ona.Quote.Application.DTOs.Request;
using Ona.Quote.Application.Interfaces.Services;
using Ona.ServiceDefaults.Attributes;

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
        [AuthorizeRoles(Role.Operator)]
        public async Task<IActionResult> CreateClient(ClientCreateRequest request)
        {
            var client = await _clientAppService.CreateAsync(request);
            return Ok(client);
        }

        [HttpPatch]
        [AuthorizeRoles(Role.Operator)]
        public async Task<IActionResult> UpdateCurrentClient(ClientUpdateRequest request)
        {
            var client = await _clientAppService.UpdateAsync(request);
            return Ok(client);
        }
    }
}
