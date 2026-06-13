using DTO;
using Microsoft.AspNetCore.Mvc;
using ServiceContract.DTO;
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
            public IActionResult GetById(int id)
            {
                var result = _productService.GetById(id);
                if (result == null) return NotFound();
                return Ok(result);
            }

            [HttpPost]
            public IActionResult Add(DtoproductAdd model)
            {
                var result = _productService.AddProduct(model);
                if (!result.Succeeded) return BadRequest(result.Errormessage);
                return Ok(result.Data);
            }

            [HttpDelete("{id}")]
            public IActionResult Delete(int id)
            {
                var result = _productService.Delete(id);
                if (!result.Succeeded) return BadRequest(result.Errormessage);
                return Ok();
            }
        }
    }





