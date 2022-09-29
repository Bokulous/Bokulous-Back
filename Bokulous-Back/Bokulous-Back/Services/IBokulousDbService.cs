using Bokulous_Back.Models;

namespace Bokulous_Back.Services
{
    public interface IBokulousDbService
    {
        Task CreateBookAsync(Book newBook);
        Task CreateCategoryAsync(Category newCategory);
        Task CreateUserAsync(User newUser);
        Task<List<Book>> GetBookAsync();
        Task<Book?> GetBookAsync(string id);
        Task<List<Category>> GetCategoryAsync();
        Task<Category?> GetCategoryAsync(string id);
        Task<List<User>> GetUserAsync();
        Task<User?> GetUserAsync(string id);
        Task<User> LoginAsync(User userLogin);
        Task RemoveBookAsync(string id);
        Task RemoveCategoryAsync(string id);
        Task RemoveUserAsync(string id);
        Task UpdateBookAsync(string id, Book updatedBook);
        Task UpdateCategoryAsync(string id, Category updatedCategory);
        Task UpdateUserAsync(string id, User updatedUser);
    }
}