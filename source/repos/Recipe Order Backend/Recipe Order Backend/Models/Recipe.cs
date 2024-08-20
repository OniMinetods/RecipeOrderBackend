namespace RecipeOrder.Models
{
    public class Recipe
    {
        public int RecipeID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Ingredients { get; set; } = string.Empty;
        public string Tags { get; set; } = string.Empty;
        public double? Rating { get; set; }
        public List<RecipeReviews> Reviews { get; set; } = new List<RecipeReviews>();
        public string Author { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class MyOwnRecipe
    {
        public string Title { get; set; } = string.Empty;
        public string Ingredients { get; set; } = string.Empty;
        public string Tags { get; set; } = string.Empty;
        public double? Rating { get; set; }
        public List<RecipeReviews> Reviews { get; set; } = new List<RecipeReviews>();
        public string Description { get; set; } = string.Empty;
    }

    public class CreateRecipe
    {
        public string Title { get; set; } = string.Empty;
        public string Ingredients { get; set; } = string.Empty;
        public string Tags { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class RecipeReviews
    {
        public int Id { get; set; }
        public string Reviewer { get; set; } = string.Empty;
        public int Estimation { get; set; }
        public string Text { get; set; } = string.Empty;
    }
}
