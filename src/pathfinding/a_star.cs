/*
Yazarlar: Bora Pektaş
A* Yol bulma algoritması, sadece mesafe ile çalışır, masraf ve süreye göre hesaplayamaz.
Sezgisi x,y'ye göre kuş uçuşu mesafedir.

*/

using System;
using System.Collections.Generic;
using SmartTransit.Models;
using SmartTransit.MultiGraph;
using SmartTransit.DataStructures;

namespace SmartTransit.Pathfinding
{
    public class AStar
    {//Bora Pektas
        private TransitGraph _graph;

        public AStar(TransitGraph graph)
        {
            _graph = graph;
        }

        // Sezgi fonksiyonu: Kuş uçumu mesafe
        private double Heuristic(Station a, Station b)
        {//Bora Pektas
            return Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
        }

        // Başlangıç istasyonundan son istasyona bulunan rota
        public (List<Route> Path, double TotalDistance) FindPath(Station start, Station goal)
        {//Bora Pektas
            var openSet = new PriorityQueue();
            openSet.Enqueue(0, start);

            //Seçilen rotaların listesi, sonuçta bulunan yolu tutar
            var cameFrom = new HashTable<int, Route>(_graph.Stations.Count);
            //Başlangıçtan bütün istasyonlara olan düz mesafeler
            var gScore = new HashTable<int, double>(_graph.Stations.Count);
            //G skoru ve sezgi toplamı listesi
            var fScore = new HashTable<int, double>(_graph.Stations.Count);

            //Skorlama başlatma
            foreach (var station in _graph.Stations)
            {
            	//Sonsuz ile başlar
                gScore.Add(station.Id, double.PositiveInfinity);
                fScore.Add(station.Id, double.PositiveInfinity);
            }
            gScore[start.Id] = 0;
            fScore[start.Id] = Heuristic(start, goal);

            // Döngü oluşturmamak için gidilen yerleri kaydediyoruz
            var closedSet = new HashTable<int, bool>(_graph.Stations.Count);

            while (!openSet.IsEmpty())
            {
                Station current = openSet.Dequeue();

                if (current.Id == goal.Id)
                {
                    return ReconstructPath(cameFrom, current, gScore[current.Id]);
                }

                if (closedSet.ContainsKey(current.Id))
                {
                    continue; // İkinci rota, istasyona daha kısa mesafeli bir yol olduğundan geçiliyor
                }
                closedSet.Add(current.Id, true);

                // İstasyona bağlı rotalar bulunur
                List<Route> outgoingRoutes = _graph.GetOutgoingRoutes(current);

                foreach (var route in outgoingRoutes)
                {
                    // Komşu istasyonlar bulunur
                    Station neighbor = route.Source.Id == current.Id ? route.Target : route.Source;

                    if (closedSet.ContainsKey(neighbor.Id))
                    {
                        continue; //Zaten işlenmişse geç
                    }

                    double tentativeGScore = gScore[current.Id] + route.Distance;

                    if (tentativeGScore < gScore[neighbor.Id])
                    {
                        // Daha iyi bir yol bulunduğunda yolu değiştir
                        cameFrom[neighbor.Id] = route;
                        gScore[neighbor.Id] = tentativeGScore;
                        
                        double f = tentativeGScore + Heuristic(neighbor, goal);
                        fScore[neighbor.Id] = f;

                        // Komşuyu yeni öncelik ile kuyruğa ekler
                        // Öncelikli kuyruk en az öncelikli olanı pop'lar
                        openSet.Enqueue(f, neighbor);
                    }
                }
            }

            // Yol yok
            return (new List<Route>(), double.PositiveInfinity);
        }
        //Bulunan yolu return'lemeye hazır hale getirme
        private (List<Route>, double) ReconstructPath(HashTable<int, Route> cameFrom, Station current, double totalDistance)
        {//Bora Pektas
            var path = new List<Route>();
            
            while (cameFrom.ContainsKey(current.Id))
            {
                Route route = cameFrom[current.Id];
                path.Add(route);
                
                // Baştan sona giderek ters yol bulunur
                current = route.Source.Id == current.Id ? route.Target : route.Source;
            }
            
            path.Reverse(); // Sondan başayı tersine çevirme
            return (path, totalDistance);
        }
    }
}
