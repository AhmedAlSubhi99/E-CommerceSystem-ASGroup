using AutoMapper;
using E_CommerceSystem;
using E_CommerceSystem.Models;
using E_CommerceSystem.Repositories;
using E_CommerceSystem.Services;
using Microsoft.EntityFrameworkCore;


public class CategoryService : ICategoryService
{
    private readonly ApplicationDbContext _ctx;
    private readonly IMapper _mapper;

    public CategoryService(ApplicationDbContext ctx, IMapper mapper)
    {
        _ctx = ctx;
        _mapper = mapper;
    }

    public async Task<IEnumerable<CategoryDTO>> GetAllAsync()
    {
        var entities = await _ctx.Categories.AsNoTracking().ToListAsync();
        return _mapper.Map<IEnumerable<CategoryDTO>>(entities);
    }

    public async Task<CategoryDTO?> GetByIdAsync(int id)
    {
        var entity = await _ctx.Categories.FindAsync(id);
        return entity is null ? null : _mapper.Map<CategoryDTO>(entity);
    }

    public async Task<CategoryDTO> CreateAsync(CategoryCreateDto dto)
    {
        var entity = _mapper.Map<Category>(dto);
        _ctx.Categories.Add(entity);
        await _ctx.SaveChangesAsync();
        return _mapper.Map<CategoryDTO>(entity);
    }

    public async Task<bool> UpdateAsync(int id, CategoryUpdateDto dto)
    {
        var entity = await _ctx.Categories.FindAsync(id);
        if (entity is null) return false;

        _mapper.Map(dto, entity);       // copies fields onto tracked entity
        await _ctx.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _ctx.Categories.FindAsync(id);
        if (entity is null) return false;

        _ctx.Categories.Remove(entity);
        await _ctx.SaveChangesAsync();
        return true;
    }
}
