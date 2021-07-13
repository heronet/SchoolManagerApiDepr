using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
        private readonly UserManager<EntityUser> _userManager;

        public ProductsController(ApplicationDbContext dbContext, UserManager<EntityUser> userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }

        // Readonly Calls
        [HttpGet]
        public async Task<ActionResult> GetProducts()
        {
            var products = await _dbContext.Products.ToListAsync();
            var productDtos = products.Select(p => GetProductDTO(p));
            return Ok(productDtos);
        }

        [HttpGet("categories")]
        public async Task<ActionResult> GetCategories()
        {
            var categories = await _dbContext.Categories.ToListAsync();
            var categoryDTOs = categories.Select(c => GetCategoryDTO(c));
            return Ok(categoryDTOs);
        }
        [Authorize(Policy = Security.Policies.Store.ManageStorePolicy)]
        [HttpGet("orders")]
        public async Task<ActionResult> GetOrders()
        {
            var orders = await _dbContext.Orders
                .Include(o => o.User)
                .Include(o => o.Product)
                .ToListAsync();
            var orderDtos = orders.Select(o => GetOrderDto(o));
            return Ok(orderDtos);
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
                Stock = productDTO.Stock,
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
                .Where(p => p.Id == id)
                .Include(p => p.Orders) // Include orders to set the product in them to null
                .FirstOrDefaultAsync();
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
                .ThenInclude(p => p.Orders)
                .OrderBy(c => c.Name)
                .AsSplitQuery()
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

        [Authorize(Policy = Security.Policies.Store.OrderFromStorePolicy)]
        [HttpPost("order")]
        public async Task<ActionResult> OrderProduct(OrderDTO orderDTO)
        {
            var user = await _userManager.FindByIdAsync(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            if (user == null) BadRequest("Invalid User");

            var product = await _dbContext.Products.Where(p => p.Id == orderDTO.ProductId).SingleOrDefaultAsync();
            if (product == null) return BadRequest("Product doesn't exist");

            var order = new Order
            {
                Product = product,
                User = user,
                OrderedItemsCount = orderDTO.OrderedItemsCount
            };

            _dbContext.Orders.Add(order);

            if (await _dbContext.SaveChangesAsync() > 0)
                return Ok();
            return BadRequest("Couldn't place Order");
        }

        [Authorize(Policy = Security.Policies.Store.ManageStorePolicy)]
        [HttpPatch("orders")]
        public async Task<ActionResult> DeliverOrder(OrderDTO orderDTO)
        {
            var order = await _dbContext.Orders
                .Where(o => o.Id == orderDTO.Id)
                .SingleOrDefaultAsync();

            var product = await _dbContext.Products
                .Where(p => p.Id == order.ProductId)
                .SingleOrDefaultAsync();

            var productPrice = product?.Price ?? 0;
            order.DeliveryMan = orderDTO.DeliveryMan ?? "Unknown";
            order.DeliveredItemsCount = orderDTO.DeliveredItemsCount;
            order.TotalPrice = productPrice * orderDTO.DeliveredItemsCount;
            order.Delivered = true;
            product.Stock -= orderDTO.DeliveredItemsCount;

            _dbContext.Update(order);
            _dbContext.Update(product);

            if (await _dbContext.SaveChangesAsync() > 0)
                return Ok(new { Message = "Delivered!" });
            return BadRequest("Faild to deliver");
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
                ThumbnailId = product.ThumbnailId,
                Stock = product.Stock
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
        private OrderDTO GetOrderDto(Order order)
        {
            return new OrderDTO
            {
                Id = order.Id,
                ProductId = order.ProductId,
                UserId = order.UserId,
                ProductName = order.Product?.Name ?? "Deleted Product",
                Username = order.User?.UserName ?? "Deleted User",
                CreatedAt = order.CreatedAt,
                Delivered = order.Delivered,
                DeliveryMan = order.DeliveryMan,
                DeliveredItemsCount = order.DeliveredItemsCount,
                OrderedItemsCount = order.OrderedItemsCount,
                TotalPrice = order.TotalPrice
            };
        }
    }
}