using FSH.WebApi.Shared.Authorization;
using hotel_ize_frontend.Client.Components.EntityTable;
using hotel_ize_frontend.Client.Infrastructure.ApiClient;
using Mapster;
using Microsoft.AspNetCore.Components;

namespace hotel_ize_frontend.Client.Pages.Ize.Configurations.Agents;
public partial class Agents
{
    [Inject]
    protected IAgentsClient AgentsClient { get; set; } = default!;

    [Inject]
    protected IUsersClient UsersClient { get; set; } = default!;

    protected EntityServerTableContext<AgentDto, Guid, UpdateAgentRequest> Context { get; set; } = default!;

    private List<UserDetailsDto> _users { get; set; } = new();
    private bool _isLoading { get; set; } = false;

    protected override void OnInitialized() =>
         Context = new(
            entityName: L["Agent"],
            entityNamePlural: L["Agents"],
            entityResource: FSHResource.Agents,
            fields: new()
            {
                 new(a => a.Nom, L["Nom"], "Nom"),
                 new(a => a.Prenoms, L["Prenoms"], "Prenoms"),
                 new(a => a.IsActive, L["Est actif ?"], "IsActive")
            },
            idFunc: a => a.Id,
            searchFunc: async filter => (await AgentsClient
                .SearchAsync(filter.Adapt<AgentBySearchRequest>()))
                .Adapt<PaginationResponse<AgentDto>>(),
            createFunc: async a =>
            {
                var selectedUser = _users.FirstOrDefault(_ => _.Id == a.UserCode);
                if (selectedUser is not null)
                {
                    a.UserCode = selectedUser.Id;
                    a.Nom = selectedUser.LastName;
                    a.Prenoms = selectedUser.FirstName;
                }

                await AgentsClient.CreateAsync(a.Adapt<CreateAgentRequest>());
            },
            updateFunc: async (id, a) => await AgentsClient.UpdateAsync(id, a),
            deleteFunc: async id => await AgentsClient.DeleteAsync(id),
            exportAction: string.Empty
        );

    protected override async Task OnInitializedAsync()
    {
        await GetUsers();
    }

    private async Task GetUsers()
    {
        _isLoading = true;
        var response = await UsersClient.GetListAsync();
        if (response.Count > 0)
        {
            _users = response.ToList();
            _isLoading = false;
        }
    }

    private async Task<IEnumerable<Guid>> SearchUsers(string value)
    {
        if (string.IsNullOrEmpty(value))
            return _users.Select(_ => _.Id);

        return _users.Where(_ => _.LastName.Contains(value, StringComparison.InvariantCultureIgnoreCase))
            .Select(_ => _.Id)
            .ToList();
    }

}