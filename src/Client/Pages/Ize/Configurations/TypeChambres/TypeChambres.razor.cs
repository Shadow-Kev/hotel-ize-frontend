using FSH.WebApi.Shared.Authorization;
using hotel_ize_frontend.Client.Components.EntityTable;
using hotel_ize_frontend.Client.Infrastructure.ApiClient;
using Mapster;
using Microsoft.AspNetCore.Components;

namespace hotel_ize_frontend.Client.Pages.Ize.Configurations.TypeChambres;
public partial class TypeChambres
{
    [Inject] private ITypeChambresClient TypeChambresClient { get; set; }

    protected EntityServerTableContext<TypeChambreDto, Guid, UpdateTypeChambreRequest> Context { get; set; } = default!;

    protected override void OnInitialized()
    {
        Context = new(
            entityName: L["TypeChambre"],
            entityNamePlural: L["TypeChambres"],
            entityResource: FSHResource.TypeChambres,
            fields: new()
            {
                new(tp => tp.Code, "Code"),
                new(tp => tp.Libelle, "Libelle"),
            },
            idFunc: tp => tp.Id,
            searchFunc: async filter => (await TypeChambresClient
                .SearchAsync(filter.Adapt<TypeChambresBySearchRequest>()))
                .Adapt<PaginationResponse<TypeChambreDto>>(),
            createFunc: async tp => await TypeChambresClient.CreateAsync(tp.Adapt<CreateTypeChambreRequest>()),
            updateFunc: async (id, tp ) => await TypeChambresClient.UpdateAsync(id, tp),
            deleteFunc: async id => await TypeChambresClient.DeleteAsync(id),
            exportAction: string.Empty
        );
    }
}