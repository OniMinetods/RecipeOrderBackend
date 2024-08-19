using System.Collections.Generic;
using System.Threading.Tasks;
using RecipeOrder.Models;

namespace Autorisation.Interfaces
{
    public interface IRecipesRepository
    {
        Task<IEnumerable<Recipe>> GetRecipesByIds(int[] ids);
    }
}
