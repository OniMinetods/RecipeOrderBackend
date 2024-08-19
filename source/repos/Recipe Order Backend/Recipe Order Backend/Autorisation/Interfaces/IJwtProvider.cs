using RecipeOrder.Models;

namespace Autorisation.Interfaces
{
    public interface IJwtProvider
    {
        string GenerateToken(User user);
    }
}
