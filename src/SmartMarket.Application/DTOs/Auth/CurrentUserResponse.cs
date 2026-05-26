using SmartMarket.Domain.Enums;

namespace SmartMarket.Application.DTOs.Auth;

public sealed record CurrentUserResponse(
    Guid Id,
    string Email,
    UserRole Role,
    string? FirstName,
    string? LastName);
