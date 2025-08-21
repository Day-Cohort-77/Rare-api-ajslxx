using RareAPI.Models;
using RareAPI.Services;

namespace RareAPI.Endpoints
{
  public static class CategoryEndpoints
  {
    public static void MapCategoryEndpoints(this WebApplication app)
    {
      app.MapGet("/categories", async (CategoriesServices categoriesService) =>
      {
        var categories = await categoriesService.GetAllCategoriesAsync();
        return Results.Ok(categories);
      });

      app.MapGet("/categories/{id}", async (int id, CategoriesServices categoriesService) =>
      {
        var category = await categoriesService.GetCategoryByIdAsync(id);
        return category is not null ? Results.Ok(category) : Results.NotFound();
      });

      app.MapPost("/categories", async (Category category, CategoriesServices categoriesService) =>
      {
        try
        {
          var newCategory = await categoriesService.CreateCategoryAsync(category);
          return Results.Created($"/categories/{category.Id}", newCategory);
        }
        catch (Exception ex)
        {
          return Results.Problem($"An error occurred while creating the category: {ex.Message}");
        }
      });

      app.MapDelete("/categories/{id}", async (int id, CategoriesServices categoriesService) =>
      {
        try
        {
          bool deleted = await categoriesService.DeleteCategoryAsync(id);

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

      app.MapPut("/categories/{id}", async (int id, Category updatedCategory, CategoriesServices categoriesService) =>
      {
        try
        {

          // Set the ID from the route parameter
          updatedCategory.Id = id;

          // Update the Category
          var result = await categoriesService.UpdateCategoryAsync(updatedCategory);

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
