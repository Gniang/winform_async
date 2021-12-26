using System;

namespace winformasync.common
{

    public interface IProductRepository
    {
        Product[] GetProducts();
    }



    public class ProductRepositoryMock : IProductRepository
    {
        public Product[] GetProducts()
        {

            return CreateMockProducts();
        }


        private Product[] CreateMockProducts()
        {
            return new[]{
                new Product{ Id = 1, Name = "aaa", WeightTon = 123},
                new Product{ Id = 2, Name = "bbb", WeightTon = 456},
                new Product{ Id = 3, Name = "ccc", WeightTon = 789},
                new Product{ Id = 4, Name = "ddd", WeightTon = 012},
                new Product{ Id = 5, Name = "eee", WeightTon = 345},
            };
        }
    }

}