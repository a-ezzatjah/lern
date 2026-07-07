using DTO;
using Microsoft.AspNetCore.Mvc;
using ServiceContract.DTO.DtoProduct;
using ServiceContract.Interfaces;

namespace lern.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var result = await _productService.GetByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> AddAsync(DtoproductAdd model)
        {
            var result = await _productService.AddProductAsync(model);
            if (!result.Succeeded) return BadRequest(result.Errors ?? new List<string> { result.Errormessage ?? "خطا" });
            return Ok(result.Data);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var result = await _productService.DeleteAsync(id);
            if (!result.Succeeded) return BadRequest(result.Errors ?? new List<string> { result.Errormessage ?? "خطا" });
            return Ok();
        }
    }
}





