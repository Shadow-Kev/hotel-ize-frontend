using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using hotel_ize_frontend.Client.Infrastructure.ApiClient;
using hotel_ize_frontend.Client.Components.EntityTable;
using FSH.WebApi.Shared.Authorization;
using Mapster;
using System.Security.Claims;

namespace hotel_ize_frontend.Client.Pages.Ize.Restaurations;
public partial class Ventes
{
    [CascadingParameter]
    protected Task<AuthenticationState> AuthState { get; set; } = default!;
    [Inject]
    protected IVentesClient VentesClient { get; set; } = default!;
    [Inject]
    protected IAgentsClient AgentsClient { get; set; } = default!;
    [Inject]
    protected IProductsClient ProductsClient { get; set; } = default!;

    protected EntityServerTableContext<VenteDto, Guid, UpdateVenteRequest> Context { get; set; } = default!;
    private EntityTable<VenteDto, Guid, UpdateVenteRequest> _table = default!;
    private List<VenteDto> _ventes { get; set; } = default!;
    private List<ProductDto> _products { get; set; } = new();
    private bool _isLoading { get; set; } = false;
    private string _searchString = "";

    protected override async void OnInitialized()
    {
        Context = new(
            entityName: L["Vente"],
            entityNamePlural: L["Ventes"],
            entityResource: FSHResource.Ventes,
            fields: new()
            {
                new(v => v.ProductNom, L["Produit"], "Produit"),
                new(v => v.Quantite, L["Quantités"], "Quantite"),
                new(v => v.AgentNom, L["Agent"], "Agent")
            },
            idFunc: v => v.Id,
            searchFunc: async filter =>
            {
                var venteFilter = filter.Adapt<SearchVentesRequest>();
                venteFilter.ProductId = SearchProductId == default ? null : SearchProductId;
                venteFilter.AgentId = SearchAgentId == default ? null : SearchAgentId;
                venteFilter.Prix = SearchVentePrix;
                venteFilter.Quantite = SearchVenteQuantite;

                var result = await VentesClient.SearchAsync(venteFilter);
                return result.Adapt<PaginationResponse<VenteDto>>();
            },
            createFunc: async v =>
            {
                var user = (await AuthState).User;
                string userIdString = user.GetUserId();
                Guid userCode = Guid.Parse(userIdString!);
                var agents = await AgentsClient.GetAllAsync();
                v.AgentId = agents.Where(_ => _.UserCode == userCode).Select(_ => _.Id).FirstOrDefault();
                await VentesClient.CreateAsync(v.Adapt<CreateVenteRequest>());
            },
            updateFunc: async (id, v) =>
            {
                var user = (await AuthState).User;
                string userIdString = user.GetUserId();
                Guid userCode = Guid.Parse(userIdString!);
                var agents = await AgentsClient.GetAllAsync();
                v.AgentId = agents.Where(_ => _.UserCode == userCode).Select(_ => _.Id).FirstOrDefault();
                await VentesClient.UpdateAsync(id, v);
            },
            deleteFunc: async id => await VentesClient.DeleteAsync(id)
        );
        await SearchProductToSell();
    }

    private async Task SearchProductToSell()
    {
        _isLoading = true;
        var productPaginationResponse = await ProductsClient.SearchAsync(new SearchProductsRequest()
        {
            Keyword = string.Empty,
            PageNumber = 1,
            PageSize = 10
        });
        if (string.IsNullOrEmpty(_searchString))
        {
            _products = productPaginationResponse.Data.ToList().Take(5).ToList();
        }
        else
        {
            _products = productPaginationResponse.Data
                .Where(p => p.Name.Contains(_searchString, StringComparison.InvariantCultureIgnoreCase))
                .ToList();
        }

        _isLoading = false;
    }

    private async Task<IEnumerable<Guid>> SearchProduct(string value)
    {
        return string.IsNullOrEmpty(value)
            ? _products.Select(_ => _.Id)
            : _products.Where(_ => _.Name.Contains(value, StringComparison.InvariantCultureIgnoreCase))
                .Select(_ => _.Id)
                .ToList();
    }

    private Guid _searchProductId;
    private Guid SearchProductId
    {
        get => _searchProductId;
        set
        {
            _searchProductId = value;
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

    private decimal? _searchVentePrix;
    private decimal? SearchVentePrix
    {
        get => _searchVentePrix;
        set
        {
            _searchVentePrix = value;
            _ = _table.ReloadDataAsync();
        }
    }

    private int? _searchVenteQuantite;
    private int? SearchVenteQuantite
    {
        get => _searchVenteQuantite;
        set
        {
            _searchVenteQuantite = value;
            _ = _table.ReloadDataAsync();
        }
    }
}