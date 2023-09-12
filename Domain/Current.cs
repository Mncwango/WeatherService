namespace Domain
{
    public class Current
    {
        public string Observation_time { get; set; }
        public int Temperature { get; set; }
        public int Weather_code { get; set; }
        public string[] Weather_icons { get; set; }
        public string[] Weather_descriptions { get; set; }
        public int Wind_speed { get; set; }
        public int Wind_degree { get; set; }
        public string Wind_dir { get; set; }
        public int Pressure { get; set; }
        public int Precip { get; set; }
        public int Humidity { get; set; }
        public int Cloudcover { get; set; }
        public int Feelslike { get; set; }
        public int Uv_index { get; set; }
        public int Visibility { get; set; }
        public string Is_day { get; set; }
    }
}
