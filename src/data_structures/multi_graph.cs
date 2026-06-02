/*
Bu kısım Multigraph veri yapısıdır. Multigraph, sadece İstasyonları
ve Rotaları tutan bir yapıdan ibarettir ve kendi içinde bunları işlevsel olarak
birleştirmez. Sadece depolama ve erişim amaçlıdır.
*/

using System.Collections.Generic;
using SmartTransit.Models;
using SmartTransit.DataStructures;

namespace SmartTransit.MultiGraph
{
    public class TransitGraph
    {
        public List<Station> Stations { get; set; } = new List<Station>();
        public List<Route> Routes { get; set; } = new List<Route>();
        
        // İstasyon ID'sini o istasyona bağlı rotaların listesine bağlayan Adjacency List
        public HashTable<int, List<Route>> AdjacencyList { get; set; }
        
        public void BuildAdjacencyList()
        {
            AdjacencyList = new HashTable<int, List<Route>>(Stations.Count * 2);
            
            foreach (var station in Stations)
            {
                AdjacencyList.Add(station.Id, new List<Route>());
            }
            
            foreach (var route in Routes)
            {
                AdjacencyList[route.Source.Id].Add(route);
                AdjacencyList[route.Target.Id].Add(route);
            }
        }

        // Herhangi bir istasyondan çıkan rotaları O(1) karmaşıklığında veren fonksiyon
        public List<Route> GetOutgoingRoutes(Station station)
        {
            if (AdjacencyList == null)
            {
                BuildAdjacencyList();
            }
            
            if (AdjacencyList.TryGetValue(station.Id, out var connectedRoutes))
            {
                return connectedRoutes;
            }
            
            return new List<Route>();
        }
    }
}
