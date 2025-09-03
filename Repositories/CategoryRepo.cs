using E_CommerceSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace E_CommerceSystem.Repositories
{
    public class CategoryRepo : ICategoryRepo
    {
        private readonly ApplicationDbContext _ctx;
        public CategoryRepo(ApplicationDbContext ctx) => _ctx = ctx;

        public IEnumerable<Category> GetAll() => _ctx.Categories.AsNoTracking().ToList();

        public Category? GetById(int id) =>
            _ctx.Categories.Include(c => c.Products).FirstOrDefault(c => c.CategoryId == id);

        public void Add(Category entity)
        {
            _ctx.Categories.Add(entity);
            _ctx.SaveChanges();
        }

        public void Update(Category entity)
        {
            _ctx.Categories.Update(entity);
            _ctx.SaveChanges();
        }

        public bool Delete(int id)
        {
            var entity = _ctx.Categories.FirstOrDefault(c => c.CategoryId == id);
            if (entity == null) return false;
            _ctx.Categories.Remove(entity);
            _ctx.SaveChanges();
            return true;
        }

        public bool Exists(int id) => _ctx.Categories.Any(c => c.CategoryId == id);
    }
}
