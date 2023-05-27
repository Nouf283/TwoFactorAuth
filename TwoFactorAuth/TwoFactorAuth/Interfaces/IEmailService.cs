using System.Threading.Tasks;

namespace TwoFactorAuth.Interfaces
{
    public interface IEmailService
    {
        Task SendAsync(string from, string to, string subject, string body);
    }
}