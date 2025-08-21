using RareAPI.Models;
using RareAPI.Services;

namespace RareAPI.Endpoints
{
  public static class CategoryEndpoints
  {
    public static void MapCategoryEndpoints(this WebApplication app)
    {
      app.MapGet("/categories", async (DatabaseService db) =>
      {
        var users = await db.GetAllCategoriesAsync();
        return Results.Ok(users);
      });

      app.MapPost("/categories", async (Category category, DatabaseService db) =>
      {
        try
        {
          var newCategory = await db.CreateCategoryAsync(category);
          return Results.Created($"/categories/{category.Id}", newCategory);
        }
        catch (Exception ex)
        {
          return Results.Problem($"An error occurred while creating the category: {ex.Message}");
        }
      });

      app.MapDelete("/categories/{id}", async (int id, DatabaseService db) =>
      {
        try
        {
          bool deleted = await db.DeleteCategoryAsync(id);

          if (deleted)
          {
            // Return a 204 No Content response
            return Results.NoContent();
          }
          else
          {
            // Return a 404 Not Found response
            return Results.NotFound();
          }
        }
        catch (Exception ex)
        {
          return Results.Problem($"An error occurred while deleting the category: {ex.Message}");
        }
      });

      app.MapPut("/categories/{id}", async (int id, Category updatedCategory, DatabaseService db) =>
      {
        try
        {

          // Set the ID from the route parameter
          updatedCategory.Id = id;

          // Update the Category
          var result = await db.UpdateCategoryAsync(updatedCategory);

          // Return the updated Category
          return Results.Ok(result);
        }
        catch (Exception ex)
        {
          return Results.Problem($"An error occurred while updating the Category: {ex.Message}");
        }
      });
    }
  }
}