using FSH.WebApi.Shared.Authorization;
using hotel_ize_frontend.Client.Components.EntityTable;
using hotel_ize_frontend.Client.Infrastructure.ApiClient;
using hotel_ize_frontend.Client.Infrastructure.Common;
using Mapster;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;

namespace hotel_ize_frontend.Client.Pages.Ize.Configurations.Chambres;
public partial class Chambres
{
    [Inject]
    protected IChambresClient ChambresClient { get; set; } = default!;
    [Inject]
    protected ITypeChambresClient TypeChambresClient { get; set; } = default!;

    protected EntityServerTableContext<ChambreDto, Guid, ChambreViewModel> Context { get; set; } = default!;

    private EntityTable<ChambreDto, Guid, ChambreViewModel> _table = default!;
    private List<TypeChambreDto> _typeChambres { get; set; } = default!;

    private bool _isLoading { get; set; } = false;

    protected override void OnInitialized() =>
        Context = new(
            entityName: L["Chambre"],
            entityNamePlural: L["Chambres"],
            entityResource: FSHResource.Chambres,
            fields: new()
            {
                new(c => c.Nom, L["Nom"], "Nom"),
                new(c => c.Capacite, L["Capacite"], "Capacite"),
                new(c => c.Prix, L["Prix"], "Prix"),
                new(c => c.TypeChambreNom, L["Type Chambre"], "TypeChambre.Libelle"),
                new(c => c.Climatisee, L["Climatisee ?"], "Climatisee"),
                new(c => c.PetitDejeunerInclus, L["Petit Dejeuné inclus ?"], "PetitDejeunerInclus"),
                new(c => c.Disponible, L["Disponible?"], "Disponible")
            },
            enableAdvancedSearch: true,
            idFunc: c => c.Id,
            searchFunc: async filter =>
            {
                var chambreFilter = filter.Adapt<SearchChambresRequest>();

                chambreFilter.TypeChambreId = SearchTypeChambreId == default ? null : SearchTypeChambreId;
                chambreFilter.Prix = SearchChambrePrix;
                chambreFilter.Capacite = SearchChambreCapacite;

                var result = await ChambresClient.SearchAsync(chambreFilter);
                return result.Adapt<PaginationResponse<ChambreDto>>();
            },
            createFunc: async c =>
            {
                if (!string.IsNullOrEmpty(c.ImageInBytes))
                {
                    c.Image = new FileUploadRequest()
                    {
                        Data = c.ImageInBytes,
                        Extension = c.ImageExtension ?? string.Empty,
                        Name = $"{c.Nom}_{Guid.NewGuid():N}",
                    };
                }

                await ChambresClient.CreateAsync(c.Adapt<CreateChambreRequest>());
                c.ImageInBytes = string.Empty;
            },
            updateFunc: async (id, c) =>
            {
                if (!string.IsNullOrEmpty(c.ImageInBytes))
                {
                    c.DeleteCurrentImage = true;
                    c.Image = new FileUploadRequest()
                    {
                        Data = c.ImageInBytes,
                        Extension = c.ImageExtension ?? string.Empty,
                        Name = $"{c.Nom}_{Guid.NewGuid():N}",
                    };
                }

                await ChambresClient.UpdateAsync(id, c.Adapt<UpdateChambreRequest>());
                c.ImageInBytes = string.Empty;
            },
            exportFunc: async filter =>
            {
                var exportFilter = filter.Adapt<ExportChambresRequest>();
                exportFilter.TypeChambreId = SearchTypeChambreId == default ? null : SearchTypeChambreId;
                exportFilter.Capacite = SearchChambreCapacite;
                exportFilter.Prix = SearchChambrePrix;

                return await ChambresClient.ExportAsync(exportFilter);
            },
            deleteFunc: async id => await ChambresClient.DeleteAsync(id)
        );

    protected override async Task OnInitializedAsync()
    {
        await GetTypeChambres();
    }

    private Guid _searchTypeChambreId;
    private Guid SearchTypeChambreId
    {
        get => _searchTypeChambreId;
        set
        {
            _searchTypeChambreId = value;
            _ = _table.ReloadDataAsync();
        }
    }

    private decimal _searchChambrePrix;
    private decimal SearchChambrePrix
    {
        get => _searchChambrePrix;
        set
        {
            _searchChambrePrix = value;
            _ = _table.ReloadDataAsync();
        }
    }

    private int _searchChambreCapacite;
    private int SearchChambreCapacite
    {
        get => _searchChambreCapacite;
        set
        {
            _searchChambreCapacite = value;
            _ = _table.ReloadDataAsync();
        }
    }

    private async Task GetTypeChambres()
    {
        _isLoading = true;
        var response = await TypeChambresClient.GetAllAsync();
        if (response.Count > 0)
        {
            _typeChambres = response.ToList();
            _isLoading = false;
        }
    }

    private async Task<IEnumerable<Guid>> SearchTypeChambre(string value)
    {
        return string.IsNullOrEmpty(value)
            ? _typeChambres.Select(_ => _.Id)
            : _typeChambres.Where(_ => _.Libelle.Contains(value, StringComparison.InvariantCultureIgnoreCase))
                .Select(_ => _.Id)
                .ToList();
    }

    private async Task UploadFiles(InputFileChangeEventArgs e)
    {
        if (e.File != null)
        {
            string? extension = Path.GetExtension(e.File.Name);
            if (!ApplicationConstants.SupportedImageFormats.Contains(extension.ToLower()))
            {
                Snackbar.Add("Format d'image non supporté.", Severity.Error);
                return;
            }

            Context.AddEditModal.RequestModel.ImageExtension = extension;
            var imageFile = await e.File.RequestImageFileAsync(ApplicationConstants.StandardImageFormat, ApplicationConstants.MaxImageWidth, ApplicationConstants.MaxImageHeight);
            byte[]? buffer = new byte[imageFile.Size];
            await imageFile.OpenReadStream(ApplicationConstants.MaxAllowedSize).ReadAsync(buffer);
            Context.AddEditModal.RequestModel.ImageInBytes = $"data:{ApplicationConstants.StandardImageFormat};base64,{Convert.ToBase64String(buffer)}";
            Context.AddEditModal.ForceRender();
        }
    }

    public void ClearImageInBytes()
    {
        Context.AddEditModal.RequestModel.ImageInBytes = string.Empty;
        Context.AddEditModal.ForceRender();
    }

    public void SetDeleteCurrentImageFlag()
    {
        Context.AddEditModal.RequestModel.ImageInBytes = string.Empty;
        Context.AddEditModal.RequestModel.ImagePath = string.Empty;
        Context.AddEditModal.RequestModel.DeleteCurrentImage = true;
        Context.AddEditModal.ForceRender();
    }
}

public class ChambreViewModel : UpdateChambreRequest
{
    public string? ImagePath { get; set; }
    public string? ImageInBytes { get; set; }
    public string? ImageExtension { get; set; }
}