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

        public IEnumerable<SupplierDTO> GetAll() =>
            _mapper.Map<IEnumerable<SupplierDTO>>(_repo.GetAll());

        public SupplierDTO? GetById(int id)
        {
            var entity = _repo.GetById(id);
            return entity == null ? null : _mapper.Map<SupplierDTO>(entity);
        }

        public SupplierDTO Create(SupplierDTO input)
        {
            var entity = _mapper.Map<Supplier>(input);
            _repo.Add(entity);
            return _mapper.Map<SupplierDTO>(entity);
        }

        public bool Update(int id, SupplierDTO input)
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
