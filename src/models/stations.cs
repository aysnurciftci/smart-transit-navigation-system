/*
Yazarlar: Ege Başaran, Bora Pektaş
Burası İstasyon nesne class'ıdır. İstasyonun temelde x ve y konumu bilgileri vardır.

Buna ek olarak ileride konum bilgileri, ID, istasyon adı bilgileri
bir hash tablosu içinde saklanmaktadır.
*/


using SmartTransit.DataStructures;

namespace SmartTransit.Models
{
    public class Station
    {
        private HashTable<string, object> properties;

        public int Id 
        { 
            get => (int)properties["Id"]; 
            set => properties["Id"] = value; 
        }

        public string Name 
        { 
            get => (string)properties["Name"]; 
            set => properties["Name"] = value; 
        }

        public double X 
        { 
            get => (double)properties["X"]; 
            set => properties["X"] = value; 
        }

        public double Y 
        { 
            get => (double)properties["Y"]; 
            set => properties["Y"] = value; 
        }

        public Station(int id, double x, double y)
        {
            properties = new HashTable<string, object>();
            Id = id;
            X = x;
            Y = y;
            Name = $"Station-{id}";
        }
    }
}
