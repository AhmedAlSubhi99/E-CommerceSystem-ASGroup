using E_CommerceSystem.Models;
using E_CommerceSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_CommerceSystem.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class SupplierController : ControllerBase
    {
        private readonly ISupplierService _service;
        private readonly ILogger<SupplierController> _logger;

        public SupplierController(ISupplierService service, ILogger<SupplierController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var result = _service.GetAll();
            _logger.LogInformation("Fetched {Count} suppliers.", result.Count());
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public IActionResult GetById(int id)
        {
            var dto = _service.GetById(id);
            if (dto == null)
            {
                _logger.LogWarning("Supplier with ID {SupplierId} not found.", id);
                return NotFound($"Supplier with ID {id} not found.");
            }

            _logger.LogInformation("Fetched supplier with ID {SupplierId}.", id);
            return Ok(dto);
        }

        [Authorize(Roles = "admin,manager")]
        [HttpPost]
        public IActionResult Create([FromBody] SupplierDTO dto)
        {
            var created = _service.Create(dto);
            _logger.LogInformation("Supplier {SupplierName} created with ID {SupplierId}.", created.Name, created.SupplierId);
            return CreatedAtAction(nameof(GetById), new { id = created.SupplierId }, created);
        }

        [Authorize(Roles = "admin,manager")]
        [HttpPut("{id:int}")]
        public IActionResult Update(int id, [FromBody] SupplierDTO dto)
        {
            var updated = _service.Update(id, dto);
            if (!updated)
            {
                _logger.LogWarning("Update failed: Supplier with ID {SupplierId} not found.", id);
                return NotFound($"Supplier with ID {id} not found.");
            }

            _logger.LogInformation("Supplier with ID {SupplierId} updated successfully.", id);
            return NoContent();
        }

        [Authorize(Roles = "admin")]
        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            var deleted = _service.Delete(id);
            if (!deleted)
            {
                _logger.LogWarning("Delete failed: Supplier with ID {SupplierId} not found.", id);
                return NotFound($"Supplier with ID {id} not found.");
            }

            _logger.LogInformation("Supplier with ID {SupplierId} deleted successfully.", id);
            return NoContent();
        }
    }
}
