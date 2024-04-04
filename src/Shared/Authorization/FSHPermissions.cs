using System.Collections.ObjectModel;

namespace FSH.WebApi.Shared.Authorization;
public static class FSHAction
{
    public const string View = nameof(View);
    public const string Search = nameof(Search);
    public const string Create = nameof(Create);
    public const string Update = nameof(Update);
    public const string Delete = nameof(Delete);
    public const string Export = nameof(Export);
    public const string Generate = nameof(Generate);
    public const string Clean = nameof(Clean);
    public const string UpgradeSubscription = nameof(UpgradeSubscription);
}

public static class FSHResource
{
    public const string Tenants = nameof(Tenants);
    public const string Dashboard = nameof(Dashboard);
    public const string Hangfire = nameof(Hangfire);
    public const string Users = nameof(Users);
    public const string UserRoles = nameof(UserRoles);
    public const string Roles = nameof(Roles);
    public const string RoleClaims = nameof(RoleClaims);
    public const string Products = nameof(Products);
    public const string Brands = nameof(Brands);
    public const string TypeChambres = nameof(TypeChambres);
    public const string Agents = nameof(Agents);
    public const string Chambres = nameof(Chambres);
    public const string Clients = nameof(Clients);
}

public static class FSHPermissions
{
    private static readonly FSHPermission[] _all = new FSHPermission[]
    {
        new("View Dashboard", FSHAction.View, FSHResource.Dashboard),
        new("View Hangfire", FSHAction.View, FSHResource.Hangfire),

        new("View Users", FSHAction.View, FSHResource.Users),
        new("Search Users", FSHAction.Search, FSHResource.Users),
        new("Create Users", FSHAction.Create, FSHResource.Users),
        new("Update Users", FSHAction.Update, FSHResource.Users),
        new("Delete Users", FSHAction.Delete, FSHResource.Users),
        new("Export Users", FSHAction.Export, FSHResource.Users),

        new("View UserRoles", FSHAction.View, FSHResource.UserRoles),
        new("Update UserRoles", FSHAction.Update, FSHResource.UserRoles),

        new("View Roles", FSHAction.View, FSHResource.Roles),
        new("Create Roles", FSHAction.Create, FSHResource.Roles),
        new("Update Roles", FSHAction.Update, FSHResource.Roles),
        new("Delete Roles", FSHAction.Delete, FSHResource.Roles),

        new("View RoleClaims", FSHAction.View, FSHResource.RoleClaims),
        new("Update RoleClaims", FSHAction.Update, FSHResource.RoleClaims),

        new("View Products", FSHAction.View, FSHResource.Products, IsBasic: true),
        new("Search Products", FSHAction.Search, FSHResource.Products, IsBasic: true),
        new("Create Products", FSHAction.Create, FSHResource.Products),
        new("Update Products", FSHAction.Update, FSHResource.Products),
        new("Delete Products", FSHAction.Delete, FSHResource.Products),
        new("Export Products", FSHAction.Export, FSHResource.Products),

        new("View Brands", FSHAction.View, FSHResource.Brands, IsBasic: true),
        new("Search Brands", FSHAction.Search, FSHResource.Brands, IsBasic: true),
        new("Create Brands", FSHAction.Create, FSHResource.Brands),
        new("Update Brands", FSHAction.Update, FSHResource.Brands),
        new("Delete Brands", FSHAction.Delete, FSHResource.Brands),
        new("Generate Brands", FSHAction.Generate, FSHResource.Brands),
        new("Clean Brands", FSHAction.Clean, FSHResource.Brands),

        new("Create TypeChambres", FSHAction.Create, FSHResource.TypeChambres),
        new("Search TypeChambres", FSHAction.Search, FSHResource.TypeChambres),
        new("Update TypeChambres", FSHAction.Update, FSHResource.TypeChambres),
        new("Delete TypeChambres", FSHAction.Delete, FSHResource.TypeChambres),
        new("View TypeChambres", FSHAction.View, FSHResource.TypeChambres),

        new("View Chambres", FSHAction.View, FSHResource.Chambres),
        new("Create Chambres", FSHAction.Create, FSHResource.Chambres),
        new("Search Chambres", FSHAction.Search, FSHResource.Chambres),
        new("Update Chambres", FSHAction.Update, FSHResource.Chambres),
        new("Delete Chambres", FSHAction.Delete, FSHResource.Chambres),
        new("Export Chambres", FSHAction.Export, FSHResource.Chambres),

        new("View Client", FSHAction.View, FSHResource.Clients),
        new("Create Client", FSHAction.Create, FSHResource.Clients),
        new("Search Client", FSHAction.Search, FSHResource.Clients),
        new("Update Client", FSHAction.Update, FSHResource.Clients),
        new("Delete Client", FSHAction.Delete, FSHResource.Clients),
        new("Export Client", FSHAction.Export, FSHResource.Clients),

        new("Create Agents", FSHAction.Create, FSHResource.Agents),
        new("Search Agents", FSHAction.Search, FSHResource.Agents),
        new("Update Agents", FSHAction.Update, FSHResource.Agents),
        new("Delete Agents", FSHAction.Delete, FSHResource.Agents),
        new("View Agents", FSHAction.View, FSHResource.Agents),

        new("View Tenants", FSHAction.View, FSHResource.Tenants, IsRoot: true),
        new("Create Tenants", FSHAction.Create, FSHResource.Tenants, IsRoot: true),
        new("Update Tenants", FSHAction.Update, FSHResource.Tenants, IsRoot: true),
        new("Upgrade Tenant Subscription", FSHAction.UpgradeSubscription, FSHResource.Tenants, IsRoot: true)
    };

    public static IReadOnlyList<FSHPermission> All { get; } = new ReadOnlyCollection<FSHPermission>(_all);
    public static IReadOnlyList<FSHPermission> Root { get; } = new ReadOnlyCollection<FSHPermission>(_all.Where(p => p.IsRoot).ToArray());
    public static IReadOnlyList<FSHPermission> Admin { get; } = new ReadOnlyCollection<FSHPermission>(_all.Where(p => !p.IsRoot).ToArray());
    public static IReadOnlyList<FSHPermission> Basic { get; } = new ReadOnlyCollection<FSHPermission>(_all.Where(p => p.IsBasic).ToArray());
}

public record FSHPermission(string Description, string Action, string Resource, bool IsBasic = false, bool IsRoot = false)
{
    public string Name => NameFor(Action, Resource);
    public static string NameFor(string action, string resource) => $"Permissions.{resource}.{action}";
}