using System.Collections.ObjectModel;

namespace Doctorla.Shared.Authorization;

public static class DocActions
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

public static class DocResources
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
}

public static class DocPermissions
{
    private static readonly DocPermission[] _all = new DocPermission[]
    {
        new("View Dashboard", DocActions.View, DocResources.Dashboard),
        new("View Hangfire", DocActions.View, DocResources.Hangfire),
        new("View Users", DocActions.View, DocResources.Users),
        new("Search Users", DocActions.Search, DocResources.Users),
        new("Create Users", DocActions.Create, DocResources.Users),
        new("Update Users", DocActions.Update, DocResources.Users),
        new("Delete Users", DocActions.Delete, DocResources.Users),
        new("Export Users", DocActions.Export, DocResources.Users),
        new("View UserRoles", DocActions.View, DocResources.UserRoles),
        new("Update UserRoles", DocActions.Update, DocResources.UserRoles),
        new("View Roles", DocActions.View, DocResources.Roles),
        new("Create Roles", DocActions.Create, DocResources.Roles),
        new("Update Roles", DocActions.Update, DocResources.Roles),
        new("Delete Roles", DocActions.Delete, DocResources.Roles),
        new("View RoleClaims", DocActions.View, DocResources.RoleClaims),
        new("Update RoleClaims", DocActions.Update, DocResources.RoleClaims),
        new("View Products", DocActions.View, DocResources.Products, IsBasic: true),
        new("Search Products", DocActions.Search, DocResources.Products, IsBasic: true),
        new("Create Products", DocActions.Create, DocResources.Products),
        new("Update Products", DocActions.Update, DocResources.Products),
        new("Delete Products", DocActions.Delete, DocResources.Products),
        new("Export Products", DocActions.Export, DocResources.Products),
        new("View Brands", DocActions.View, DocResources.Brands, IsBasic: true),
        new("Search Brands", DocActions.Search, DocResources.Brands, IsBasic: true),
        new("Create Brands", DocActions.Create, DocResources.Brands),
        new("Update Brands", DocActions.Update, DocResources.Brands),
        new("Delete Brands", DocActions.Delete, DocResources.Brands),
        new("Generate Brands", DocActions.Generate, DocResources.Brands),
        new("Clean Brands", DocActions.Clean, DocResources.Brands),
        new("View Tenants", DocActions.View, DocResources.Tenants, IsRoot: true),
        new("Create Tenants", DocActions.Create, DocResources.Tenants, IsRoot: true),
        new("Update Tenants", DocActions.Update, DocResources.Tenants, IsRoot: true),
        new("Upgrade Tenant Subscription", DocActions.UpgradeSubscription, DocResources.Tenants, IsRoot: true)
    };

    public static IReadOnlyList<DocPermission> All { get; } = new ReadOnlyCollection<DocPermission>(_all);

    public static IReadOnlyList<DocPermission> Root { get; } = new ReadOnlyCollection<DocPermission>(_all.Where(p => p.IsRoot).ToArray());
    
    public static IReadOnlyList<DocPermission> Admin { get; } = new ReadOnlyCollection<DocPermission>(_all.Where(p => !p.IsRoot).ToArray());
    
    public static IReadOnlyList<DocPermission> Basic { get; } = new ReadOnlyCollection<DocPermission>(_all.Where(p => p.IsBasic).ToArray());
}

public record DocPermission(string Description, string Action, string Resource, bool IsBasic = false, bool IsRoot = false)
{
    public string Name => NameFor(Action, Resource);
    public static string NameFor(string action, string resource) => $"Permissions.{resource}.{action}";
}
