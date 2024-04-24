
using Neo4j.Driver;
using RecipesGlossary.Business.Services;
using RecipesGlossary.DataAccess.Abstractions;
using RecipesGlossary.DataAccess.Repositories;
namespace RecipesGlossary
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            var neoConfig = builder.Configuration.GetSection("Neo4j");
            builder.Services.AddSingleton(s => GraphDatabase.Driver(neoConfig["Uri"], AuthTokens.Basic(neoConfig["Username"], neoConfig["Password"])));

            builder.Services.AddSingleton<IRecipeRepository, RecipeRepository>();

            builder.Services.AddTransient<RecipeService>();

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Add CORS services
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("MyCorsPolicy", policy =>
                {
                    policy.WithOrigins("http://localhost:5173")
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseCors("MyCorsPolicy");

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
