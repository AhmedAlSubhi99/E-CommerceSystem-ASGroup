using AutoMapper;
using E_CommerceSystem.Models;
using E_CommerceSystem.Repositories;

namespace E_CommerceSystem.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepo _repo;
        private readonly IMapper _mapper;
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(ICategoryRepo repo, IMapper mapper, ILogger<CategoryService> logger)
        {
            _repo = repo;
            _mapper = mapper;
            _logger = logger;
        }

        public IEnumerable<CategoryDTO> GetAll()
        {
            var categories = _mapper.Map<IEnumerable<CategoryDTO>>(_repo.GetAll());
            _logger.LogInformation("Fetched {Count} categories from database.", categories.Count());
            return categories;
        }

        public CategoryDTO? GetById(int id)
        {
            var entity = _repo.GetById(id);
            if (entity == null)
            {
                _logger.LogWarning("Category with ID {CategoryId} not found.", id);
                return null;
            }
            _logger.LogInformation("Fetched category with ID {CategoryId}.", id);
            return _mapper.Map<CategoryDTO>(entity);
        }

        public CategoryDTO Create(CategoryDTO input)
        {
            var entity = _mapper.Map<Category>(input);
            _repo.Add(entity);
            _logger.LogInformation("Category {CategoryName} created with ID {CategoryId}.", entity.Name, entity.CategoryId);
            return _mapper.Map<CategoryDTO>(entity);
        }

        public bool Update(int id, CategoryDTO input)
        {
            var existing = _repo.GetById(id);
            if (existing == null)
            {
                _logger.LogWarning("Update failed: Category with ID {CategoryId} not found.", id);
                return false;
            }

            _mapper.Map(input, existing);
            _repo.Update(existing);

            _logger.LogInformation("Category {CategoryId} updated successfully.", id);
            return true;
        }

        public bool Delete(int id)
        {
            var result = _repo.Delete(id);
            if (result)
            {
                _logger.LogInformation("Category {CategoryId} deleted successfully.", id);
            }
            else
            {
                _logger.LogWarning("Delete failed: Category {CategoryId} not found.", id);
            }
            return result;
        }
    }
}
