using Hangfire;
using Microsoft.Extensions.Caching.Distributed;
using Ona.Commit.Application.DTOs.Requests;
using Ona.Commit.Application.DTOs.Responses;
using Ona.Commit.Application.Interfaces.Repositories;
using Ona.Commit.Application.Interfaces.Services;
using Ona.Commit.Domain.Entities;
using Ona.Commit.Domain.Enums;
using Ona.Commit.Domain.Events;
using Ona.Core.Common.Events;
using Ona.Core.Common.Exceptions;
using Ona.Core.Interfaces;
using System.Text.Json;

namespace Ona.Commit.Application.Services
{
    public class AppointmentAppService : IAppointmentAppService
    {
        private readonly ICurrentUser _currentUser;
        private readonly IAppointmentRepository _repository;
        private readonly IDistributedCache _cache;
        private readonly IDomainEventDispatcher _eventDispatcher;

        public AppointmentAppService(
            ICurrentUser currentUser,
            IAppointmentRepository repository,
            IDistributedCache cache,
            IDomainEventDispatcher eventDispatcher)
        {
            _currentUser = currentUser;
            _repository = repository;
            _cache = cache;
            _eventDispatcher = eventDispatcher;
        }

        public async Task<IEnumerable<AppointmentDto>> ListAsync()
        {
            var appointments = await _repository.GetAllAsync(a => a.Customer!);
            return appointments.Select(a => (AppointmentDto)a);
        }

        public async Task<IEnumerable<AppointmentDto>> ListAsync(DateTimeOffset startDate, DateTimeOffset endDate, Guid professionalId)
        {
            return await _repository.ListAsync(startDate, endDate, professionalId);
        }

        public async Task<AppointmentDto?> GetByIdAsync(Guid id)
        {
            var cacheKey = $"appointment:{id}";
            var cached = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cached))
            {
                return JsonSerializer.Deserialize<AppointmentDto>(cached);
            }

            var entity = await _repository.GetByIdAsync(id, a => a.Customer!);

            if (entity == null)
                throw new NotFoundException("Agendamento não encontrado.");

            var dto = (AppointmentDto)entity;
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(dto), new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
            });

            return dto;
        }

        public async Task<AppointmentDto> CreateAsync(CreateAppointmentRequest request)
        {
            if (_currentUser.Id == Guid.Empty)
                throw new ValidationException("Contexto do usuário é obrigatório.");

            var appointment = new Appointment(
                request.CustomerId,
                request.ProfessionalId,
                request.Summary,
                request.Description,
                request.StartDate.ToUniversalTime(),
                request.EndDate.ToUniversalTime());

            appointment = await _repository.CreateAsync(appointment);
            await _repository.SaveChangesAsync();

            BackgroundJob.Enqueue<ICalendarService>(x => x.CreateAppointmentEventAsync(appointment, request.ExternalEventId, request.Provider));

            return appointment;
        }

        public async Task<IEnumerable<AppointmentDto>> CreateBulkAsync(IEnumerable<CreateAppointmentRequest> requests)
        {
            if (_currentUser.Id == Guid.Empty)
                throw new ValidationException("Contexto do usuário é obrigatório.");

            var appointments = new List<(Appointment entity, CreateAppointmentRequest request)>();

            foreach (var request in requests)
            {
                var appointment = new Appointment(
                    request.CustomerId,
                    request.ProfessionalId,
                    request.Summary,
                    request.Description,
                    request.StartDate.ToUniversalTime(),
                    request.EndDate.ToUniversalTime());

                await _repository.CreateAsync(appointment);
                appointments.Add((appointment, request));
            }

            await _repository.SaveChangesAsync();

            foreach (var pair in appointments)
            {
                BackgroundJob.Enqueue<ICalendarService>(x => x.CreateAppointmentEventAsync(pair.entity, pair.request.ExternalEventId, pair.request.Provider));
            }

            return appointments.Select(pair => (AppointmentDto)pair.entity);
        }

        public async Task<AppointmentDto> UpdateAsync(Guid id, AppointmentUpdateRequest request)
        {
            var appointment = await _repository.GetByIdAsync(id);
            if (appointment == null)
                throw new NotFoundException("Agendamento não encontrado.");

            if (request.StartDate.HasValue && request.EndDate.HasValue)
                appointment.Reschedule(
                    request.StartDate.Value.ToUniversalTime(),
                    request.EndDate.Value.ToUniversalTime());

            if (request.Status.HasValue)
                appointment.UpdateStatus(request.Status.Value);

            if (!string.IsNullOrEmpty(request.Summary))
                appointment.UpdateSummary(request.Summary);

            if (!string.IsNullOrEmpty(request.Description))
                appointment.UpdateDescription(request.Description);

            appointment = _repository.Update(appointment);
            await _repository.SaveChangesAsync();
            await _cache.RemoveAsync($"appointment:{id}");

            BackgroundJob.Enqueue<ICalendarService>(x => x.UpdateAppointmentEventAsync(appointment));

            return appointment;
        }

        public async Task DeleteAsync(Guid id)
        {
            var appointment = await _repository.GetByIdAsync(id);
            if (appointment == null)
                throw new NotFoundException("Agendamento não encontrado.");

            appointment.Delete();

            _repository.Update(appointment);
            await _repository.SaveChangesAsync();
            await _cache.RemoveAsync($"appointment:{id}");

            BackgroundJob.Enqueue<ICalendarService>(x => x.DeleteAppointmentEventAsync(appointment));
        }

        public async Task ConfirmAsync(Guid id)
        {
            var appointment = await _repository.GetByIdAsync(id);
            if (appointment == null)
                throw new NotFoundException("Agendamento não encontrado.");

            appointment.Confirm();

            _repository.Update(appointment);
            await _repository.SaveChangesAsync();
            await _cache.RemoveAsync($"appointment:{id}");

            BackgroundJob.Enqueue<ICalendarService>(x => x.UpdateAppointmentEventAsync(appointment));
        }

        public async Task CancelAsync(Guid id)
        {
            var appointment = await _repository.GetByIdAsync(id);
            if (appointment == null)
                throw new NotFoundException("Agendamento não encontrado.");

            appointment.UpdateStatus(AppointmentStatus.Canceled);

            _repository.Update(appointment);
            await _repository.SaveChangesAsync();
            await _cache.RemoveAsync($"appointment:{id}");

            await _eventDispatcher.DispatchAsync(new AppointmentCancelledByPatientEvent(appointment));
        }
    }
}
