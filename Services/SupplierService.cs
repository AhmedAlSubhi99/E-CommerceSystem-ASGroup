using AutoMapper;
using E_CommerceSystem.Models;
using E_CommerceSystem.Repositories;

namespace E_CommerceSystem.Services
{
    public class SupplierService : ISupplierService
    {
        private readonly ISupplierRepo _repo;
        private readonly IMapper _mapper;
        public SupplierService(ISupplierRepo repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<IEnumerable<SupplierDTO>> GetAllAsync() =>
            _mapper.Map<IEnumerable<SupplierDTO>>(await _repo.GetAllAsync());

        public async Task<SupplierDTO?> GetByIdAsync(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            return entity == null ? null : _mapper.Map<SupplierDTO>(entity);
        }

        public async Task<SupplierDTO> CreateAsync(SupplierCreateDto dto)
        {
            var entity = _mapper.Map<Supplier>(dto);
            await _repo.AddAsync(entity);
            return _mapper.Map<SupplierDTO>(entity);
        }

        public async Task<bool> UpdateAsync(int id, SupplierUpdateDto dto)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null) return false;
            _mapper.Map(dto, entity);
            await _repo.UpdateAsync(entity);
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            await _repo.DeleteAsync(id);
            return true;
        }
    }
}
