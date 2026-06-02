/*
Burası Rota nesne class'ıdır. Rota içinde temelde
çıktığı ve gittiği istasyonların bilgileri olacaktır.

Rota Uzunluğu, zamanı ve masrafı hash table olarak eklenmiştir.

Ayrıca TransitGraph'ın bundling = true ile oluşturulması durumunda tek çizgi
değil de 8 küçük çizgi olarak çizilmesi gerekecektir. Bu çizgiler PathPoints listesinde
nokta şeklinde olurlar.

*/

using System;
using System.Collections.Generic;
using SmartTransit.DataStructures;

namespace SmartTransit.Models
{
    public class Route
    {
        private HashTable<string, object> properties;

        public int Id 
        { 
            get => (int)properties["Id"]; 
            set => properties["Id"] = value; 
        }

        public Station Source 
        { 
            get => (Station)properties["Source"]; 
            set => properties["Source"] = value; 
        }

        public Station Target 
        { 
            get => (Station)properties["Target"]; 
            set => properties["Target"] = value; 
        }

        public double Distance 
        { 
            get => (double)properties["Distance"]; 
            set => properties["Distance"] = value; 
        }

        public double Time 
        { 
            get => (double)properties["Time"]; 
            set => properties["Time"] = value; 
        }

        public double Cost 
        { 
            get => (double)properties["Cost"]; 
            set => properties["Cost"] = value; 
        }
        
        // Edge Bundling için kavis noktaları
        // Görselleştirme kapalıysa bu liste boş kalır
        public List<(double X, double Y)> PathPoints { get; set; } = new List<(double X, double Y)>();

        public Route(int id, Station source, Station target, double distance, double time, double cost)
        {
            properties = new HashTable<string, object>();
            Id = id;
            Source = source;
            Target = target;
            Distance = distance;
            Time = time;
            Cost = cost;
        }
    }
}
