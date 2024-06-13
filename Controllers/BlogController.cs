using Application.Interfaces.Services;
using Application.ModelViews.BlogViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MyBlogWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlogController : ControllerBase
    {
        private readonly IBlogService _blogService;
        private readonly IClaimsService _claims;

        public BlogController(IBlogService blogService,
            IClaimsService claimsService)
        {
            _blogService = blogService;            
            _claims = claimsService;
        }

        [HttpGet]
        [Authorize()]
        public async Task<IActionResult> GetByFilter([FromQuery] FilterBlogModel filterBonsaiModel, int pageIndex = 0, int pageSize = 20)
        {
            try
            {
                var products = await _blogService.GetByFilter(pageIndex, pageSize, filterBonsaiModel, _claims.GetIsAdmin);
                return Ok(products);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost]
        public async Task<IActionResult> Post([FromForm] BlogModel productModel)
        {
            try
            {
                await _blogService.AddAsync(productModel);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok("Tạo sản phẩm thành công!");
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> Get([FromRoute] Guid id)
        {
            try
            {
                var product = await _blogService.GetById(id, _claims.GetIsAdmin);
                if (product == null)
                {
                    return NotFound();
                }
                return Ok(product);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }
        [HttpPut("{id}")]
        [Authorize()]
        public async Task<IActionResult> Put([FromRoute] Guid id, [FromForm] BlogModel productModel)
        {
            try
            {
                await _blogService.Update(id, productModel);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok("Cập nhật thành công!");
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            try
            {
                await _blogService.Delete(id);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok("Xóa thành công!");
        }
        [HttpGet("BlogType/{blogTypeId}")]
        public async Task<IActionResult> GetByBlogType([FromRoute] Guid blogTypeId, [FromQuery] int pageIndex = 0, int pageSize = 20)
        {
            try
            {
                var products = await _blogService.GetByBlogType(pageIndex, pageSize, blogTypeId);
                return Ok(products);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPut("Disable")]
        public async Task<IActionResult> DisableBlog([FromQuery] Guid productId)
        {
            try
            {
                await _blogService.DisableBlog(productId);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
