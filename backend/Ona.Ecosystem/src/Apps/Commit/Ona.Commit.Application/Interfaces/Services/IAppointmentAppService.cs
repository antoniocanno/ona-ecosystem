using Ona.Commit.Application.DTOs;
using Ona.Commit.Application.DTOs.Request;
using Ona.Commit.Domain.Entities;
using Ona.Domain.Shared.Interfaces;

namespace Ona.Commit.Application.Interfaces.Services
{
    public interface IAppointmentAppService
    {
        Task<IEnumerable<AppointmentDto>> ListAsync();
        Task<AppointmentDto?> GetByIdAsync(Guid id);
        Task<AppointmentDto> CreateAsync(CreateAppointmentRequest request);
        Task<AppointmentDto> UpdateAsync(Guid id, AppointmentUpdateRequest request);
        Task DeleteAsync(Guid id);
    }
}
