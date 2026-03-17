using Microsoft.EntityFrameworkCore;
using Ona.Commit.Application.DTOs.Responses;
using Ona.Commit.Application.Interfaces.Repositories;
using Ona.Commit.Domain.Entities;
using Ona.Commit.Infrastructure.Data;
using Ona.Infrastructure.Shared.Repositories;

namespace Ona.Commit.Infrastructure.Repositories
{
    public class AppointmentRepository : BaseRepository<CommitDbContext, Appointment>, IAppointmentRepository
    {
        public AppointmentRepository(CommitDbContext context)
            : base(context)
        {
        }

        public async Task<IEnumerable<AppointmentDto>> ListAsync(DateTimeOffset startDate, DateTimeOffset endDate, Guid professionalId)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(a => a.ProfessionalId == professionalId &&
                            a.StartDate < endDate &&
                            a.EndDate > startDate &&
                            !a.IsDeleted)
                .Select(a => new AppointmentDto
                {
                    Id = a.Id,
                    CustomerId = a.CustomerId,
                    StartDate = a.StartDate,
                    EndDate = a.EndDate,
                    Summary = a.Summary ?? string.Empty,
                    Status = a.Status
                })
                .ToListAsync();
        }
    }
}
