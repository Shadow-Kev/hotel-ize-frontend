using FSH.WebApi.Shared.Authorization;
using hotel_ize_frontend.Client.Infrastructure.Auth;
using hotel_ize_frontend.Client.Infrastructure.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace hotel_ize_frontend.Client.Shared;
public partial class NavMenu
{
    [CascadingParameter]
    protected Task<AuthenticationState> AuthState { get; set; } = default!;
    [Inject]
    protected IAuthorizationService AuthService { get; set; } = default!;

    private string? _hangfireUrl;
    private bool _canViewHangfire;
    private bool _canViewDashboard;
    private bool _canViewRoles;
    private bool _canViewUsers;
    private bool _canViewProducts;
    private bool _canViewBrands;
    private bool _canViewTenants;
    private bool _canViewTypeChambres;
    private bool _canViewAgents;
    private bool _canViewChambres;
    private bool _canAddOrEditChambres;
    private bool _canViewClients;
    private bool _canViewVentes;
    private bool _canViewTypeReservations;
    private bool _canAddOrEditTypeReservations;
    private bool _canViewReservations;

    private bool CanViewAdministrationGroup => _canViewUsers || _canViewRoles || _canViewTenants;
    private bool CanViewConfigurationGroup => _canViewTypeChambres || _canViewAgents || _canViewChambres || _canViewTypeReservations;
    private bool CanViewReceptionGroup => _canViewClients || _canViewReservations;

    protected override async Task OnParametersSetAsync()
    {
        _hangfireUrl = "http://localhost:5000/jobs";
        var user = (await AuthState).User;
        _canViewHangfire = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.Hangfire);
        _canViewDashboard = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.Dashboard);
        _canViewRoles = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.Roles);
        _canViewUsers = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.Users);
        _canViewProducts = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.Products);
        _canViewBrands = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.Brands);
        _canViewTenants = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.Tenants);
        _canViewTypeChambres = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.TypeChambres);
        _canViewTypeReservations = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.TypeReservations);
        _canAddOrEditTypeReservations = await AuthService.HasPermissionAsync(user, FSHAction.Create, FSHResource.TypeReservations);
        _canViewAgents = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.Agents);
        _canViewChambres = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.Chambres);
        _canAddOrEditChambres = await AuthService.HasPermissionAsync(user, FSHAction.Create, FSHResource.Chambres);
        _canViewClients = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.Clients);
        _canViewVentes = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.Ventes);
        _canViewReservations = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.Reservations);
    }
}