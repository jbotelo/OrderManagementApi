namespace Orm.Application.Auth;

public static class AuthorizationPolicies
{
    public const string OrdersRead = "Orders.Read";
    public const string OrdersCreate = "Orders.Create";
    public const string OrdersUpdate = "Orders.Update";
    public const string OrdersDelete = "Orders.Delete";
}

public static class AppRoles
{
    public const string Admin = "Admin";
    public const string Manager = "Manager";
    public const string User = "User";
}
