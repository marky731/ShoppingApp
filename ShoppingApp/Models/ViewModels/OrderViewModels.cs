using System.Collections.Generic;
using PagedList;
using ShoppingApp.Models.Domain;

namespace ShoppingApp.Models.ViewModels
{
    public class OrderListViewModel
    {
        public IPagedList<Order> Orders { get; set; }
        public OrderStatus? StatusFilter { get; set; }
    }

    public class OrderDetailsViewModel
    {
        public Order Order { get; set; }
        public IEnumerable<ShopOrder> ShopOrders { get; set; }
        public bool CanCancel { get; set; }
    }
}
