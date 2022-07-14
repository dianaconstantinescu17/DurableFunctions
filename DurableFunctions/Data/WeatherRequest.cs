using System.Collections.Generic;

namespace DurableFunctions.Data
{
    public class WeatherRequest
    {
        public List<City> Cities { get; set; }
    }
    public class City
    {
        public string Name { get; set; }
    }
}
