using Microsoft.AspNetCore.Authorization;

namespace SchoolManagerApi.Controllers
{
    [Authorize(Policy = "StoreAccessPolicy")]
    public class ProductsController : DefaultController
    {
        public ProductsController()
        {

        }
    }
}