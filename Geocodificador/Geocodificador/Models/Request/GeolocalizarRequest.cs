using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Geocodificador.Models.Request
{
    public class GeolocalizarRequest
    {
        public GeolocalizarRequest()
        { 
        }
        public string Calle { get; set; }
        public int Numero { get; set; }
        public string Ciudad { get; set; }
        public string CP { get; set; }
        public string Provincia { get; set; }
        public string Pais { get; set; }
    }
}
