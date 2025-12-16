using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ona.Auth.Application.DTOs.Request;
using Ona.Auth.Application.Interfaces.Services;
using Ona.Auth.Domain.Entities;
using Ona.Core.Common.Extensions;
using Ona.ServiceDefaults.ApiExtensions;

namespace Ona.Auth.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    // Allow Owner to also manage users if needed, or strictly Admin of the tenant.
    // If Owner is super-admin, maybe they can switch tenants.
    // Assuming "Admin" is the tenant administrator.
    public class UsersController : ControllerBase
    {
        private readonly IUserAppService _userAppServices;

        public UsersController(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, IUserAppService userAppServices)
        {
            _userAppServices = userAppServices;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Owner")]
        public async Task<IActionResult> List()
        {
            var users = await _userAppServices.ListAsync();
            return Ok(users);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Owner")]
        public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
        {
            var user = await _userAppServices.CreateUserAsync(request);
            return Ok(user);
        }

        [HttpPost("invite")]
        [Authorize(Roles = "Admin,Owner")]
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
            var userId = User.GetUserId().ToGuid();
            var user = await _userAppServices.GetByIdAsync(userId);
            return Ok(user);
        }

        [HttpPatch("me")]
        [Authorize]
        public async Task<IActionResult> UpdateCurrentUser(UserUpdateRequest request)
        {
            var userId = User.GetUserId().ToGuid();
            var user = await _userAppServices.UpdateAsync(userId, request);
            return Ok(user);
        }

        [HttpPost("{id:guid}/block")]
        [Authorize(Roles = "Admin,Owner")]
        public async Task<IActionResult> Block(Guid id)
        {
            await _userAppServices.BlockUserAsync(id);
            return NoContent();
        }

        [HttpPost("{id:guid}/unblock")]
        [Authorize(Roles = "Admin,Owner")]
        public async Task<IActionResult> Unblock(Guid id)
        {
            await _userAppServices.UnblockUserAsync(id);
            return NoContent();
        }
    }
}
