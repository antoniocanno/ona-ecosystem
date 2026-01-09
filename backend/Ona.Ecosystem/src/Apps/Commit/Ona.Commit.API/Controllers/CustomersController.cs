using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ona.Commit.Application.DTOs.Requests;
using Ona.Commit.Application.Interfaces.Services;
using Ona.Core.Common.Enums;
using Ona.ServiceDefaults.Attributes;

namespace Ona.Commit.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerAppService _customerAppService;

        public CustomersController(ICustomerAppService customerAppService)
        {
            _customerAppService = customerAppService;
        }

        [HttpGet]
        [AuthorizeRoles(Role.ReadOnly)]
        public async Task<IActionResult> GetAll()
        {
            var customers = await _customerAppService.ListAsync();
            return Ok(customers);
        }

        [HttpGet("{id:guid}")]
        [AuthorizeRoles(Role.ReadOnly)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var customer = await _customerAppService.GetByIdAsync(id);
            if (customer == null)
                return NotFound();
            return Ok(customer);
        }

        [HttpPost]
        [AuthorizeRoles(Role.Operator)]
        public async Task<IActionResult> Create([FromBody] CreateCustomerRequest request)
        {
            var customer = await _customerAppService.CreateAsync(request);
            return Ok(customer);
        }

        [HttpPatch("{id:guid}")]
        [AuthorizeRoles(Role.Operator)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCustomerRequest request)
        {
            var customer = await _customerAppService.UpdateAsync(id, request);
            return Ok(customer);
        }

        [HttpDelete("{id:guid}")]
        [AuthorizeRoles(Role.Operator)]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _customerAppService.DeleteAsync(id);
            return NoContent();
        }
    }
}
