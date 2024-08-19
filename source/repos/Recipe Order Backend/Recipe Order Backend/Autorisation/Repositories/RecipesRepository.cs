using Autorisation.Interfaces;
using Microsoft.EntityFrameworkCore;
using RecipeOrder.Data;
using RecipeOrder.Models;

namespace Autorisation.Repositories
{
    public class RecipesRepository : IRecipesRepository
    {
        private readonly ApplicationDbContext _context;

        public RecipesRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Recipe>> GetRecipesByIds(int[] ids)
        {
            return await _context.Recipes.Where(s => ids.Contains(s.RecipeID)).ToListAsync();
        }
    }
}
