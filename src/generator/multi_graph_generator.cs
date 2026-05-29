/*
Bu kısım Graph oluşturan classı ve metodları içerir. 5 girdisi vardır:
1-) İstasyon sayısı
2-) Maksimum X mesafesi (haritanın yatay ekseni)
3-) Maksimum Y mesafesi (haritanın dikey ekseni)
4-) Rotaların maksimum uzunluğu
5-) Visual Bundling açık-kapalı. Visual bundling açıksa rotalar görsel anlamda
kavisli olur. Sadece görsel bir fark.

Algoritmanın özellikleri:

"Box-Muller Transform" ile istasyonlar tam rastgele değil, normal dağılım
ile dağılırlar ve merkezde daha çok, kenarlarda daha az istasyon olur.

"Euclidean Minimum Spanning Tree" Ağacı ile bağımsız istasyon bulundurmayan
bir ağaç oluşur.

"Force-Directed Edge Bundling" Bu kısım tamamen kozmetik bir kısımdır. Metro
hattı haritalarında metrolar birbirine paralel ve yakın, kavisli gider ya, onu
katıyor. Bağlantılar aynı ama kavisli şekli oluyor. Hem görselleştirmede katkı sağlar
hemde multigraphtaki aynı yerlere giden rotaları ayırt etmeye kullanılabilir.
Rotaları belli parçalardaki çizgilere bölüp bu çizgileri rota nesnesine koyuyor.
Bu şekilde kavisli rotayı tam olarak çizebilir yada üzerinden otobüs vs yürütebilirsiniz.

Aynı istasyonlar arası birden fazla rota oluşturulmaktadır, bu rotaların sayısı
n (istasyon sayısı) tipinden n*DuplicateMax ile n*DuplicateMin arasında rastgele bir sayı olacaktır.

Rota uzunluğu, x,y verilerine göre, zaman ve masraf ise min-max parametrelerinin arasında rastgele
bir sayı olarak belirlenmektedir.

Son olarak hash table içinde bütün istasyonların sahip olduğu rotalar hesaplanır. Bu şekilde bir
istasyona bağlı bütün rotaları O(1) zamanda erişebilirsiniz.

Sonuç olarak bir TransitGraph nesnesi döndürür. Bunu herhangi bir TransitGraph referansına
atıp Graphınız ile istediğinizi yapabilirsiniz.
*/




using System;
using System.Collections.Generic;
using System.Linq;
using SmartTransit.Models;
using SmartTransit.MultiGraph;


namespace SmartTransit.Generator
{
    public static class GraphGenerator
    {
        // Compile time parameters for duplicates
        private const double DuplicateMin = 0.1;
        private const double DuplicateMax = 0.3;

        // Compile time parameters for random route attributes
        private const double MinTime = 10.0;
        private const double MaxTime = 120.0;
        private const double MinCost = 2.0;
        private const double MaxCost = 50.0;

        public static TransitGraph CreateFullGraph(
            int stationCount, 
            double maxX, 
            double maxY, 
            double distanceThreshold, 
            bool enableVisualBundling)
        {
            var graph = new TransitGraph();
            var rand = new Random();

            // 1. İstasyonları Oluştur (Normal Dağılım)
            for (int i = 0; i < stationCount; i++)
            {
                double u1 = 1.0 - rand.NextDouble();
                double u2 = 1.0 - rand.NextDouble();
                double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
                
                double x = (maxX / 2) + (maxX / 6) * randStdNormal;
                double y = (maxY / 2) + (maxY / 6) * (Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2));
                
                graph.Stations.Add(new Station(i, x, y));
            }

            // 2. Rotaları Oluştur (MST + Mesafe Eşiği + Duplicates)
            graph.Routes = BuildRoutes(graph.Stations, distanceThreshold);

            // 3. Görselleştirme Aktifse Kavisleri Hesapla
            if (enableVisualBundling)
            {
                ApplyBundlingLogic(graph.Routes);
            }

            // 4. İstasyonların bağlı olduğu rotaları O(1) erişim için hesapla ve kaydet
            graph.BuildAdjacencyList();

            return graph;
        }

        private static Route CreateRandomRoute(int id, Station source, Station target, Random rand)
        {
            double distance = Math.Sqrt(Math.Pow(source.X - target.X, 2) + Math.Pow(source.Y - target.Y, 2));
            double time = MinTime + (rand.NextDouble() * (MaxTime - MinTime));
            double cost = MinCost + (rand.NextDouble() * (MaxCost - MinCost));
            return new Route(id, source, target, distance, time, cost);
        }

        private static List<Route> BuildRoutes(List<Station> stations, double threshold)
        {
            var routes = new List<Route>();
            var rand = new Random();
            int routeIdCounter = 1;

            // 1. Tüm olası bağlantıları listele (Tamamen rastgele mesafe, zaman, maliyet)
            var candidates = new List<Route>();
            for (int i = 0; i < stations.Count; i++)
            {
                for (int j = i + 1; j < stations.Count; j++)
                {
                    candidates.Add(CreateRandomRoute(routeIdCounter++, stations[i], stations[j], rand));
                }
            }

            // Mesafeye göre küçükten büyüğe sırala (Kruskal hazırlığı)
            var sortedCandidates = candidates.OrderBy(r => r.Distance).ToList();

            // MST için Union-Find yapısı
            int[] parent = Enumerable.Range(0, stations.Count).ToArray();
            int Find(int i) => parent[i] == i ? i : parent[i] = Find(parent[i]);

            foreach (var route in sortedCandidates)
            {
                int rootSource = Find(route.Source.Id);
                int rootTarget = Find(route.Target.Id);

                if (rootSource != rootTarget)
                {
                    // Bu rota MST'nin bir parçası (Bağlantı garantisi)
                    routes.Add(route);
                    parent[rootSource] = rootTarget;
                }
                else if (route.Distance < threshold && rand.NextDouble() > 0.9)
                {
                    // Zaten bağlılar ama birbirlerine yakınlar, 
                    // %10 ihtimalle alternatif bir "ekspres hat" ekle.
                    routes.Add(route);
                }
            }

            // 2. Multigraph için rastgele kopya rotalar oluştur
            int n = routes.Count;
            int numDuplicates = (int)(n * (DuplicateMin + rand.NextDouble() * (DuplicateMax - DuplicateMin)));

            for (int i = 0; i < numDuplicates; i++)
            {
                if (n == 0) break;
                // Rastgele var olan bir bağlantıyı seç
                var baseRoute = routes[rand.Next(n)];
                // Yeni bir ID ve rastgele parametrelerle aynı iki istasyon arasına yeni rota oluştur
                routes.Add(CreateRandomRoute(routeIdCounter++, baseRoute.Source, baseRoute.Target, rand));
            }

            return routes;
        }

        private static void ApplyBundlingLogic(List<Route> routes)
        {
            int subdivisions = 8; // Her yolu kaç noktaya böleceğiz?
            int iterations = 40;  // Kümeleme sertliği
            double step = 0.1;

            // Her rotayı alt noktalara bölerek başlat
            foreach (var route in routes)
            {
                for (int i = 0; i <= subdivisions; i++)
                {
                    double t = (double)i / subdivisions;
                    double px = route.Source.X + (route.Target.X - route.Source.X) * t;
                    double py = route.Source.Y + (route.Target.Y - route.Source.Y) * t;
                    route.PathPoints.Add((px, py));
                }
            }

            // Kuvvet simülasyonu (Force-Directed)
            for (int iter = 0; iter < iterations; iter++)
            {
                foreach (var r1 in routes)
                {
                    // Sadece iç noktaları hareket ettir (Uçlar -İstasyonlar- sabit kalmalı)
                    for (int i = 1; i < subdivisions; i++)
                    {
                        double forceX = 0, forceY = 0;

                        // 1. Yay Kuvveti: Kendi segmentini düz tutmaya çalışır
                        forceX += (r1.PathPoints[i - 1].X + r1.PathPoints[i + 1].X - 2 * r1.PathPoints[i].X);
                        forceY += (r1.PathPoints[i - 1].Y + r1.PathPoints[i + 1].Y - 2 * r1.PathPoints[i].Y);

                        // 2. Çekim Kuvveti: Diğer rotaların yakın segmentlerini kendine çeker
                        foreach (var r2 in routes)
                        {
                            if (r1 == r2) continue;

                            double dx = r2.PathPoints[i].X - r1.PathPoints[i].X;
                            double dy = r2.PathPoints[i].Y - r1.PathPoints[i].Y;
                            double dist = Math.Sqrt(dx * dx + dy * dy);

                            if (dist < 40 && dist > 0.5) // Etki alanı içindeyse çek
                            {
                                forceX += dx / dist;
                                forceY += dy / dist;
                            }
                        }

                        // Noktayı yeni konumuna güncelle (Tuple'lar immutable olduğu için yeniden ata)
                        var current = r1.PathPoints[i];
                        r1.PathPoints[i] = (current.X + forceX * step, current.Y + forceY * step);
                    }
                }
            }
        }
    }
}
