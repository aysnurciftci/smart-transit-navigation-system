/*
Burası Rota nesne class'ıdır. Rota içinde temelde
çıktığı ve gittiği istasyonların bilgileri olacaktır.

Bu bilgileri ile uzunluğu, süresi vb bilgileri de ileride olacaktır.

Ayrıca TransitGraph'ın bundling = true ile oluşturulması durumunda tek çizgi
değil de 8 küçük çizgi olarak çizilmesi gerekecektir. Bu çizgiler PathPoints listesinde
nokta şeklinde olurlar.

*/

using System.Collections.Generic;

namespace SmartTransit.Models
{
    public class Route
    {
        public Station Source { get; set; }
        public Station Target { get; set; }
        public double Distance { get; set; }
        
        // Edge Bundling için kavis noktaları
        // Görselleştirme kapalıysa bu liste boş kalır
        public List<(double X, double Y)> PathPoints { get; set; } = new List<(double X, double Y)>();

        public Route(Station source, Station target)
        {
            Source = source;
            Target = target;
            Distance = Math.Sqrt(Math.Pow(source.X - target.X, 2) + Math.Pow(source.Y - target.Y, 2));
        }
    }
}
