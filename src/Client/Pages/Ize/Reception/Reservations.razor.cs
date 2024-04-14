using FSH.WebApi.Shared.Authorization;
using hotel_ize_frontend.Client.Components.EntityTable;
using hotel_ize_frontend.Client.Infrastructure.ApiClient;
using Mapster;
using Microsoft.AspNetCore.Components;

namespace hotel_ize_frontend.Client.Pages.Ize.Reception;
public partial class Reservations
{
    [Inject]
    protected IReservationsClient ReservationsClient { get; set; } = default!;
    [Inject]
    protected IChambresClient ChambresClient { get; set; } = default!;

    protected EntityServerTableContext<ReservationDto, Guid, UpdateReservationRequest> Context { get; set; } = default!;

    private EntityTable<ReservationDto, Guid, UpdateReservationRequest> _table = default!;
    private List<ReservationDto> _reservations { get; set; } = default!;
    private List<ChambreDetailsDto> _chambres { get; set; } = new();
    private bool _isLoading { get; set; } = false;

    protected override void OnInitialized()
    {
        Context = new(
            entityName: L["Reservation"],
            entityNamePlural: L["Reservations"],
            entityResource: FSHResource.Reservations,
            fields: new()
            {
                new(r => r.Nom, L["Nom"], "Nom"),
                new(r => r.Prenom, L["Prenoms"], "Prenom"),
                new(r => r.DateArrive, L["Date d'arriv�e"], "DateArrive"),
                new(r => r.ChambreNom, L["Chambre"], "Chambre"),
                new(r => r.StatutReservation, L["Statut"], "Statut"),
                new(r => r.TypeReservationLibelle, L["Type Reservation"], "Type Reservation"),
            },
            idFunc: r => r.Id,
            searchFunc: async filter =>
            {
                var reservationFilter = filter.Adapt<SearchReservationsRequest>();

                reservationFilter.Nom = SearchNom;
                reservationFilter.Prenom = SearchPrenom;
                reservationFilter.ChambreId = SearchChambreId == default ? null : SearchChambreId;

                var result = await ReservationsClient.SearchAsync(reservationFilter);
                return result.Adapt<PaginationResponse<ReservationDto>>();
            },
            createFunc: async r =>
            {
                await ReservationsClient.CreateAsync(r.Adapt<CreateReservationRequest>());
            },
            updateFunc: async (id, r) =>
            {
                await ReservationsClient.UpdateAsync(id, r);
            },
            deleteFunc: async id => await ReservationsClient.DeleteAsync(id)
        );
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

    private string? _searchNom;
    private string? SearchNom
    {
        get => _searchNom;
        set
        {
            _searchNom = value;
            _ = _table.ReloadDataAsync();
        }
    }

    private string? _searchPrenom;
    private string? SearchPrenom
    {
        get => _searchPrenom;
        set
        {
            _searchPrenom = value;
            _ = _table.ReloadDataAsync();
        }
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
}