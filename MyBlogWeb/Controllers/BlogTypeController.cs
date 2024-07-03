using Application.Interfaces.Services;
using Application.ModelViews.BlogTypeViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MyBlogWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlogTypeController : ControllerBase
    {
        private readonly IBlogTypeService _blogTypeService;

        public BlogTypeController(IBlogTypeService blogTypeService)
        {
            _blogTypeService = blogTypeService;


        }
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var categories = await _blogTypeService.GetBlogTypes();
                if (categories.Items.Count == 0)
                {
                    return BadRequest("Không tìm thấy");
                }
                else
                {
                    return Ok(categories);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute] Guid id)
        {
            try
            {
                var category = await _blogTypeService.GetBlogTypeById(id);
                if (category == null)
                {
                    return BadRequest("Không tìm thấy");
                }
                else
                {
                    return Ok(category);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Post([FromBody] BlogTypeModel blogTypeModel)
        {
            try
            {
                await _blogTypeService.AddBlogType(blogTypeModel);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok();
        }
        [HttpPut("{id}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Put([FromRoute] Guid id, [FromBody] BlogTypeModel blogTypeModel)
        {
            try
            {
                await _blogTypeService.UpdateBlogType(id, blogTypeModel);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok();
        }
        [HttpDelete("{id}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            try
            {
                await _blogTypeService.DeleteBlogType(id);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok();
        }
    }
}
