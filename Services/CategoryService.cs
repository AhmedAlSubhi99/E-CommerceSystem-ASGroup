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

    public async Task<IEnumerable<Category>> GetAllAsync() =>
           await _ctx.Categories.AsNoTracking().ToListAsync();

    public async Task<Category?> GetByIdAsync(int id) =>
       await _ctx.Categories.AsNoTracking().FirstOrDefaultAsync(x => x.CategoryId == id);

    public async Task<Category> CreateAsync(CategoryCreateDTO dto)
    {
        var entity = _mapper.Map<Category>(dto);
        _ctx.Categories.Add(entity);
        await _ctx.SaveChangesAsync();
        return entity;
    }

    public async Task<bool> UpdateAsync(int id, CategoryUpdateDto dto)
    {
        var existing = await _ctx.Categories.FirstOrDefaultAsync(x => x.CategoryId == id);
        if (existing is null) return false;

        _mapper.Map(dto, existing); // maps Name/Description
        await _ctx.SaveChangesAsync();
        return true;
    }


    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await _ctx.Categories.FirstOrDefaultAsync(x => x.CategoryId == id);
        if (existing is null) return false;

        _ctx.Categories.Remove(existing);
        await _ctx.SaveChangesAsync();
        return true;
    }
}
