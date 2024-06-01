using FSH.WebApi.Shared.Authorization;
using hotel_ize_frontend.Client.Components.EntityTable;
using hotel_ize_frontend.Client.Infrastructure.ApiClient;
using Mapster;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
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
    [Inject]
    protected IPdfsClient PdfsClient { get; set; } = default!;
    [Inject]
    protected IClientsClient ClientsClient { get; set; } = default!;

    protected EntityServerTableContext<VenteDto, Guid, UpdateVenteRequest> Context { get; set; } = default!;
    private EntityTable<VenteDto, Guid, UpdateVenteRequest> _table = default!;
    private List<VenteDto> _ventes { get; set; } = default!;
    private List<ProductDto> _products { get; set; } = new();
    private List<ClientDetailsDto> _clients { get; set; } = new();
    private Guid _productId;
    private Guid? _clientId;
    private int _quantite;
    private List<ProductQuantite> _productQuantites { get; set; } = new();
    private List<VenteProduitDto> _venteProduits { get; set; } = new();
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
                new((v) =>
                {
                    var productNames = new List<string>();
                    var prods = v.VenteProduits.ToList();
                    foreach(var prod in prods)
                    {
                        productNames.Add(prod?.Product?.Name);
                    }

                    return string.Join(", ", productNames);
                }, L["Produit"], "Produit"),
                new((v) =>
                {
                    var productQuantites = new List<string>();
                    var prods = v.VenteProduits.ToList();
                    foreach (var prod in prods)
                    {
                        productQuantites.Add(prod?.Quantite.ToString()!);
                    }

                    return string.Join(", ", productQuantites);
                }, L["Quantités"], "Quantite"),
                new((v) =>
                {
                    var productPrix = new List<string>();
                    var prods = v.VenteProduits.ToList();
                    foreach (var prod in prods)
                    {
                        productPrix.Add(prod?.Prix.ToString()!);
                    }

                    return string.Join(", ", productPrix);
                }, L["Prix Unitaire"], "PU"),
                new(v => v.VenteProduits.Sum(_ => _.Quantite * _.Prix).ToString(), L["Total"], "Total"),
                new(v => v.AgentNom, L["Agent"], "Agent"),
                new(v => v.ClientNom ?? "", L["Client"], "Client")
            },
            idFunc: v => v.Id,
            searchFunc: async filter =>
            {
                var venteFilter = filter.Adapt<SearchVentesRequest>();
                venteFilter.AgentId = SearchAgentId == default ? null : SearchAgentId;

                var result = await VentesClient.SearchAsync(venteFilter);
                return result.Adapt<PaginationResponse<VenteDto>>();
            },
            createFunc: async v =>
            {
                var user = (await AuthState).User;
                string userIdString = user.GetUserId();
                Guid userCode = Guid.Parse(userIdString!);
                var agents = await AgentsClient.GetAllAsync();
                var agentOnline = agents.FirstOrDefault(_ => _.UserCode == userCode);
                if (agentOnline is not null)
                    v.AgentId = agentOnline.Id;
                else Snackbar.Add("Agent invalide", Severity.Error);
                v.ClientId = _clientId;
                v.Products = new List<ProductQuantite>();
                foreach (var vp in _venteProduits)
                {
                    v.Products.Add(new ProductQuantite
                    {
                        ProductId = vp.ProductId,
                        Quantite = vp.Quantite,
                    });
                }

                await VentesClient.CreateAsync(v.Adapt<CreateVenteRequest>());
            },
            updateFunc: async (id, v) =>
            {
                var user = (await AuthState).User;
                string userIdString = user.GetUserId();
                Guid userCode = Guid.Parse(userIdString!);
                var agents = await AgentsClient.GetAllAsync();
                var agentOnline = agents.FirstOrDefault(_ => _.UserCode == userCode);
                if (agentOnline is not null)
                    v.AgentId = agentOnline.Id;
                else Snackbar.Add("Agent invalide", Severity.Error);
                v.ClientId = _clientId;
                v.Products.Clear();
                foreach (var vp in _venteProduits)
                {
                    v.Products.Add(new ProductQuantite
                    {
                        ProductId = vp.ProductId,
                        Quantite = vp.Quantite,
                    });
                }

                await VentesClient.UpdateAsync(id, v);
            },
            exportAction: string.Empty,
            deleteFunc: async id => await VentesClient.DeleteAsync(id),
            hasExtraActionsFunc: () => true
        );
        await SearchProductToSell();
        await SearchClientToSell();
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

    private async Task SearchClientToSell()
    {
        _isLoading = true;
        var clientsResponse = await ClientsClient.GetAllAsync();
        if (clientsResponse.Count > 0)
        {
            if (string.IsNullOrEmpty(_searchString))
            {
                _clients = clientsResponse.ToList().Take(10).ToList();
            }
            else
            {
                _clients = clientsResponse.ToList()
                    .Where(c => c.Nom.Contains(_searchString, StringComparison.InvariantCultureIgnoreCase))
                    .ToList();
            }
        }
    }

    private async Task<IEnumerable<Guid>> SearchProduct(string value)
    {
        return string.IsNullOrEmpty(value)
            ? _products.Select(_ => _.Id)
            : _products.Where(_ => _.Name.Contains(value, StringComparison.InvariantCultureIgnoreCase))
                .Select(_ => _.Id)
                .ToList();
    }

    private async Task<IEnumerable<Guid?>> SearchClient(string value)
    {
        return string.IsNullOrEmpty(value)
            ? _clients.Select(_ => (Guid?)_.Id)
            : _clients.Where(_ => _.Nom.Contains(value, StringComparison.InvariantCultureIgnoreCase))
                .Select(_ => (Guid?)_.Id)
                .ToList();
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

    private async void AddProductToTable(Guid productId, int quantite)
    {
        try
        {
            if (_productId != Guid.Empty && _quantite != 0)
            {
                _productQuantites.Add(new ProductQuantite
                {
                    ProductId = productId,
                    Quantite = quantite
                });

                foreach (var item in _productQuantites)
                {
                    var p = await ProductsClient.GetAsync(item.ProductId);
                    _venteProduits.Add(new VenteProduitDto
                    {
                        Prix = p.Prix,
                        Quantite = item.Quantite,
                        ProductId = item.ProductId,
                        Product = p.Adapt<ProductDto>()
                    });
                    await InvokeAsync(StateHasChanged);
                }

                _productQuantites.Clear();
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add(ex.Message, Severity.Error);
        }
    }

    private async void RemoveVenteProduitInTheTable(int produit)
    {
        _venteProduits.RemoveAt(produit);
        await InvokeAsync(StateHasChanged);
    }

    private async void GenererFactureVente(Guid id)
    {
        if (id != Guid.Empty)
        {
            var response = await PdfsClient.PrintFactureVenteAsync(id);
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