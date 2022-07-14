namespace DurableFunctions.Data
{
    public class WeatherResponse
    {
        public Location location {get;set;}
        public Information current { get; set; }
    }
    public class Location
    {
        public string country {get;set;}
    }

    public class Information
    {
        public decimal temp_c { get; set; }
    }

}
