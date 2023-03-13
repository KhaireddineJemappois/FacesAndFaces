using MassTransit;
using Messaging.InterfacesConstants.Commands;
using Messaging.InterfacesConstants.Events;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using OrdersApi.Hubs;
using OrdersApi.Models;
using OrdersApi.Persistence;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace OrdersApi.Messages.Consumers
{
    public class RegisterOrderCommandConsumer : IConsumer<IRegisterOrderCommand>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHubContext<OrderHub> _hubContext;

        public RegisterOrderCommandConsumer(IOrderRepository orderRepository, IHttpClientFactory httpClientFactory, IHubContext<OrderHub> hubContext)
        {
            _orderRepository = orderRepository;
            _httpClientFactory = httpClientFactory;
            _hubContext = hubContext;
        }

        public async Task Consume(ConsumeContext<IRegisterOrderCommand> context)
        {
            var result = context.Message;
            if (result.OrderId!=Guid.Empty && result.UserEmail != null
                && result.ImageData != null)
            {
                SaveOrder(result);
                await _hubContext.Clients.All.SendAsync("updateOrders","New Order Created",result.OrderId);
                var client = _httpClientFactory.CreateClient();
                Tuple<List<byte[]>,Guid> orderDetailData = await GetFacesFromApiAsync(client, result.ImageData, result.OrderId);
                var faces = orderDetailData.Item1;
                var orderId=orderDetailData.Item2;
                SaveOrderDetail(orderId,faces);
                await _hubContext.Clients.All.SendAsync("updateOrders", "New Processed", result.OrderId);
                await context.Publish<IOrderProcessedEvent>(new
                {
                    OrderId = orderId,
                    result.UserEmail,
                    Faces = faces,
                    result.PictureUrl
                });
            } 
        }

        private void SaveOrderDetail(Guid orderId, List<byte[]> faces)
        {
            var order=_orderRepository.GetOrderAsync(orderId).Result;
            if(order != null)
            {
                order.Status = Status.Processed;
                foreach(var face in faces)
                {
                    var orderDetail = new OrderDetail
                    {
                        OrderId = orderId,
                        FaceData = face
                    };
                    order.OrderDetails.Add(orderDetail);
                }
                _orderRepository.UpdateOrder(order);
                
            }
        }

        private async Task<Tuple<List<byte[]>, Guid>> GetFacesFromApiAsync(HttpClient client, byte[] imageData, Guid orderId)
        {
            var byteContent=new ByteArrayContent(imageData);
            Tuple<List<byte[]>, Guid> orderDetailData = null;
            byteContent.Headers.ContentType=new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
            using (var response = await client.PostAsync("http://localhost:6001/api/faces?orderId=" + orderId, byteContent))
            {
                var apiResponse=await response.Content.ReadAsStringAsync();
                orderDetailData = JsonConvert.DeserializeObject<Tuple<List<byte[]>, Guid>>(apiResponse);
            }
            return orderDetailData;
        }

        private void SaveOrder(IRegisterOrderCommand result)
        {
            var order = new Order
            {
                ImageData = result.ImageData,
                OrderId = result.OrderId,
                PictureUrl = result.PictureUrl,
                Status = Status.Registered,
                UserEmail = result.UserEmail,
            };
            _orderRepository.RegisterOrder(order);
        }
    }
}
