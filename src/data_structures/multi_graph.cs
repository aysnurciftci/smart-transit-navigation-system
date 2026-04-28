/*
Bu kısım Multigraph veri yapısıdır. Multigraph, sadece İstasyonları
ve Rotaları tutan bir yapıdan ibarettir ve kendi içinde bunları işlevsel olarak
birleştirmez. Sadece depolama ve erişim amaçlıdır.
*/

using System.Collections.Generic;
using SmartTransit.Models;


namespace SmartTransit.MultiGraph
{
    public class TransitGraph
    {
        public List<Station> Stations { get; set; } = new List<Station>();
        public List<Route> Routes { get; set; } = new List<Route>();
        
        //herhangi bir istasyondan çıkan rotaları liste halinde veren fonksiyon
        //ileride sözlük ile değiştirmemiz gerekebilir çünkü O(n)
        public List<Route> GetOutgoingRoutes(Station station)
        {
            return Routes
                .Where(r => r.Source.Id == station.Id || r.Target.Id == station.Id)
                .ToList();
        }
    }
    
}
