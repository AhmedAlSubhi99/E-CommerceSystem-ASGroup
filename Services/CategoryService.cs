using AutoMapper;
using E_CommerceSystem.Models;
using E_CommerceSystem.Repositories;

namespace E_CommerceSystem.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepo _repo;
        private readonly IMapper _mapper;

        public CategoryService(ICategoryRepo repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public IEnumerable<CategoryDTO> GetAll() =>
            _mapper.Map<IEnumerable<CategoryDTO>>(_repo.GetAll());

        public CategoryDTO? GetById(int id)
        {
            var entity = _repo.GetById(id);
            return entity == null ? null : _mapper.Map<CategoryDTO>(entity);
        }

        public CategoryDTO Create(CategoryDTO input)
        {
            var entity = _mapper.Map<Category>(input);
            _repo.Add(entity);
            return _mapper.Map<CategoryDTO>(entity);
        }

        public bool Update(int id, CategoryDTO input)
        {
            var existing = _repo.GetById(id);
            if (existing == null) return false;
            _mapper.Map(input, existing);
            _repo.Update(existing);
            return true;
        }

        public bool Delete(int id) => _repo.Delete(id);
    }
}
