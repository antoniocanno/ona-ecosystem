using Ona.Commit.Application.DTOs.Responses;
using Ona.Commit.Application.Interfaces.Repositories;
using Ona.Commit.Application.Interfaces.Services;
using Ona.Core.Common.Exceptions;

namespace Ona.Commit.Application.Services
{
    public class AlertAppService : IAlertAppService
    {
        private readonly IOperatorAlertRepository _repository;

        public AlertAppService(IOperatorAlertRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<AlertDto>> GetUnreadAsync()
        {
            var alerts = await _repository.GetUnreadAsync();
            return alerts.Select(a => new AlertDto
            {
                Id = a.Id,
                Title = a.Title,
                Message = a.Message,
                IsRead = a.IsRead,
                CreatedAt = a.CreatedAt,
                RelatedAppointmentId = a.RelatedAppointmentId
            });
        }

        public async Task MarkAsReadAsync(Guid id)
        {
            var alert = await _repository.GetByIdAsync(id);
            if (alert == null) throw new NotFoundException("Alerta não encontrado");

            alert.MarkAsRead();
            _repository.Update(alert);
            await _repository.SaveChangesAsync();
        }
    }
}
