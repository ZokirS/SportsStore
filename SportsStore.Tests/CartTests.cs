using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using SportsStore.Models;
using SportsStore.Pages;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Routing;
namespace SportsStore.Tests
{
    public class CartTests
    {
        [Fact]
        public void Can_Add_New_Lines()
        {
            //Arrange - create some test products
            Product p1 = new Product { ProductID = 1, Name = "P1" };
            Product p2 = new Product { ProductID = 2, Name = "P2" };

            //Arrange - create a new cart
            Cart target = new Cart();

            //Act
            target.AddItem(p1, 1);
            target.AddItem(p2, 1);
            CartLine[] result = target.Lines.ToArray();

            //Assert
            Assert.Equal(2, result.Length);
            Assert.Equal(p1, result[0].Product);
            Assert.Equal(p2, result[1].Product);
        }

        [Fact]
        public void Can_Add_Quantity_For_Existing_Lines()
        {
            //Arrange - create some test products
            Product p1 = new Product { ProductID = 1, Name = "P1" };
            Product p2 = new Product { ProductID = 2, Name = "P2" };

            //Arraneg - create a new cart
            Cart target = new Cart();

            //Act
            target.AddItem(p1, 1);
            target.AddItem(p2, 1);
            target.AddItem(p1, 10);
            CartLine[] results = (target.Lines ?? new())
                .OrderBy(x=>x.Product.ProductID).ToArray();

            //Assert
            Assert.Equal(2, results.Length);
            Assert.Equal(11, results[0].Quantity);
            Assert.Equal(1, results[1].Quantity);
        }

        [Fact]
        public void Can_Remove_Line()
        {
            //Arrange - create some test products
            Product p1 = new Product { ProductID = 1, Name = "P1" };
            Product p2 = new Product { ProductID = 2, Name = "P2" };
            Product p3 = new Product { ProductID = 3, Name = "P3" };

            //Arraneg - create a new cart
            Cart target = new Cart();

            //Arrange - add some products to the cart
            target.AddItem(p1, 1);
            target.AddItem(p2, 3);
            target.AddItem(p3, 5);
            target.AddItem(p2, 1);

            //Act
            target.RemoveLine(p2);

            //Assert
            Assert.Empty(target.Lines.Where(x => x.Product == p2));
            Assert.Equal(2,target.Lines.Count);
        }

        [Fact]
        public void Calculate_Cart_Total()
        {
            //Arrange - create some test products
            Product p1 = new Product { ProductID = 1, Name = "P1", Price = 100M };
            Product p2 = new Product { ProductID = 2, Name = "P2", Price = 50M };

            //Arraneg - create a new cart
            Cart target = new Cart();

            //Act
            target.AddItem(p1, 1);
            target.AddItem(p2, 1);
            target.AddItem(p1, 3);
            decimal result = target.ComputeTotalValues();

            //Assert
            Assert.Equal(450M, result);
        }

        [Fact]
        public void Can_Clear_Contents()
        {
            //Arrange - create some test products
            Product p1 = new Product { ProductID = 1, Name = "P1", Price = 100M };
            Product p2 = new Product { ProductID = 2, Name = "P2", Price = 50M };

            //Arraneg - create a new cart
            Cart target = new Cart();

            //Arrange - add some items
            target.AddItem(p1, 1);
            target.AddItem(p2, 1);

            //Act - clear the cart
            target.Clear();

            //Assert
            Assert.Empty(target.Lines);
        }

        [Fact]
        public void Can_Load_Cart()
        {
            //Arrange 
            Product p1 = new Product { ProductID = 1, Name = "P1", Price = 100M };
            Product p2 = new Product { ProductID = 2, Name = "P2", Price = 50M };
            Mock<IStoreRepository> mock = new Mock<IStoreRepository>();
            mock.Setup(x=>x.Products).Returns((new Product[]
            {
                p1,p2
            }).AsQueryable());

            Cart testCart = new Cart();
            testCart.AddItem(p1, 2);
            testCart.AddItem(p2 , 1);

            Mock<ISession> mockSession = new Mock<ISession>();
            byte[] data = 
                Encoding.UTF8.GetBytes(JsonSerializer.Serialize(testCart));
            mockSession.Setup(x => x.TryGetValue(It.IsAny<string>(),
                out data));
            Mock<HttpContext> mockContext = new Mock<HttpContext>();
            mockContext.SetupGet(c=>c.Session).Returns(mockSession.Object);

            //Action
            CartModel cartModel = new CartModel(mock.Object)
            {
                PageContext = new PageContext(new ActionContext
                {
                    HttpContext = mockContext.Object,
                    RouteData = new RouteData(),
                    ActionDescriptor = new PageActionDescriptor()
                })
            };
            cartModel.OnGet("myUrl");

            //Assert
            Assert.Equal(2, cartModel.Cart?.Lines.Count);
            Assert.Equal("myUrl", cartModel.ReturnUrl);
        }

        [Fact]
        public void Can_Update_Cart()
        {
            // Arrange
            // - create a mock repository
            Mock<IStoreRepository> mockRepo = new Mock<IStoreRepository>();
            mockRepo.Setup(m => m.Products).Returns((new Product[] {
new Product { ProductID = 1, Name = "P1" }
}).AsQueryable<Product>());
            Cart? testCart = new Cart();
            Mock<ISession> mockSession = new Mock<ISession>();
            mockSession.Setup(s => s.Set(It.IsAny<string>(), It.IsAny<byte[]>()))
            .Callback<string, byte[]>((key, val) => {
                testCart =
    JsonSerializer.Deserialize<Cart>(Encoding.UTF8.
    GetString(val));
            });
            Mock<HttpContext> mockContext = new Mock<HttpContext>();
            mockContext.SetupGet(c => c.Session).Returns(mockSession.Object);
            // Action
            CartModel cartModel = new CartModel(mockRepo.Object)
            {
                PageContext = new PageContext(new ActionContext
                {
                    HttpContext = mockContext.Object,
                    RouteData = new RouteData(),
                    ActionDescriptor = new PageActionDescriptor()
                })
            };
            cartModel.OnPost(1, "myUrl");

            //Assert
            Assert.Single(testCart.Lines);
            Assert.Equal("P1", testCart.Lines.First().Product.Name);
            Assert.Equal(1, testCart.Lines.First().Quantity);
        }
    }
}
