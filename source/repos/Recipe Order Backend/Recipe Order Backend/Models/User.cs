namespace RecipeOrder.Models
{
    public class User
    {
        private User(Guid id, string userName, string passwordHash, string email, string recipeFavorites, string sex)
        {
            UserID = id;
            Username = userName;
            Password = passwordHash;
            Email = email;
            RecipeFavorites = recipeFavorites;
            Sex = sex;
        }
        public Guid UserID { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string RecipeFavorites { get; set; } = string.Empty;
        public string Sex { get; set; } = string.Empty;

        public int[] GetFavoritesAsArray()
        {
            return RecipeFavorites
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .ToArray();
        }
        
        public void SetFavoritesFromArray(int[] favoritesArray)
        {
            RecipeFavorites = string.Join(",", favoritesArray);
        }

        public User()
        {

        }

        public static User Create(Guid id, string userName, string passwordHash, string email, string recipeFavorites, string sex)
        {
            return new User(id, userName, passwordHash, email, recipeFavorites, sex);
        }
    }

    public class ChangePasswordRequest
    {
        public string OldPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
