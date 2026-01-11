using Ona.Commit.Application.DTOs.Requests;
using Ona.Commit.Application.DTOs.Responses;

namespace Ona.Commit.Application.Interfaces.Services
{
    public interface IAppointmentAppService
    {
        Task<IEnumerable<AppointmentDto>> ListAsync();
        Task<IEnumerable<AppointmentDto>> ListAsync(DateTimeOffset startDate, DateTimeOffset endDate, Guid professionalId);
        Task<AppointmentDto?> GetByIdAsync(Guid id);
        Task<AppointmentDto> CreateAsync(CreateAppointmentRequest request);
        Task<IEnumerable<AppointmentDto>> CreateBulkAsync(IEnumerable<CreateAppointmentRequest> requests);
        Task<AppointmentDto> UpdateAsync(Guid id, AppointmentUpdateRequest request);
        Task DeleteAsync(Guid id);
    }
}
