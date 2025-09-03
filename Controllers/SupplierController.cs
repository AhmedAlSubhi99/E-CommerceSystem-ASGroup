// Controllers/SupplierController.cs
using AutoMapper;
using E_CommerceSystem.Models;
using E_CommerceSystem.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class SupplierController : ControllerBase
{
    private readonly ISupplierService _service;
    private readonly IMapper _mapper;
    public SupplierController(ISupplierService service, IMapper mapper)
    { _service = service; _mapper = mapper; }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SupplierDTO>>> GetAll()
    {
        var items = await _service.GetAllAsync();
        return Ok(_mapper.Map<IEnumerable<SupplierDTO>>(items));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<SupplierDTO>> GetById(int id)
    {
        var sup = await _service.GetByIdAsync(id);
        if (sup is null) return NotFound($"Supplier {id} not found.");
        return Ok(_mapper.Map<SupplierDTO>(sup));
    }

    [HttpPost]
    public async Task<ActionResult<SupplierDTO>> Create([FromBody] SupplierCreateDto dto)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var created = await _service.CreateAsync(dto);
        var readDto = _mapper.Map<SupplierDTO>(created);

        return CreatedAtAction(nameof(GetById), new { id = readDto.SupplierId }, readDto);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] SupplierUpdateDto dto)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var ok = await _service.UpdateAsync(id, dto);
        return ok ? NoContent() : NotFound($"Supplier {id} not found.");
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _service.DeleteAsync(id);
        return ok ? NoContent() : NotFound($"Supplier {id} not found.");
    }
}
