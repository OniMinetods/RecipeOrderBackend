namespace RecipeOrder.Models
{
    public class Order
    {
        public int OrderID { get; set; }
        public Guid UserID { get; set; }
        public string RecipeID { get; set; } = string.Empty;
    }
}
