using SmartMarket.Domain.Entities;

namespace SmartMarket.Application.Interfaces.Security;

public interface IJwtTokenService
{
    string GenerateToken(User user);
}
