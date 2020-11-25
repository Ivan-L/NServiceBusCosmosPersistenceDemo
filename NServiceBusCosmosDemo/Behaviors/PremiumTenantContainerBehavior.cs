using System;
using System.Threading.Tasks;
using NServiceBus.Pipeline;

namespace NServiceBusCosmosDemo.Behaviors
{
    public class PremiumTenantContainerBehavior : Behavior<ITransportReceiveContext>
    {
        public override Task Invoke(ITransportReceiveContext context, Func<Task> next)
        {
            if (context.Message.Headers.TryGetValue("TenantId", out var tenantId))
            {
                // Check if tenant is a "premium" tenant and if it should have its own container.
                // For demo purposes, we assume that the presence of this header means it is a premium tenant.
                var containerInformation = TenantContainerInformation.ForTenant(tenantId);
                context.Extensions.Set(containerInformation);
            }
            
            return next();
        }
    }
}