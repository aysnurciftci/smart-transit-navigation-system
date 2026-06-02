/*
  AUTHOR: Aysenur Ciftci
  MODULE: Locator - Dinamik Uzamsal Konumlandirma Servisi

  MİMARİ TASARIM
  --------------------------------------------------------------------------------------
  Bu sınıf, harita üzerindeki serbest koordinatların (X, Y), A* ve Dijkstra gibi
  düğüm tabanlı yol bulma algoritmalarının işleyebileceği 'Station' nesnelerine
  dönüştürülmesini sağlar.

  Harita arayüzünden gelen koordinatlar doğrudan grafik algoritmaları tarafından
  kullanılamaz. Bu nedenle Locator katmanı, QuadTree tabanlı uzamsal indeksleme
  mekanizmasını kullanarak ilgili noktaya en yakın istasyonu tespit eder ve
  algoritmalara uygun giriş verisi üretir.

  DİNAMİK SINIR OPTİMİZASYONU
  --------------------------------------------------------------------------------------
  Sabit harita boyutları yerine, TransitGraph içerisindeki istasyonların gerçek
  koordinatlarından (Min/Max X ve Y) dinamik olarak bir kapsayıcı alan
  (Bounding Box) hesaplanır.

  Böylece harita ölçeği değişse dahi QuadTree yapısı tüm istasyonları kapsayacak
  şekilde otomatik olarak oluşturulur ve farklı veri kümelerine uyum sağlar.

  PERFORMANS YAKLAŞIMI
  --------------------------------------------------------------------------------------
  QuadTree yapısı Locator oluşturulurken yalnızca bir kez inşa edilir ve bellekte
  tutulur (Cache Mantığı).

  Böylece her kullanıcı sorgusunda ağacın yeniden oluşturulması ve tüm
  istasyonların tekrar eklenmesi engellenir. Harita üzerindeki konum sorguları
  doğrudan hazır uzamsal indeks üzerinden gerçekleştirilir.
*/

using System.Linq;
using SmartTransit.Models;
using SmartTransit.MultiGraph;

namespace SmartTransit.DataStructures
{
    public class Locator
    {
        // Bellekte tutulan uzamsal indeks
        private readonly QuadTree _quadTree;

        /// <summary>
        /// TransitGraph içerisindeki istasyonlardan dinamik sınırlar hesaplayarak
        /// QuadTree indeksini oluşturur.
        /// </summary>
        /// <param name="graph">
        /// Sistemdeki tüm istasyon ve rota bilgilerini içeren TransitGraph nesnesi.
        /// </param>
        public Locator(TransitGraph graph)
        {
            if (graph == null || graph.Stations.Count == 0)
            {
                return;
            }

            // İstasyon koordinatlarından kapsayıcı alanın sınırları hesaplanır
            double minX = graph.Stations.Min(s => s.X);
            double maxX = graph.Stations.Max(s => s.X);

            double minY = graph.Stations.Min(s => s.Y);
            double maxY = graph.Stations.Max(s => s.Y);

            // Kapsayıcı alanın merkezi belirlenir
            double centerX = (minX + maxX) / 2.0;
            double centerY = (minY + maxY) / 2.0;

            // Sınırdaki istasyonların dışarıda kalmaması için güvenlik payı eklenir
            double halfWidth = ((maxX - minX) / 2.0) + 10;
            double halfHeight = ((maxY - minY) / 2.0) + 10;

            // Dinamik QuadTree oluşturulur
            _quadTree = new QuadTree(
                centerX,
                centerY,
                halfWidth,
                halfHeight,
                capacity: 4);

            // Tüm istasyonlar yalnızca bir kez uzamsal indekse yüklenir
            foreach (var station in graph.Stations)
            {
                _quadTree.Insert(station);
            }
        }

        /// <summary>
        /// Verilen koordinata en yakın istasyonu QuadTree tabanlı
        /// uzamsal indeks üzerinden bulur.
        /// </summary>
        /// <param name="targetX">Hedef noktanın X koordinatı</param>
        /// <param name="targetY">Hedef noktanın Y koordinatı</param>
        /// <returns>
        /// En yakın Station nesnesi; indeks oluşturulamamışsa null.
        /// </returns>
        public Station LocateNearestStation(double targetX, double targetY)
        {
            if (_quadTree == null)
            {
                return null;
            }

            var nearestStations =
                _quadTree.FindKNearestNeighbors(targetX, targetY, 1);

            return nearestStations.FirstOrDefault();
        }
    }
}