using RecipeOrder.Data;
using RecipeOrder.Models;
using Microsoft.EntityFrameworkCore;

public class RecipeRepository : IRecipeRepository
{
    private readonly ApplicationDbContext _context;

    public RecipeRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Recipe>> GetAllRecipesAsync(string searchString)
    {
        var recipes = from i in _context.Recipes select i;

        if (!string.IsNullOrEmpty(searchString))
        {
            recipes = recipes.Where(i => i.Title.Contains(searchString));
        }

        return await recipes.ToListAsync();
    }
}