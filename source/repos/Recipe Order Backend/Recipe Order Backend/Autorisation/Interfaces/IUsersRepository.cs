using System.Threading.Tasks;
using RecipeOrder.Models;

namespace Autorisation.Interfaces
{
    public interface IUsersRepository
    {
        Task Add(User user);
        Task<User> GetByEmail(string email);
        Task<bool> EmailExists(string email);
        Task<User?> GetById(Guid id);
    }
}
