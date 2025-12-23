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
    }
}
