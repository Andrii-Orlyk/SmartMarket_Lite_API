namespace SmartMarket.Application.DTOs.Auth;

public sealed record RegisterRequest(
    string Email,
    string Password,
    string? FirstName,
    string? LastName);
