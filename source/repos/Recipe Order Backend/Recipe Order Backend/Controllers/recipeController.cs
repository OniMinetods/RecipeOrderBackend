using Autorisation.Services;
using Microsoft.AspNetCore.Mvc;
using RecipeOrder.Data;
using Autorisation.Interfaces;
using RecipeOrder.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;
using Azure.Core;

namespace RecipeOrder.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecipeController : ControllerBase
    {
        private readonly UsersService _usersService;
        private readonly IRecipeRepository _recipeRepository;
        private readonly ApplicationDbContext _context;
        private readonly ApplicationUserDbContext _userContext;

        public RecipeController(IRecipeRepository recipeRepository, ApplicationDbContext context, UsersService usersService, ApplicationUserDbContext userContext)
        {
            _recipeRepository = recipeRepository;
            _context = context;
            _usersService = usersService;
            _userContext = userContext;
        }

        [HttpGet("search")]
        public async Task<IActionResult> GetRecipesBySearch(string? searchString)
        {
            var recipes = await _recipeRepository.GetAllRecipesAsync(searchString);
            return Ok(recipes);
        }

        [HttpGet]
        public async Task<ActionResult<List<Recipe>>> GetRecipes()
        {
            var recipes = await _context.Recipes.ToListAsync();
            var result = recipes.Select(i => new
            {
                i.RecipeID,
                i.Title,
                i.Ingredients,
                i.Tags,
                i.Rating,
                i.Reviews,
                i.Author,   
            }).ToList();
            return Ok(result);
        }
        [Authorize]
        [HttpPost("createRecipe")]
        public async Task<IActionResult> CreateRecipe([FromBody] CreateRecipe createRecipe)
        {
            if (createRecipe == null)
            {
                return BadRequest("Данные поля не могут быть пустыми");
            }

            if (string.IsNullOrEmpty(createRecipe.Title) ||
                string.IsNullOrEmpty(createRecipe.Ingredients) ||
                string.IsNullOrEmpty(createRecipe.Tags))
            {
                return BadRequest("Все поля должны быть заполнены");
            }
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized("Пользователь не авторизован.");
            }

            if (!Guid.TryParse(userId, out var userGuid))
            {
                return Unauthorized("Неверный идентификатор пользователя.");
            }

            var recipe = new Recipe
            {
                Title = createRecipe.Title,
                Ingredients = createRecipe.Ingredients,
                Tags = createRecipe.Tags,
                Description = createRecipe.Description,
                Author = userGuid.ToString(),
            };

            _context.Recipes.Add(recipe);
            await _context.SaveChangesAsync();

            var result = new
            {
                Title = recipe.Title,
                Ingredients = recipe.Ingredients,
                Tags = recipe.Tags,
                Description = recipe.Description,
            };

            return CreatedAtAction(nameof(GetRecipes), new { id = recipe.RecipeID }, result);
        }

        [Authorize]
        [HttpGet("myOwnRecipes")]
        public async Task<ActionResult<List<Recipe>>> GetMyOwnRecipes()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            var userId = Guid.Parse(userIdClaim.Value);

            var userRecipes = await _context.Recipes
                                            .Where(u => u.Author == userId.ToString())
                                            .Select(u => new MyOwnRecipe
                                            {
                                                Title = u.Title,
                                                Ingredients = u.Ingredients,
                                                Tags = u.Tags,
                                                Rating = u.Rating,
                                                Reviews = u.Reviews,
                                                Description = u.Description,
                                            })
                                            .ToListAsync();

            return Ok(userRecipes);
        }

        [Authorize]
        [HttpGet("favorites")]
        public async Task<ActionResult<List<Recipe>>> GetUserFavourites()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            var userId = Guid.Parse(userIdClaim.Value);

            var userFavourites = await _userContext.Users
                                               .Where(u => u.UserID == userId)
                                               .Select(u => u.RecipeFavorites)
                                               .FirstOrDefaultAsync();

            if (userFavourites == null)
            {
                return NotFound("Избранные пользователя не найдены");
            }

            var recipeIds = userFavourites
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(id => int.Parse(id.Trim()))
                .ToList();

            var recipes = await _context.Recipes
                                         .Where(i => recipeIds.Contains(i.RecipeID))
                                         .ToListAsync();

            if (recipes == null || !recipes.Any())
            {
                return NotFound("Не найдены кроссовки с таким Id");
            }

            return Ok(recipes);
        }

        [HttpGet("reviews")]
        public async Task<ActionResult<List<RecipeReviews>>> GetRatingRecipes()
        {
            var reviews = await _context.Recipes
                                        .Select(item => item.Reviews)
                                        .ToListAsync();
            return Ok(reviews);
        }

        [HttpGet("reviews/{recipeId}")]
        public async Task<ActionResult<List<Recipe>>> GetRatingRecipesById(int recipeId)
        {
            var reviews = await _context.Recipes
                               .Where(r => r.RecipeID == recipeId)
                               .Select(r => r.Reviews)
                               .FirstOrDefaultAsync();

            if (reviews == null || reviews.Count == 0)
            {
                return NotFound("Отзывы для данного рецепта не найдены.");
            }

            return Ok(reviews);
        }

        [Authorize]
        [HttpPost("AddReview/{recipeId}")]
        public async Task<IActionResult> AddReview(int recipeId, [FromBody] RecipeReviews request)
        {
            var userNameClaim = User.FindFirst(ClaimTypes.Name);
            var userName = userNameClaim?.Value;

            var recipe = await _context.Recipes
                                       .Where(r => r.RecipeID == recipeId)
                                       .FirstOrDefaultAsync();

            if (recipe == null)
            {
                return NotFound("Рецепт не найден");
            }

            var review = new RecipeReviews
            {
                Id = recipe.Reviews.Any() ? recipe.Reviews.Max(r => r.Id) + 1 : 1,
                Reviewer = request.Reviewer, 
                Estimation = request.Estimation,
                Text = request.Text
            };

            if (recipe.Reviews == null)
            {
                recipe.Reviews = new List<RecipeReviews>();
            }

            recipe.Reviews.Add(review);

            recipe.Rating = recipe.Reviews.Average(r => r.Estimation);

            _context.Recipes.Update(recipe);
            await _context.SaveChangesAsync();

            return Ok("Отзыв успешно добавлен");
        }


        [Authorize]
        [HttpPost("add-to-favorites")]
        public async Task<ActionResult> AddToUserFavourites([FromBody] int recipeId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            var userId = Guid.Parse(userIdClaim.Value);

            var user = await _userContext.Users
                                         .Where(u => u.UserID == userId)
                                         .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound("Пользователь не найден");
            }

            var recipeExists = await _context.Recipes.AnyAsync(r => r.RecipeID == recipeId);
            if (!recipeExists)
            {
                return NotFound("Рецепт не найден");
            }

            var userFavourites = user.RecipeFavorites ?? string.Empty;
            var favouriteIds = userFavourites
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(id => int.Parse(id.Trim()))
                .ToList();

            if (!favouriteIds.Contains(recipeId))
            {
                favouriteIds.Add(recipeId);
                user.RecipeFavorites = string.Join(",", favouriteIds);
                await _userContext.SaveChangesAsync();
                return Ok("Товар добавлен в избранные");
            }
            else
            {
                return BadRequest("Товар уже находится в избранных");
            }
        }

        [Authorize]
        [HttpDelete("remove-from-favorites/{recipeId}")]
        public async Task<ActionResult> RemoveFromUserFavourites(int recipeId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            var userId = Guid.Parse(userIdClaim.Value);

            var user = await _userContext.Users
                                         .Where(u => u.UserID == userId)
                                         .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound("Пользователь не найден");
            }

            var userFavourites = user.RecipeFavorites ?? string.Empty;
            var favouriteIds = userFavourites
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(id => int.Parse(id.Trim()))
                .ToList();

            if (favouriteIds.Contains(recipeId))
            {
                favouriteIds.Remove(recipeId);
                user.RecipeFavorites = string.Join(",", favouriteIds);
                await _userContext.SaveChangesAsync();
                return Ok("Товар удален из избранных");
            }
            else
            {
                return NotFound("Товар не найден в избранных");
            }
        }

    }
}
