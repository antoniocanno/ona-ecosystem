using Ona.Quote.Domain.Entities;

namespace Ona.Quote.Domain.Interfaces.Services
{
    public interface IClientService
    {
        void Validate(Client client);
    }
}
