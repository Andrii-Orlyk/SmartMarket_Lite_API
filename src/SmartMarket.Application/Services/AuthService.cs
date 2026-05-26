using FluentValidation;
using SmartMarket.Application.Common;
using SmartMarket.Application.DTOs.Auth;
using SmartMarket.Application.Interfaces.Repositories;
using SmartMarket.Application.Interfaces.Security;
using SmartMarket.Application.Interfaces.Services;
using SmartMarket.Domain.Entities;
using SmartMarket.Domain.Enums;

namespace SmartMarket.Application.Services;

public sealed class AuthService(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService,
    IValidator<RegisterRequest> registerValidator,
    IValidator<LoginRequest> loginValidator) : IAuthService
{
    public async Task<Result<AuthResponse>> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await registerValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<AuthResponse>.Failure(
                ErrorCodes.ValidationFailed,
                "Validation failed.",
                validation.Errors.Select(x => x.ErrorMessage).ToList());
        }

        var email = NormalizeEmail(request.Email);

        if (await userRepository.EmailExistsAsync(email, cancellationToken))
        {
            return Result<AuthResponse>.Failure(
                ErrorCodes.AuthEmailExists,
                "Email is already registered.");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = passwordHasher.Hash(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Role = UserRole.User,
            CreatedAt = DateTime.UtcNow
        };

        await userRepository.AddAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<AuthResponse>.Success(new AuthResponse(jwtTokenService.GenerateToken(user)));
    }

    public async Task<Result<AuthResponse>> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await loginValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<AuthResponse>.Failure(
                ErrorCodes.ValidationFailed,
                "Validation failed.",
                validation.Errors.Select(x => x.ErrorMessage).ToList());
        }

        var email = NormalizeEmail(request.Email);
        var user = await userRepository.GetByEmailAsync(email, cancellationToken);

        if (user is null || !passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            return Result<AuthResponse>.Failure(
                ErrorCodes.AuthInvalidCredentials,
                "Invalid email or password.");
        }

        return Result<AuthResponse>.Success(new AuthResponse(jwtTokenService.GenerateToken(user)));
    }

    public async Task<Result<CurrentUserResponse>> GetCurrentUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return Result<CurrentUserResponse>.Failure(
                ErrorCodes.AuthUnauthorized,
                "User is not authenticated.");
        }

        return Result<CurrentUserResponse>.Success(new CurrentUserResponse(
            user.Id,
            user.Email,
            user.Role,
            user.FirstName,
            user.LastName));
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
