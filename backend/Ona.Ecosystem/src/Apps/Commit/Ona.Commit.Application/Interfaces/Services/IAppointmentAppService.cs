using Ona.Commit.Application.DTOs.Requests;
using Ona.Commit.Application.DTOs.Responses;

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
