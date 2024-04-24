using RecipesGlossary.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipesGlossary.DataAccess.Abstractions
{
    public interface IGenericRepository<T> where T : GenericModel
    {
        Task<T> GetByIdAsync(string id);
    }
}
