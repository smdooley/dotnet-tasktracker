using TaskTrackerApi.Models;

namespace TaskTrackerApi.Services
{
    public interface IJwtService
    {
        string GenerateToken(User user);
    }
}