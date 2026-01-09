using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ona.Auth.Application.DTOs.Requests;
using Ona.Auth.Application.Interfaces.Services;
using Ona.Core.Common.Enums;
using Ona.ServiceDefaults.Attributes;

namespace Ona.Auth.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserAppService _userAppServices;

        public UsersController(IUserAppService userAppServices)
        {
            _userAppServices = userAppServices;
        }

        [HttpGet]
        [AuthorizeRoles(Role.Manager)]
        public async Task<IActionResult> List()
        {
            var users = await _userAppServices.ListAsync();
            return Ok(users);
        }

        [HttpPost]
        [AuthorizeRoles(Role.Manager)]
        public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
        {
            var user = await _userAppServices.CreateUserAsync(request);
            return Ok(user);
        }

        [HttpPost("invite")]
        [AuthorizeRoles(Role.Manager)]
        public async Task<IActionResult> Invite([FromBody] InviteUserRequest request)
        {
            await _userAppServices.InviteUserAsync(request);
            return Ok();
        }

        [HttpPost("accept-invite")]
        [AllowAnonymous]
        public async Task<IActionResult> AcceptInvite([FromBody] AcceptInviteRequest request)
        {
            await _userAppServices.AcceptInviteAsync(request);
            return Ok();
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            var user = await _userAppServices.GetMeAsync();
            return Ok(user);
        }

        [HttpPatch("me")]
        [Authorize]
        public async Task<IActionResult> UpdateCurrentUser(UserUpdateRequest request)
        {
            var user = await _userAppServices.UpdateMeAsync(request);
            return Ok(user);
        }

        [HttpPost("{id:guid}/block")]
        [AuthorizeRoles(Role.Manager)]
        public async Task<IActionResult> Block(Guid id)
        {
            await _userAppServices.BlockUserAsync(id);
            return NoContent();
        }

        [HttpPost("{id:guid}/unblock")]
        [AuthorizeRoles(Role.Manager)]
        public async Task<IActionResult> Unblock(Guid id)
        {
            await _userAppServices.UnblockUserAsync(id);
            return NoContent();
        }
    }
}
