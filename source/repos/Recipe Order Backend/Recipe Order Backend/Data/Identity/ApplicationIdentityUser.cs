using Microsoft.AspNetCore.Identity;

namespace RecipeOrder.Data.Identity
{
    public class ApplicationIdentityUser : IdentityUser
    {
        public long ApplicationId { get; set; }
    }
}
