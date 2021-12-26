using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using winformasync.common;

namespace server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly ILogger<ProductController> _logger;
        private readonly IProductRepository _productRepository;

        public ProductController(ILogger<ProductController> logger, IProductRepository productRepository)
        {
            _logger = logger;
            _productRepository = productRepository;
        }

        [HttpGet]
        public async Task<IEnumerable<Product>> Get(int timesec = 0)
        {
            await Task.Delay(1000 * timesec);
            return _productRepository.GetProducts();
        }
    }
}
