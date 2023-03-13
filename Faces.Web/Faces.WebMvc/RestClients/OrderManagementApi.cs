using Faces.WebMvc.ViewModels;
using Microsoft.Extensions.Configuration;
using Refit;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Faces.WebMvc.RestClients
{
    public class OrderManagementApi : IOrderManagementApi
    {
        private readonly IOrderManagementApi _restClient;

        public OrderManagementApi(IConfiguration config, HttpClient httpClient)
        {
            string apiHostAndPort = config.GetSection("ApiServiceLocations")
                .GetValue<string>("OrdersApiLocation");
            httpClient.BaseAddress = new Uri($"http://{apiHostAndPort}/api");
            _restClient = RestService.For<IOrderManagementApi>(httpClient);
        }

        public async Task<OrderViewModel> GetOrderById(Guid orderId)
        {
            try
            {
                return await _restClient.GetOrderById(orderId);
            }
            catch (ApiException ex)
            {
                if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }
                else
                {
                    throw;
                }
            }
        }

        public async Task<List<OrderViewModel>> GetOrders()
        {
            var orders= await _restClient.GetOrders();
            return orders;
        }
    }
}
