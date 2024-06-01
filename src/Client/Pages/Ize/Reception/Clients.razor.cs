using FSH.WebApi.Shared.Authorization;
using hotel_ize_frontend.Client.Components.EntityTable;
using hotel_ize_frontend.Client.Infrastructure.ApiClient;
using Mapster;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using System.Security.Claims;

namespace hotel_ize_frontend.Client.Pages.Ize.Reception;
public partial class Clients
{
    [CascadingParameter]
    protected Task<AuthenticationState> AuthState { get; set; } = default!;
    [Inject]
    protected IClientsClient ClientsClient { get; set; } = default!;
    [Inject]
    protected IAgentsClient AgentsClient { get; set; } = default!;
    [Inject]
    protected IChambresClient ChambresClient { get; set; } = default!;
    [Inject]
    protected IPdfsClient PdfsClient {  get; set; } = default!;

    protected EntityServerTableContext<ClientDto, Guid, UpdateClientRequest> Context { get; set; } = default!;

    private EntityTable<ClientDto, Guid, UpdateClientRequest> _table = default!;
    private List<ClientDto> _clients { get; set; } = default!;
    private List<ChambreDetailsDto> _chambres { get; set; } = new();
    private bool _isLoading { get; set; } = false;

    protected override async void OnInitialized()
    {
        Context = new(
            entityName: L["Client"],
            entityNamePlural: L["Clients"],
            entityResource: FSHResource.Clients,
            fields: new()
            {
                new(c => c.Nom, L["Nom"], "Nom"),
                new(c => c.Prenom, L["Prenom"], "Prenom"),
                new(c => c.DateArrive, L["Date d'arrivée"], "DateArrive"),
                new(c => c.DateDepart, L["Date de départ"], "DateDepart"),
                new(c => c.ChambreNom, L["Chambre"], "ChambreNom")
            },
            idFunc: c => c.Id,
            searchFunc: async filter =>
            {
                var clientFilter = filter.Adapt<SearchClientsRequest>();

                clientFilter.ChambreId = SearchChambreId == default ? null : SearchChambreId;
                clientFilter.AgentId = SearchAgentId == default ? null : SearchAgentId;
                clientFilter.Nom = SearchClientNom;

                var result = await ClientsClient.SearchAsync(clientFilter);
                return result.Adapt<PaginationResponse<ClientDto>>();
            },
            createFunc: async c =>
            {
                var user = (await AuthState).User;
                string userIdString = user.GetUserId();
                Guid userCode = Guid.Parse(userIdString!);
                var agents = await AgentsClient.GetAllAsync();
                c.AgentId = agents.Where(_ => _.UserCode == userCode).Select(_ => _.Id).FirstOrDefault();
                await ClientsClient.CreateAsync(c.Adapt<CreateClientRequest>());
            },
            updateFunc: async (id, c) =>
            {
                var user = (await AuthState).User;
                string userIdString = user.GetUserId();
                Guid userCode = Guid.Parse(userIdString!);
                var agents = await AgentsClient.GetAllAsync();
                c.AgentId = agents.Where(_ => _.UserCode == userCode).Select(_ => _.Id).FirstOrDefault();
                await ClientsClient.UpdateAsync(id, c);
            },
            deleteFunc: async id => await ClientsClient.DeleteAsync(id),
            hasExtraActionsFunc: () => true
        );
        await GetChambres();
    }

    private Guid _searchChambreId;
    private Guid SearchChambreId
    {
        get => _searchChambreId;
        set
        {
            _searchChambreId = value;
            _ = _table.ReloadDataAsync();
        }
    }

    private Guid _searchAgentId;
    private Guid SearchAgentId
    {
        get => _searchAgentId;
        set
        {
            _searchAgentId = value;
            _ = _table.ReloadDataAsync();
        }
    }

    private string? _searchClientNom;
    private string? SearchClientNom
    {
        get => _searchClientNom;
        set
        {
            _searchClientNom = value;
            _ = _table.ReloadDataAsync();
        }
    }

    private async Task GetChambres()
    {
        _isLoading = true;
        var response = await ChambresClient.GetAllAsync();
        if (response.Count > 0)
        {
            _chambres = response.ToList();
            _isLoading = false;
        }
    }

    private async Task<IEnumerable<Guid?>> SearchChambre(string value)
    {
        return string.IsNullOrEmpty(value)
            ? _chambres.Select(_ => (Guid?)_.Id)
            : _chambres.Where(_ => _.Nom.Contains(value, StringComparison.InvariantCultureIgnoreCase))
                .Select(_ => (Guid?)_.Id)
                .ToList();
    }

    private async void GenererFactureClient(Guid id)
    {
        if (id != Guid.Empty)
        {
            var response = await PdfsClient.PrintFactureClientAsync(id);
            if (response.StatusCode == 200)
            {
                Snackbar.Add("Facture générer avec succès", Severity.Success);
            }
            else
            {
                Snackbar.Add("Erreur lors de la génération de la facture", Severity.Error);
            }
        }
    }
}