using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManagerApi.Data;
using SchoolManagerApi.DTOs;
using SchoolManagerApi.Models;

namespace SchoolManagerApi.Controllers
{
    [Authorize(Policy = Security.Policies.Store.AccessStorePolicy)]
    public class ProductsController : DefaultController
    {
        private readonly ApplicationDbContext _dbContext;

        public ProductsController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // Readonly Calls
        [HttpGet]
        public async Task<ActionResult> GetProducts()
        {
            var products = await _dbContext.Products.ToListAsync();
            var productDtos = products.Select(p => GetProductDTO(p));
            return Ok(productDtos);
        }
        [HttpGet("order/{id}")]
        public async Task<ActionResult> OrderProduct(Guid id)
        {
            var product = await _dbContext.Products.Where(p => p.Id == id).SingleOrDefaultAsync();
            if (product == null) return BadRequest("Product doesn't exist");

            var productDto = GetProductDTO(product);
            return Ok();
        }
        [HttpGet("categories")]
        public async Task<ActionResult> GetCategories()
        {
            var categories = await _dbContext.Categories.ToListAsync();
            var categoryDTOs = categories.Select(c => GetCategoryDTO(c));
            return Ok(categoryDTOs);
        }


        // Writable calls
        [Authorize(Policy = Security.Policies.Store.ManageStorePolicy)]
        [HttpPost]
        public async Task<ActionResult> AddProduct(ProductDTO productDTO)
        {
            var category = await _dbContext.Categories
                .Where(c => c.Name.ToLower() == productDTO.Category.Trim().ToLower())
                .FirstOrDefaultAsync();
            if (category == null) return BadRequest("[Error]: Unknown Category");

            var product = new Product
            {
                Name = productDTO.Name.Trim(),
                Price = productDTO.Price,
                Category = category,
                CategoryName = category.Name,
                ThumbnailUrl = "https://st3.depositphotos.com/5093981/16041/i/600/depositphotos_160415976-stock-photo-blank-papers-white-table-blank.jpg",
                ThumbnailId = "Placeholder"
            };
            _dbContext.Products.Add(product);
            if (await _dbContext.SaveChangesAsync() > 0)
                return Ok(new { Message = "Product Added Successfully" });
            return BadRequest("Can't add product");
        }
        [Authorize(Policy = Security.Policies.Store.ManageStorePolicy)]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteProduct(Guid id)
        {
            var product = await _dbContext.Products
                .Where(p => p.Id == id).FirstOrDefaultAsync();
            if (product == null) return BadRequest("[Error]: Unknown Product");

            _dbContext.Products.Remove(product);
            if (await _dbContext.SaveChangesAsync() > 0)
                return Ok(new { Message = "Product Deleted Successfully" });
            return BadRequest("Can't delete product");
        }

        [Authorize(Policy = Security.Policies.Store.ManageStorePolicy)]
        [HttpPost("category")]
        public async Task<ActionResult> AddCategory(CategoryDTO categoryDTO)
        {
            var category = await _dbContext.Categories
                .Where(c => c.Name.ToLower() == categoryDTO.Name.Trim().ToLower())
                .FirstOrDefaultAsync();

            if (category != null) return BadRequest("[Error]: Can't Add Duplicate Category");

            category = new Category
            {
                Name = categoryDTO.Name.Trim()
            };
            _dbContext.Categories.Add(category);
            if (await _dbContext.SaveChangesAsync() > 0) // Return all categories
            {
                var categories = await _dbContext.Categories.ToListAsync();
                var categoryDTOs = categories.Select(c => GetCategoryDTO(c));
                return Ok(categoryDTOs);
            }
            return BadRequest("Can't add category");
        }
        [Authorize(Policy = Security.Policies.Store.ManageStorePolicy)]
        [HttpDelete("category/{id}")]
        public async Task<ActionResult> DeleteCategory(Guid id)
        {
            var category = await _dbContext.Categories
                .Where(c => c.Id == id)
                .Include(c => c.Products)
                .FirstOrDefaultAsync();

            if (category == null) return BadRequest("[Error]: Category Doesn't Exist.");

            _dbContext.Categories.Remove(category);
            if (await _dbContext.SaveChangesAsync() > 0) // Return all categories
            {
                var categories = await _dbContext.Categories.ToListAsync();
                var categoryDTOs = categories.Select(c => GetCategoryDTO(c));
                return Ok(categoryDTOs);
            }
            return BadRequest("Can't remove category");
        }

        private ProductDTO GetProductDTO(Product product)
        {
            return new ProductDTO
            {
                Name = product.Name,
                Id = product.Id,
                Category = product.CategoryName,
                Price = product.Price,
                ThumbnailUrl = product.ThumbnailUrl,
                ThumbnailId = product.ThumbnailId
            };
        }
        private CategoryDTO GetCategoryDTO(Category category)
        {
            return new CategoryDTO
            {
                Name = category.Name,
                Id = category.Id
            };
        }
    }
}