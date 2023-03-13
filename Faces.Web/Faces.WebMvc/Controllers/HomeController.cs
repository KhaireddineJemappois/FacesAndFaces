using Faces.WebMvc.ViewModels;
using MassTransit;
using Messaging.InterfacesConstants.Commands;
using Messaging.InterfacesConstants.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Faces.WebMvc.Controllers
{
    public class HomeController : Controller
    {
        private readonly IBusControl _busControl;
        private readonly ILogger<HomeController> _logger;

        public HomeController(IBusControl busControl, ILogger<HomeController> logger)
        {
            _busControl = busControl;
            _logger = logger;
        }
        [HttpGet]
        public IActionResult RegisterOrder()
        {
            return View();
        } 
        public ActionResult Index()
        {
            return View();
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        [HttpPost]
        public async Task<IActionResult> RegisterOrder(OrderViewModel model)
        { 
            MemoryStream memory = new MemoryStream();
            using(var uploadFile=model.File.OpenReadStream())
            {
                await uploadFile.CopyToAsync(memory);
            }
            model.OrderId=Guid.NewGuid();
            model.ImageData=memory.ToArray();
            model.ImageUrl = model.File.FileName;
            var sendToUri = new Uri(@$"{RabbitMqMassTransitConstants.RabbitMquri}{RabbitMqMassTransitConstants.RegisterOrderCommandQueue}");
            var endPoint = await _busControl.GetSendEndpoint(sendToUri);
            await endPoint.Send<IRegisterOrderCommand>(
                new
                {
                    model.OrderId,
                    model.UserEmail,
                    model.ImageData,
                    model.ImageUrl

                }
                );
            ViewData["OrderId"] = model.OrderId;
            return View("Thanks");
                
        }
    }
}
