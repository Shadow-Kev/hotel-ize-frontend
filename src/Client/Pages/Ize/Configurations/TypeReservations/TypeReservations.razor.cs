using FSH.WebApi.Shared.Authorization;
using hotel_ize_frontend.Client.Components.EntityTable;
using hotel_ize_frontend.Client.Infrastructure.ApiClient;
using Mapster;
using Microsoft.AspNetCore.Components;

namespace hotel_ize_frontend.Client.Pages.Ize.Configurations.TypeReservations;
public partial class TypeReservations
{
    [Inject]
    protected ITypeReservationsClient TypeReservationsClient { get; set; } = default!;

    protected EntityServerTableContext<TypeReservationDto, Guid, UpdateTypeReservationRequest> Context { get; set; } = default!;

    protected override void OnInitialized()
    {
        Context = new(
            entityName: L["TypeReservation"],
            entityNamePlural: L["TypeReservations"],
            entityResource: FSHResource.TypeReservations,
            fields: new()
            {
                new(tr => tr.Libelle, "Libelle", "Libelle")
            },
            idFunc: tr => tr.Id,
            searchFunc: async filter =>
                (await TypeReservationsClient.SearchAsync(filter.Adapt<TypeReservationBySearchRequest>()))
                .Adapt<PaginationResponse<TypeReservationDto>>(),
            createFunc: async tr => await TypeReservationsClient.CreateAsync(tr.Adapt<CreateTypeReservationRequest>()),
            updateFunc: async (id, tr) => await TypeReservationsClient.UpdateAsync(id, tr),
            deleteFunc: async id => await TypeReservationsClient.DeleteAsync(id),
            exportAction: string.Empty
        );
    }
}