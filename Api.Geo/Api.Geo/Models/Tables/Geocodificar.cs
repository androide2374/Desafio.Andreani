using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Geo.Models.Tables
{
    public partial class Geocodificar
    {
        public int Id { get; set; }
        public int IdPedido { get; set; }
        public string Latitud { get; set; }
        public string Longitud { get; set; }
        public string Estado { get; set; }
        public PedidoGeo PedidoGeo { get; set; }
    }
}
