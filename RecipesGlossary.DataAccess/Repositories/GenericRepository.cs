using Neo4j.Driver;
using RecipesGlossary.DataAccess.Abstractions;
using RecipesGlossary.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipesGlossary.DataAccess.Repositories
{
    public abstract class GenericRepository<T> : IGenericRepository<T> where T : GenericModel
    {
        protected readonly IDriver _driver;

        public GenericRepository(IDriver driver)
        {
            _driver = driver;
        }

        public abstract Task<T> GetByIdAsync(string id);
    }
}
