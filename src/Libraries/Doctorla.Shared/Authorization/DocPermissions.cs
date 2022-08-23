using System.Collections.ObjectModel;

namespace Doctorla.Shared.Authorization;

public static class DocAction
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

public static class DocResource
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
        new("View Dashboard", DocAction.View, DocResource.Dashboard),
        new("View Hangfire", DocAction.View, DocResource.Hangfire),
        new("View Users", DocAction.View, DocResource.Users),
        new("Search Users", DocAction.Search, DocResource.Users),
        new("Create Users", DocAction.Create, DocResource.Users),
        new("Update Users", DocAction.Update, DocResource.Users),
        new("Delete Users", DocAction.Delete, DocResource.Users),
        new("Export Users", DocAction.Export, DocResource.Users),
        new("View UserRoles", DocAction.View, DocResource.UserRoles),
        new("Update UserRoles", DocAction.Update, DocResource.UserRoles),
        new("View Roles", DocAction.View, DocResource.Roles),
        new("Create Roles", DocAction.Create, DocResource.Roles),
        new("Update Roles", DocAction.Update, DocResource.Roles),
        new("Delete Roles", DocAction.Delete, DocResource.Roles),
        new("View RoleClaims", DocAction.View, DocResource.RoleClaims),
        new("Update RoleClaims", DocAction.Update, DocResource.RoleClaims),
        new("View Products", DocAction.View, DocResource.Products, IsBasic: true),
        new("Search Products", DocAction.Search, DocResource.Products, IsBasic: true),
        new("Create Products", DocAction.Create, DocResource.Products),
        new("Update Products", DocAction.Update, DocResource.Products),
        new("Delete Products", DocAction.Delete, DocResource.Products),
        new("Export Products", DocAction.Export, DocResource.Products),
        new("View Brands", DocAction.View, DocResource.Brands, IsBasic: true),
        new("Search Brands", DocAction.Search, DocResource.Brands, IsBasic: true),
        new("Create Brands", DocAction.Create, DocResource.Brands),
        new("Update Brands", DocAction.Update, DocResource.Brands),
        new("Delete Brands", DocAction.Delete, DocResource.Brands),
        new("Generate Brands", DocAction.Generate, DocResource.Brands),
        new("Clean Brands", DocAction.Clean, DocResource.Brands),
        new("View Tenants", DocAction.View, DocResource.Tenants, IsRoot: true),
        new("Create Tenants", DocAction.Create, DocResource.Tenants, IsRoot: true),
        new("Update Tenants", DocAction.Update, DocResource.Tenants, IsRoot: true),
        new("Upgrade Tenant Subscription", DocAction.UpgradeSubscription, DocResource.Tenants, IsRoot: true)
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
