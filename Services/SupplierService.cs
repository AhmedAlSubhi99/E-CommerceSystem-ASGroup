using AutoMapper;
using E_CommerceSystem.Models;
using E_CommerceSystem.Repositories;
using Microsoft.EntityFrameworkCore;

namespace E_CommerceSystem.Services
{
    public class SupplierService : ISupplierService
    {
        private readonly ISupplierRepo _repo;
        private readonly ApplicationDbContext _ctx;
        private readonly IMapper _mapper;
        public SupplierService(ISupplierRepo repo, ApplicationDbContext ctx, IMapper mapper)
        {
            _repo = repo;
            _ctx = ctx;
            _mapper = mapper;
        }

        public async Task<IEnumerable<Supplier>> GetAllAsync() =>
           await _ctx.Suppliers.AsNoTracking().ToListAsync();


        public async Task<Supplier?> GetByIdAsync(int id) =>
          await _ctx.Suppliers.AsNoTracking().FirstOrDefaultAsync(x => x.SupplierId == id);

        public async Task<Supplier> CreateAsync(SupplierCreateDto dto)
        {
            var entity = _mapper.Map<Supplier>(dto);
            _ctx.Suppliers.Add(entity);
            await _ctx.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> UpdateAsync(int id, SupplierUpdateDto dto)
        {
            var existing = await _ctx.Suppliers.FirstOrDefaultAsync(x => x.SupplierId == id);
            if (existing is null) return false;

            _mapper.Map(dto, existing);
            await _ctx.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var existing = await _ctx.Suppliers.FirstOrDefaultAsync(x => x.SupplierId == id);
            if (existing is null) return false;

            _ctx.Suppliers.Remove(existing);
            await _ctx.SaveChangesAsync();
            return true;
        }
    }
}
