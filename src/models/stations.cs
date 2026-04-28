/*
Burası İstasyon nesne class'ıdır. İstasyonun temelde x ve y konumu bilgileri vardır.

Buna ek olarak ileride konum bilgileri, ID, istasyon adı bilgileri
bir hash tablosu içinde saklanacaktır.
*/


namespace SmartTransit.Models
{
    public class Station
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double X { get; set; }
        public double Y { get; set; }

        public Station(int id, double x, double y)
        {
            Id = id;
            X = x;
            Y = y;
            Name = $"Station-{id}";
        }
    }
}
