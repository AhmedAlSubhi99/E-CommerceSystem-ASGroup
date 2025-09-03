// Controllers/CategoryController.cs
using AutoMapper;
using E_CommerceSystem.Models;
using E_CommerceSystem.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _service;
    private readonly IMapper _mapper;
    public CategoryController(ICategoryService service, IMapper mapper)
    { _service = service; _mapper = mapper; }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryDTO>>> GetAll()
    {
        var cats = await _service.GetAllAsync();
        return Ok(_mapper.Map<IEnumerable<CategoryDTO>>(cats));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CategoryDTO>> GetById(int id)
    {
        var cat = await _service.GetByIdAsync(id);
        if (cat is null) return NotFound($"Category {id} not found.");
        return Ok(_mapper.Map<CategoryDTO>(cat));
    }

    [HttpPost]
    public async Task<ActionResult<CategoryDTO>> Create([FromBody] CategoryCreateDTO dto)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var created = await _service.CreateAsync(dto);
        var readDto = _mapper.Map<CategoryDTO>(created);

        return CreatedAtAction(nameof(GetById), new { id = readDto.CategoryId }, readDto);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] CategoryUpdateDto dto)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var ok = await _service.UpdateAsync(id, dto);
        return ok ? NoContent() : NotFound($"Category {id} not found.");
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _service.DeleteAsync(id);
        return ok ? NoContent() : NotFound($"Category {id} not found.");
    }
}
