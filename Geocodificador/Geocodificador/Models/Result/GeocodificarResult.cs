using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Geocodificador.Models.Result
{
    public class GeocodificarResult
    {
        public int Id { get; set; }
        public string Longitud { get; set; }
        public string Latitud { get; set; }
        public string Estado { get; set; }
    }
}
