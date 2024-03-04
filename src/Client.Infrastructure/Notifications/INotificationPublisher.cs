using FSH.WebApi.Shared.Notifications;

namespace hotel_ize_frontend.Client.Infrastructure.Notifications;
public interface INotificationPublisher
{
    Task PublishAsync(INotificationMessage notification);
}