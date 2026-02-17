using ApiEcommerce.Constants;
using ApiEcommerce.Models.Dtos;
using ApiEcommerce.Repository.IRepository;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace ApiEcommerce.Controllers.V2
{
    [Route("api/v{version:apiVersion}/[controller]")] 
    [ApiVersion("2.0")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    //[EnableCors(PolicyNames.AllowSpecificOrigin)]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;

        public CategoriesController(ICategoryRepository categoryRepository, IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

//Version 2.0 que ordena por id
        [AllowAnonymous]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [MapToApiVersion("2.0")]
        //[EnableCors("AllowSpecificOrigin")]
        public IActionResult GetCategoriesOrderById()
        {
            var categories = _categoryRepository.GetCategories().OrderBy(cat => cat.Id);
            var categoriesDto = new List<CategoryDto>();

            foreach(var category in categories)
            {
                categoriesDto.Add(_mapper.Map<CategoryDto>(category));
            }
            return Ok(categoriesDto);

            
        }

[AllowAnonymous]
 [HttpGet("{id:int}", Name ="GetCategory")]
 //[ResponseCache(Duration = 10)]
 [ResponseCache(CacheProfileName = CacheProfiles.Default10)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetCategory(int id)
        {
            Console.WriteLine($"Categoria con el Id: {id} a las {DateTime.Now}");
            var category = _categoryRepository.GetCategory(id);
             Console.WriteLine($"REspuesta con el Id: {id}");
            if (category == null)
            {
                return NotFound($"La categoria con el id {id} no existe");
            }
            var categoryDto = _mapper.Map<CategoryDto>(category);
            return Ok(categoryDto);  
        }

 [HttpPost]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult CreateCategory([FromBody]CreateCategoryDto createCategoryDto)
        {
           if(createCategoryDto == null)
            {
                return BadRequest(ModelState);
            }

            if (_categoryRepository.CategoryExists(createCategoryDto.Name))
            {
                ModelState.AddModelError("CustomError", "La categoria ya existe");
                return BadRequest(ModelState);
            }
            var category = _mapper.Map<Category>(createCategoryDto);
            if (!_categoryRepository.CreateCategory(category))
            {
                ModelState.AddModelError("Custom error", $"Algo salio mal al guardar {category.Name}");
                return StatusCode(500, ModelState);
            }
            return CreatedAtRoute("GetCategory", new {id=category.Id}, category);
        }

[HttpPatch("{id:int}", Name = "UpdateCategory")]
       [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
public IActionResult UpdateCategory(int id, [FromBody] CreateCategoryDto updateCategoryDto)
{
    if (updateCategoryDto == null)
        return BadRequest(ModelState);

    var category = _categoryRepository.GetCategory(id);

    if (category == null)
        return NotFound($"La categoría con el id {id} no existe");

    if (_categoryRepository.CategoryExists(updateCategoryDto.Name) &&
        category.Name != updateCategoryDto.Name)
    {
        ModelState.AddModelError("CustomError", "La categoría ya existe");
        return BadRequest(ModelState);
    }

    _mapper.Map(updateCategoryDto, category);

    if (!_categoryRepository.UpdateCategory(category))
    {
        ModelState.AddModelError("CustomError", $"Algo salió mal al actualizar {category.Name}");
        return StatusCode(500, ModelState);
    }

    return NoContent();
}

[HttpDelete("{id:int}", Name = "DeleteCategory")]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
[ProducesResponseType(StatusCodes.Status500InternalServerError)]
public IActionResult DeleteCategory(int id)
{
    // 1. Validar si existe la categoría
    var category = _categoryRepository.GetCategory(id);

    if (category == null)
        return NotFound($"La categoría con el id {id} no existe");

    // 2. Intentar eliminar
    if (!_categoryRepository.DeleteCategory(category))
    {
        ModelState.AddModelError("CustomError", $"Algo salió mal al eliminar la categoría {category.Name}");
        return StatusCode(500, ModelState);
    }

    // 3. Todo OK
    return NoContent();
}



    }
}
