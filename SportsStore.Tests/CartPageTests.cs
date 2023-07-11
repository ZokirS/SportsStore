using Moq;
using SportsStore.Models;
using SportsStore.Pages;

namespace SportsStore.Tests
{
    public class CartPageTests
    {
        [Fact]
        public void Can_Load_Cart()
        {
            //Arrange - create a mock repository
            Product p1 = new Product { ProductID = 1, Name = "P1" };
            Product p2 = new Product { ProductID = 2, Name = "P2" };
            Mock<IStoreRepository> mock = new Mock<IStoreRepository>();
            mock.Setup(m => m.Products).Returns((new Product[]
            {
                p1,
                p2
            }).AsQueryable<Product>());

            Cart testCart = new Cart();
            testCart.AddItem(p1, 2);
            testCart.AddItem(p2, 1);

            //Action
            CartModel cart = new CartModel(mock.Object, testCart);
            cart.OnGet("myUrl");

            //Assert
            Assert.Equal(2, cart.Cart.Lines.Count);
            Assert.Equal("myUrl", cart.ReturnUrl);
        }

        [Fact]
        public void Can_Update_Cart()
        {
            //Arrange
            Mock<IStoreRepository> mockRepo = new Mock<IStoreRepository>();
            mockRepo.Setup(m => m.Products).Returns((new Product[]
            {
                new Product { ProductID = 1, Name = "P1"}
            }).AsQueryable<Product>);

            Cart testCart = new Cart();

            //Action
            CartModel cartModel = new CartModel(mockRepo.Object, testCart);
            cartModel.OnPost(1, "myUrl");

            //Assert
            Assert.Single(testCart.Lines);
            Assert.Equal("P1", testCart.Lines.First().Product.Name);
            Assert.Equal(1, testCart.Lines.First().Quantity);
        }
    }
}
