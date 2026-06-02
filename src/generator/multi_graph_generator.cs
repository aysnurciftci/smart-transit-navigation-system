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

            // 2. Rotaları Oluştur (MST + Mesafe Eşiği)
            graph.Routes = BuildRoutes(graph.Stations, distanceThreshold);

            // 3. Görselleştirme Aktifse Kavisleri Hesapla
            if (enableVisualBundling)
            {
                ApplyBundlingLogic(graph.Routes);
            }

            return graph;
        }

        private static List<Route> BuildRoutes(List<Station> stations, double threshold)
        {
            var routes = new List<Route>();
            var rand = new Random();

            // 1. Tüm olası bağlantıları mesafeleriyle listele
            var candidates = new List<Route>();
            for (int i = 0; i < stations.Count; i++)
            {
                for (int j = i + 1; j < stations.Count; j++)
                {
                    candidates.Add(new Route(stations[i], stations[j]));
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
                    // Cobweb prevention: Ensure this new express route doesn't cross existing routes
                    bool crosses = false;
                    foreach (var existing in routes)
                    {
                        // Aynı istasyona bağlanan hatlar kesişmez, sadece birleşir
                        if (existing.Source.Id == route.Source.Id || existing.Source.Id == route.Target.Id ||
                            existing.Target.Id == route.Source.Id || existing.Target.Id == route.Target.Id)
                            continue;

                        if (LinesIntersect(route.Source.X, route.Source.Y, route.Target.X, route.Target.Y,
                                        existing.Source.X, existing.Source.Y, existing.Target.X, existing.Target.Y))
                        {
                            crosses = true;
                            break;
                        }
                    }

                    if (!crosses)
                    {
                        routes.Add(route);
                    }
                }
            }
            return routes;
        }

        private static bool LinesIntersect(double x1,double y1, double x2,double y2, double x3,double y3, double x4,double y4)
        {
            // İki doğrunun kesişim testi (Cross Product / Orientation check)
            double d1 = Direction(x3,y3,x4,y4,x1,y1);
            double d2 = Direction(x3,y3,x4,y4,x2,y2);
            double d3 = Direction(x1,y1,x2,y2,x3,y3);
            double d4 = Direction(x1,y1,x2,y2,x4,y4);
            if (((d1 > 0 && d2 < 0) || (d1 < 0 && d2 > 0)) &&
                ((d3 > 0 && d4 < 0) || (d3 < 0 && d4 > 0))) return true;
            return false;
        }
        private static double Direction(double x1, double y1, double x2, double y2, double x3, double y3)
        {
            return (x3 - x1) * (y2 - y1) - (x2 - x1) * (y3 - y1);
        }
        private static void ApplyBundlingLogic(List<Route> routes)
        {
            int subdivisions = 8; // Her yolu kaç noktaya böleceğiz?
            int iterations = 60;  // Kümeleme sertliği
            double step = 0.1;

            // Her rotayı alt noktalara bölerek başlat
            foreach (var route in routes)
            {
                for (int i = 0; i <= subdivisions; i++)
                {
                    double t = (double)i / subdivisions;
                    double px = route.Source.X + (route.Target.X - route.Source.X) * t;
                    double py = route.Source.Y + (route.Target.Y - route.Source.Y) * t;

                    if (i > 0 && i < subdivisions) 
                    {
                        px += (new Random().NextDouble() - 0.5) * 8.0;
                        py += (new Random().NextDouble() - 0.5) * 8.0;
                    }

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

                        // Uzun rotaların çok daha sert kümelenmesi, kısa rotaların ise düz kalması için karesel ölçekleme
                        double distanceRatio = r1.Distance / 100.0;
                        double distanceScale = Math.Min(distanceRatio * distanceRatio, 15.0);   // Fizik patlamasın diye maks 15x limit


                        // 2. Çekim Kuvveti: Diğer rotaların yakın segmentlerini kendine çeker
                        foreach (var r2 in routes)
                        {
                            if (r1 == r2) continue;

                            double dx = r2.PathPoints[i].X - r1.PathPoints[i].X;
                            double dy = r2.PathPoints[i].Y - r1.PathPoints[i].Y;
                            double dist = Math.Sqrt(dx * dx + dy * dy);

                            if (dist < 40 && dist > 3.0) // Etki alanı içindeyse çek
                            {
                                forceX += (dx / dist) * distanceScale;
                                forceY += (dy / dist) * distanceScale;
                            }
                            else if (dist <= 3.0 && dist > 0.1) // Çok yakınlarsa hafifçe it ki paralel görünsünler
                            {
                            forceX -= (dx / dist) * 1.5 * distanceScale;
                            forceY -= (dy / dist) * 1.5 * distanceScale;
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
