using RecipeOrder.Data;
using RecipeOrder.Models;

    public interface IRecipeRepository
    {
        Task<IEnumerable<Recipe>> GetAllRecipesAsync(string searchString);
    }
