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
        private readonly ILogger<CategoryController> _logger;

        public CategoryController(ICategoryService service, ILogger<CategoryController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var result = _service.GetAll();
            _logger.LogInformation("Fetched {Count} categories.", result.Count());
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public IActionResult GetById(int id)
        {
            var dto = _service.GetById(id);
            if (dto == null)
            {
                _logger.LogWarning("Category with ID {CategoryId} not found.", id);
                return NotFound($"Category with ID {id} not found.");
            }

            _logger.LogInformation("Fetched category with ID {CategoryId}.", id);
            return Ok(dto);
        }

        [Authorize(Roles = "admin,manager")]
        [HttpPost]
        public IActionResult Create([FromBody] CategoryDTO dto)
        {
            var created = _service.Create(dto);
            _logger.LogInformation("Category {CategoryName} created with ID {CategoryId}.", created.Name, created.CategoryId);
            return CreatedAtAction(nameof(GetById), new { id = created.CategoryId }, created);
        }

        [Authorize(Roles = "admin,manager")]
        [HttpPut("{id:int}")]
        public IActionResult Update(int id, [FromBody] CategoryDTO dto)
        {
            var updated = _service.Update(id, dto);
            if (!updated)
            {
                _logger.LogWarning("Update failed: Category with ID {CategoryId} not found.", id);
                return NotFound($"Category with ID {id} not found.");
            }

            _logger.LogInformation("Category with ID {CategoryId} updated successfully.", id);
            return NoContent();
        }

        [Authorize(Roles = "admin")]
        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            var deleted = _service.Delete(id);
            if (!deleted)
            {
                _logger.LogWarning("Delete failed: Category with ID {CategoryId} not found.", id);
                return NotFound($"Category with ID {id} not found.");
            }

            _logger.LogInformation("Category with ID {CategoryId} deleted successfully.", id);
            return NoContent();
        }
    }
}
