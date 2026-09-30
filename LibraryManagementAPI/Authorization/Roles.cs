namespace LibraryManagementAPI.Authorization;

/// <summary>
/// Role name constants used with [Authorize(Roles = ...)] attributes.
/// The actual users/roles are managed by the separate Identity project;
/// this API only needs the role names to match the claims in the JWT it receives.
/// </summary>
public static class Roles
{
    public const string Admin = "Admin";
    public const string Librarian = "Librarian";
    public const string Member = "Member";

    public static readonly string[] All = { Admin, Librarian, Member };
}
