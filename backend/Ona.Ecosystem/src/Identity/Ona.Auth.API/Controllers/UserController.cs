using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ona.Auth.API.Extensions;
using Ona.Auth.Application.DTOs.Request;
using Ona.Auth.Application.Interfaces.Services;

namespace Ona.Auth.API.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly IUserAppService _userAppServices;

        public UserController(IUserAppService userAppServices)
        {
            _userAppServices = userAppServices;
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = User.GetUserId();
            var user = await _userAppServices.GetDtoByIdAsync(userId);
            return Ok(user);
        }

        [HttpPatch("me")]
        [Authorize]
        public async Task<IActionResult> UpdateCurrentUser(UserUpdateRequest request)
        {
            var userId = User.GetUserId();
            var user = await _userAppServices.UpdateAsync(userId, request);
            return Ok(user);
        }
    }
}
