using Ona.Commit.Application.DTOs;
using Ona.Commit.Application.DTOs.Request;
using Ona.Commit.Application.Interfaces.Repositories;
using Ona.Commit.Application.Interfaces.Services;
using Ona.Commit.Domain.Entities;
using Ona.Core.Common.Exceptions;
using Ona.Domain.Shared.Interfaces;

namespace Ona.Commit.Application.Services
{
    public class AppointmentAppService : IAppointmentAppService
    {
        private readonly ICurrentUser _currentUser;
        private readonly IAppointmentRepository _repository;

        public AppointmentAppService(
            ICurrentUser currentUser,
            IAppointmentRepository repository)
        {
            _currentUser = currentUser;
            _repository = repository;
        }

        public async Task<IEnumerable<AppointmentDto>> ListAsync()
        {
            var appointments = await _repository.GetAllAsync(a => a.Customer);
            return appointments.Select(a => (AppointmentDto)a);
        }

        public async Task<AppointmentDto?> GetByIdAsync(Guid id)
        {
            var entity = await _repository.GetByIdAsync(id, a => a.Customer);
            if (entity == null) return null;
            return entity;
        }

        public async Task<AppointmentDto> CreateAsync(CreateAppointmentRequest request)
        {
            if (!_currentUser.Id.HasValue)
                throw new ValidationException("Contexto do usuário é obrigatório.");

            var appointment = new Appointment(
                _currentUser.Id.Value,
                request.CustomerId,
                request.StartDate.ToUniversalTime(),
                request.EndDate.ToUniversalTime());

            appointment = await _repository.CreateAsync(appointment);
            await _repository.SaveChangesAsync();

            return appointment;
        }

        public async Task<AppointmentDto> UpdateAsync(Guid id, AppointmentUpdateRequest request)
        {
            var appointment = await _repository.GetByIdAsync(id);
            if (appointment == null)
                throw new ValidationException("Agendamento não encontrado.");

            if (request.StartDate.HasValue && request.EndDate.HasValue)
                appointment.Reschedule(
                    request.StartDate.Value.ToUniversalTime(),
                    request.EndDate.Value.ToUniversalTime());

            if (request.Status.HasValue)
                appointment.UpdateStatus(request.Status.Value);

            appointment = _repository.Update(appointment);
            await _repository.SaveChangesAsync();

            return appointment;
        }

        public async Task DeleteAsync(Guid id)
        {
            var appointment = await _repository.GetByIdAsync(id);
            if (appointment == null)
                throw new ValidationException("Agendamento não encontrado.");

            appointment.Delete();

            _repository.Update(appointment);
            await _repository.SaveChangesAsync();
        }
    }
}
