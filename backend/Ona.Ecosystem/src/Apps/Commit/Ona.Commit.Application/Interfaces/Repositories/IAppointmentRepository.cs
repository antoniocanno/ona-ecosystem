using Ona.Commit.Application.DTOs.Responses;
using Ona.Commit.Domain.Entities;
using Ona.Core.Interfaces;

namespace Ona.Commit.Application.Interfaces.Repositories
{
    public interface IAppointmentRepository : IBaseRepository<Appointment>
    {
        Task<IEnumerable<AppointmentDto>> ListAsync(DateTimeOffset startDate, DateTimeOffset endDate, Guid professionalId);
    }
}
