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
            var appointments = await _repository.GetAllAsync();
            return appointments.Select(a => (AppointmentDto)a);
        }

        public async Task<AppointmentDto?> GetByIdAsync(Guid id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return null;
            return entity;
        }

        public async Task<AppointmentDto> CreateAsync(CreateAppointmentRequest request)
        {
            if (!_currentUser.Id.HasValue)
                throw new ValidationException("Contexto do usuário é obrigatório.");

            var appointment = new Appointment
            {
                UserId = _currentUser.Id.Value,
                CustomerId = request.CustomerId,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Status = request.Status,
            };

            return await _repository.CreateAsync(appointment);
        }

        public async Task<AppointmentDto> UpdateAsync(Guid id, AppointmentUpdateRequest request)
        {
            var appointment = await _repository.GetByIdAsync(id);
            if (appointment == null)
                throw new ValidationException("Agendamento não encontrado.");

            if (request.StartDate.HasValue)
                appointment.StartDate = request.StartDate.Value;
            if (request.EndDate.HasValue)
                appointment.EndDate = request.EndDate.Value;
            if (request.Status.HasValue)
                appointment.Status = request.Status.Value;

            appointment.UpdatedAt = DateTimeOffset.UtcNow;
            appointment = _repository.Update(appointment);
            await _repository.SaveChangesAsync();

            return appointment;
        }

        public async Task DeleteAsync(Guid id)
        {
            var appointment = await _repository.GetByIdAsync(id);
            if (appointment == null)
                throw new ValidationException("Agendamento não encontrado.");

            appointment.IsDeleted = true;
            appointment.UpdatedAt = DateTimeOffset.UtcNow;
            _repository.Update(appointment);
            await _repository.SaveChangesAsync();
        }
    }
}
