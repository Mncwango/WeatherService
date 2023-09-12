namespace Domain
{
    public class Location
    {
        public string Name { get; set; }
        public string Country { get; set; }
        public string Region { get; set; }
        public string Lat { get; set; }
        public string Lon { get; set; }
        public string Timezone_id { get; set; }
        public string Localtime { get; set; }
        public int Localtime_epoch { get; set; }
        public string Utc_offset { get; set; }
    }
}
