using TwoFactorAuth.Entities;

namespace TwoFactorAuth.Interfaces
{
    public interface ITokenServices
    {
        string CreateToken(AppUser appUser);
    }
}
