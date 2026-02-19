using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ApiEcommerce.Repository.IRepository;
using AutoMapper;
using ApiEcommerce.Models.Dtos;
using ApiEcommerce.Models;
using Microsoft.AspNetCore.Authorization;
using Asp.Versioning;
using ApiEcommerce.Models.Dtos.Responses;

namespace ApiEcommerce.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersionNeutral]
    public class ProductsController : ControllerBase
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;

        public ProductsController(IProductRepository productRepository, ICategoryRepository categoryRepository, IMapper mapper)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        //METODO REFACTORIZADO PARA GUARDAR IMÁGENES 

        private void SaveProductImage(Product product, IFormFile image)
        {
            string fileName = product.ProductId + Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);

            var imagesFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "ProductsImages");
            if (!Directory.Exists(imagesFolder))
            {
                Directory.CreateDirectory(imagesFolder);
            }

            var filePath = Path.Combine(imagesFolder, fileName);

            using var fileStream = new FileStream(filePath, FileMode.Create);
            image.CopyTo(fileStream);

            var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}{HttpContext.Request.PathBase.Value}";
            product.ImgUrl = $"{baseUrl}/ProductsImages/{fileName}";
            product.ImgUrlLocal = filePath;
        }



        [AllowAnonymous]
        [HttpGet]
        public IActionResult GetProducts()
        {
            var products = _productRepository.GetProducts();
            var productsDto = _mapper.Map<List<ProductDto>>(products);
            return Ok(productsDto);
        }

        [AllowAnonymous]
        [HttpGet("Paged", Name = "GetProductInPage")]
        public IActionResult GetProductInPage([FromQuery] int pageNumber =1, [FromQuery] int pageSize=5)
        {
            if(pageNumber <1 || pageSize < 1)
            {
                return BadRequest("Los parametros de paginacion son incorrectos");
            }
            var totalProducts = _productRepository.GetTotalProducts();
            var totalPages = (int)Math.Ceiling((double)totalProducts/pageSize);
            if(pageNumber > totalPages)
            {
                return NotFound("No hay mas paginas");
            }
            var products = _productRepository.GetProductsInPages(pageNumber, pageSize);

            var productDto = _mapper.Map<List<ProductDto>>(products);
            var paginationResponse = new PaginationResponse<ProductDto>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages,
                Items = productDto
            };
            return Ok(paginationResponse);
        }

           [AllowAnonymous]
        [HttpGet("{productId:int}", Name = "GetProduct")]
        public IActionResult GetProduct(int productId)
        {
            var product = _productRepository.GetProduct(productId);

            if (product == null)
            {
                return NotFound($"El producto con el id {productId} no existe");
            }

            var productDto = _mapper.Map<ProductDto>(product);
            return Ok(productDto);
        }

        [HttpPost]
        public IActionResult CreateProduct([FromForm] CreateProductDto createProductDto)
        {
            if (createProductDto == null)
                return BadRequest(ModelState);

            if (_productRepository.ProductExists(createProductDto.Name))
            {
                ModelState.AddModelError("CustomError", "El producto ya existe");
                return BadRequest(ModelState);
            }

            if (!_categoryRepository.CategoryExists(createProductDto.CategoryId))
            {
                ModelState.AddModelError("CustomError", "La categoría no existe");
                return BadRequest(ModelState);
            }

            var product = _mapper.Map<Product>(createProductDto);

            if (createProductDto.Image != null)
                SaveProductImage(product, createProductDto.Image);
            else
                product.ImgUrl = "https://placehold.co/400x300";

            if (!_productRepository.CreateProduct(product))
            {
                ModelState.AddModelError("CustomError", $"Algo salió mal al guardar {product.Name}");
                return StatusCode(500, ModelState);
            }

            var createdProduct = _productRepository.GetProduct(product.ProductId);
            var productDto = _mapper.Map<ProductDto>(createdProduct);

            return CreatedAtRoute("GetProduct", new { productId = product.ProductId }, productDto);
        }


        [HttpPut("{productId:int}", Name = "UpdateProduct")]
        public IActionResult UpdateProduct(int productId, [FromForm] UpdateProductDto updateProductDto)
        {
            if (updateProductDto == null)
                return BadRequest(ModelState);

            if (!_productRepository.ProductExists(productId))
                return NotFound($"No se encontró un producto con el ID {productId}");

            if (!_categoryRepository.CategoryExists(updateProductDto.CategoryId))
            {
                ModelState.AddModelError("CustomError", "La categoría especificada no existe");
                return BadRequest(ModelState);
            }

            var product = _mapper.Map<Product>(updateProductDto);
            product.ProductId = productId;

            if (updateProductDto.Image != null)
                SaveProductImage(product, updateProductDto.Image);

            if (!_productRepository.UpdateProduct(product))
            {
                ModelState.AddModelError("CustomError", $"Algo salió mal al actualizar el producto {product.Name}");
                return StatusCode(500, ModelState);
            }

            return NoContent();
        }


        [HttpDelete("{productId:int}", Name = "DeleteProduct")]
        public IActionResult DeleteProduct(int productId)
        {
            if (productId == 0)
                return BadRequest(ModelState);

            var product = _productRepository.GetProduct(productId);

            if (product == null)
                return NotFound($"El producto con el id {productId} no existe");

            if (!_productRepository.DeleteProduct(product))
            {
                ModelState.AddModelError("CustomError", $"Algo salió mal al borrar el producto {product.Name}");
                return StatusCode(500, ModelState);
            }

            return NoContent();
        }
    }
}
