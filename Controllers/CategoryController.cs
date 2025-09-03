using E_CommerceSystem.Models;
using E_CommerceSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_CommerceSystem.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _service;

        public CategoryController(ICategoryService service)
        {
            _service = service;
        }

        [HttpGet]
        public IActionResult GetAll() => Ok(_service.GetAll());

        [HttpGet("{id:int}")]
        public IActionResult GetById(int id)
        {
            var dto = _service.GetById(id);
            return dto == null ? NotFound($"Category with ID {id} not found.") : Ok(dto);
        }

        [Authorize(Roles = "admin,manager")]
        [HttpPost]
        public IActionResult Create([FromBody] CategoryDTO dto)
        {
            var created = _service.Create(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.CategoryId }, created);
        }

        [Authorize(Roles = "admin,manager")]
        [HttpPut("{id:int}")]
        public IActionResult Update(int id, [FromBody] CategoryDTO dto)
        {
            return _service.Update(id, dto)
                ? NoContent()
                : NotFound($"Category with ID {id} not found.");
        }

        [Authorize(Roles = "admin")]
        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            return _service.Delete(id)
                ? NoContent()
                : NotFound($"Category with ID {id} not found.");
        }
    }
}
