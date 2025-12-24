using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Ona.Domain.Shared.Interfaces;

namespace Ona.Commit.Infrastructure.Data
{
    public class CommitDbContextFactory : IDesignTimeDbContextFactory<CommitDbContext>
    {
        public CommitDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<CommitDbContext>();
            optionsBuilder.UseNpgsql("Host=localhost;Database=OnaCommit;Username=postgres;Password=postgres");

            return new CommitDbContext(optionsBuilder.Options, null!);
        }
    }
}
