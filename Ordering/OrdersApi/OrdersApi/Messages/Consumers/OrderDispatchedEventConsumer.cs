using MassTransit;
using Messaging.InterfacesConstants.Events;
using Microsoft.AspNetCore.SignalR;
using OrdersApi.Hubs;
using OrdersApi.Persistence;
using System;
using System.Threading.Tasks;

namespace OrdersApi.Messages.Consumers
{
    public class OrderDispatchedEventConsumer : IConsumer<IOrderDispatchedEvent>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IHubContext<OrderHub> _hubContext;
        public OrderDispatchedEventConsumer(IOrderRepository orderRepository, IHubContext<OrderHub> hubContext)
        {
            this._orderRepository = orderRepository;
            _hubContext = hubContext;
        }

        public async Task Consume(ConsumeContext<IOrderDispatchedEvent> context)
        {
            var message = context.Message;
            var orderId = message.OrderId;
            UpdateDatabase(orderId);

            await _hubContext.Clients.All.SendAsync("updateOrders",
             "Order Dispatched", orderId);
        }

        private void UpdateDatabase(Guid orderId)
        {
            var order = _orderRepository.GetOrder(orderId);
            if (order != null)
            {
                order.Status = Models.Status.Sent;
                _orderRepository.UpdateOrder(order);
            }
        }
    }
}
