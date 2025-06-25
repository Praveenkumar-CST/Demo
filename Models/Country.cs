namespace WiseHR.Models
{
    public class Country
    {
        public string Iso2 { get; set; }
        public string Name { get; set; }
    }
    public class State
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Iso2 { get; set; }
    }

    public class City
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
