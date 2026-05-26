namespace SmartMarket.Application.Options;

public sealed class SeedAdminSettings
{
    public const string SectionName = "SeedAdmin";

    public string Email { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;

    public string? FirstName { get; init; }

    public string? LastName { get; init; }
}
