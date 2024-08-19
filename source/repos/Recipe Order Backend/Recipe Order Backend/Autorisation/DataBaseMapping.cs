using AutoMapper;
using RecipeOrder.Models;
using RecipeOrder.Data;

namespace Autorisation
{
    public class DataBaseMappings : Profile
    {
        public DataBaseMappings()
        {
            CreateMap<User, User>().ReverseMap();
        }
    }
}
