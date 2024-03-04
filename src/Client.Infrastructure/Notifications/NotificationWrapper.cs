using FSH.WebApi.Shared.Notifications;
using MediatR;

namespace hotel_ize_frontend.Client.Infrastructure.Notifications;
public class NotificationWrapper<TNotificationMessage> : INotification
    where TNotificationMessage : INotificationMessage
{
    public NotificationWrapper(TNotificationMessage notification) => Notification = notification;

    public TNotificationMessage Notification { get; }
}