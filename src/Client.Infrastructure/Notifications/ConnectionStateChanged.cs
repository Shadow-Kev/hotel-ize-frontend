using FSH.WebApi.Shared.Notifications;

namespace hotel_ize_frontend.Client.Infrastructure.Notifications;
public record ConnectionStateChanged(ConnectionState State, string? Message) : INotificationMessage;