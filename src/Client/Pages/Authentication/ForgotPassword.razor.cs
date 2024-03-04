using FSH.WebApi.Shared.Multitenancy;
using hotel_ize_frontend.Client.Components.Common;
using hotel_ize_frontend.Client.Infrastructure.ApiClient;
using hotel_ize_frontend.Client.Shared;
using Microsoft.AspNetCore.Components;

namespace hotel_ize_frontend.Client.Pages.Authentication;
public partial class ForgotPassword
{
    private readonly ForgotPasswordRequest _forgotPasswordRequest = new();
    private CustomValidation? _customValidation;
    private bool BusySubmitting { get; set; }

    [Inject]
    private IUsersClient UsersClient { get; set; } = default!;

    private string Tenant { get; set; } = MultitenancyConstants.Root.Id;

    private async Task SubmitAsync()
    {
        BusySubmitting = true;

        await ApiHelper.ExecuteCallGuardedAsync(
            () => UsersClient.ForgotPasswordAsync(Tenant, _forgotPasswordRequest),
            Snackbar,
            _customValidation);

        BusySubmitting = false;
    }
}