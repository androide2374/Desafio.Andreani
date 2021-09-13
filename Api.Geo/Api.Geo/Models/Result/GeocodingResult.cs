using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Geo.Models.Result
{
    public class GeocodingResult
    {
        public int Id { get; set; }
        public string Longitud { get; set; }
        public string Latitud { get; set; }
        public string Estado { get; set; }

        public GeocodingResult(int id, string lon, string lat, string estado)
        {
            Id = id;
            Longitud = lon;
            Latitud = lat;
            Estado = estado;
        }
    }
}
