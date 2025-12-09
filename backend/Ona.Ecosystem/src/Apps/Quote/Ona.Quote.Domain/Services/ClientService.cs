using Ona.Core.Common.Exceptions;
using Ona.Quote.Domain.Entities;
using Ona.Quote.Domain.Interfaces.Services;

namespace Ona.Quote.Domain.Services
{
    public class ClientService : IClientService
    {
        public void Validate(Client client)
        {
            if (client == null)
                throw new ValidationException("Cliente inválido.");
        }
    }
}
