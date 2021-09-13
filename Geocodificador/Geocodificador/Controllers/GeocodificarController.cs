using Geocodificador.Context;
using Geocodificador.Models.Result;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Geocodificador.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class GeocodificarController : ControllerBase
    {
        private readonly AppDbContext _context;
        public GeocodificarController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult> Get(int id)
        {
            var EstadoPedido =  _context.Geocodificar.FirstOrDefault(x=>x.IdPedido == id);

            if (EstadoPedido!= null)
            {
                GeocodificarResult result = new GeocodificarResult()
                {
                    Id = EstadoPedido.IdPedido,
                    Longitud = EstadoPedido.Longitud,
                    Latitud = EstadoPedido.Latitud,
                    Estado = EstadoPedido.Estado
                };
                return Ok(result);
            }
            return StatusCode(204);
        }
    }
}
