using AutoMapper;
using E_CommerceSystem.Models;
using E_CommerceSystem.Models.DTO;
using E_CommerceSystem.Repositories.Interfaces;
using E_CommerceSystem.Services.Interfaces;

namespace E_CommerceSystem.Services
{
    public class SupplierService : ISupplierService
    {
        private readonly ISupplierRepo _repo;
        private readonly IMapper _mapper;
        private readonly ILogger<SupplierService> _logger;

        public SupplierService(ISupplierRepo repo, IMapper mapper, ILogger<SupplierService> logger)
        {
            _repo = repo;
            _mapper = mapper;
            _logger = logger;
        }

        public IEnumerable<SupplierDTO> GetAll()
        {
            var suppliers = _mapper.Map<IEnumerable<SupplierDTO>>(_repo.GetAll());
            _logger.LogInformation("Fetched {Count} suppliers from database.", suppliers.Count());
            return suppliers;
        }

        public SupplierDTO? GetById(int id)
        {
            var entity = _repo.GetById(id);
            if (entity == null)
            {
                _logger.LogWarning("Supplier with ID {SupplierId} not found.", id);
                return null;
            }

            _logger.LogInformation("Fetched supplier with ID {SupplierId}.", id);
            return _mapper.Map<SupplierDTO>(entity);
        }

        public SupplierDTO Create(SupplierDTO input)
        {
            var entity = _mapper.Map<Supplier>(input);
            _repo.Add(entity);
            _logger.LogInformation("Supplier {SupplierName} created with ID {SupplierId}.", entity.Name, entity.SupplierId);
            return _mapper.Map<SupplierDTO>(entity);
        }

        public bool Update(int id, SupplierDTO input)
        {
            var existing = _repo.GetById(id);
            if (existing == null)
            {
                _logger.LogWarning("Update failed: Supplier with ID {SupplierId} not found.", id);
                return false;
            }

            _mapper.Map(input, existing);
            _repo.Update(existing);

            _logger.LogInformation("Supplier {SupplierId} updated successfully.", id);
            return true;
        }

        public bool Delete(int id)
        {
            var result = _repo.Delete(id);
            if (result)
            {
                _logger.LogInformation("Supplier {SupplierId} deleted successfully.", id);
            }
            else
            {
                _logger.LogWarning("Delete failed: Supplier {SupplierId} not found.", id);
            }
            return result;
        }
    }
}
