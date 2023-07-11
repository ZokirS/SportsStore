namespace SportsStore.Models
{
    public interface IOrderRepository
    {
        IQueryable<Order> Orders ();
        void SaveOrder(Order order);
    }
}
