using Api.Geo.Context;
using Api.Geo.Models.Request;
using Api.Geo.Models.Result;
using Api.Geo.Models.Tables;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Api.Geo.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class GeolocalizarController : ControllerBase
    {

        private readonly AppDbContext _context;
        private readonly RabbitManagement _rabbitManagement;

        public GeolocalizarController(AppDbContext context, RabbitManagement rabbitManagement)
        {
            _rabbitManagement = rabbitManagement;
            _context = context;
        }

        [HttpPost]
        public async Task<ActionResult> Post(GeolocalizarRequest model)
        {
            Console.WriteLine($"{DateTime.Now} --- PedidoGeoLocalizar");
            try
            {
                PedidoGeo pedido = new PedidoGeo()
                {
                    Calle = model.Calle,
                    Ciudad = model.Ciudad,
                    CP = model.CP,
                    Numero = model.Numero,
                    Pais = model.Pais,
                    Provincia = model.Provincia
                };
                if (ModelState.IsValid)
                {
                    await _context.PedidoGeo.AddAsync(pedido);
                    await _context.SaveChangesAsync();

                    GeocodingResult result = new GeocodingResult(1, "123412", "123123", "asdasd");

                    Console.WriteLine($"{DateTime.Now} --- Pedido Guardado ---- {pedido.Id}");

                    _rabbitManagement.PublishMessage(pedido);
                    return StatusCode(202, new { id = pedido.Id });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return StatusCode(404, "Error");
            }
            return StatusCode(404, "Error");

        }
    }
}
