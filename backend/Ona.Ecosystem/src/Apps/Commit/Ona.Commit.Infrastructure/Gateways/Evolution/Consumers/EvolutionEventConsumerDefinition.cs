using MassTransit;

namespace Ona.Commit.Infrastructure.Gateways.Evolution.Consumers
{
    public class EvolutionEventConsumerDefinition : ConsumerDefinition<EvolutionEventConsumer>
    {
        public EvolutionEventConsumerDefinition()
        {
            EndpointName = "evolution-tracking-queue";
        }

        protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator, IConsumerConfigurator<EvolutionEventConsumer> consumerConfigurator, IRegistrationContext context)
        {
            if (endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rabbit)
            {
                rabbit.Durable = true;
                rabbit.AutoDelete = false;

                rabbit.ConfigureConsumeTopology = false;
                rabbit.ClearSerialization();
                rabbit.UseRawJsonSerializer();

                rabbit.Bind("evolution_exchange", s =>
                {
                    s.ExchangeType = "topic";
                    s.RoutingKey = "#";
                });
            }
        }
    }
}
