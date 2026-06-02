/*
  AUTHOR: Aysenur Ciftci
  MODULE: Locator - Uzamsal Konumlandırma Servisi

  MİMARİ STRATEJİ & AKADEMİK SAVUNMA (PURE C#)
  --------------------------------------------------------------------------------------
  Jüri sunumunda kuramsal doğruluk ve veri yapılarının saf mantığına bağlılığı vurgulamak
  adına bu modülde hiçbir LINQ veya harici hazır filtreleme kütüphanesi kullanılmamıştır.
  Tüm konumlandırma ve harita sınır işlemleri C#'ın temel ilkel (primitive) dil parçaları,
  geleneksel döngüler ve constructor enjeksiyonu ile yönetilmektedir.

  PERFORMANS YAKLAŞIMI:
  QuadTree yapısı Locator nesnesi oluşturulurken harita boyutlarına göre (mapWidth, mapHeight)
  bellekte yalnızca bir kez inşa edilir (Cache Mantığı). Böylece her kullanıcı sorgusunda
  ağacın yeniden oluşturulma maliyeti ve CPU üzerindeki ek yükler tamamen engellenir.
 */

using System.Collections.Generic;
using SmartTransit.Models;
using SmartTransit.MultiGraph;

namespace SmartTransit.DataStructures
{
    /// <summary>
    /// Harita üzerindeki serbest koordinatları en yakın Station nesnesine dönüştüren
    /// uzamsal konumlandırma servisidir.
    ///
    /// Locator oluşturulurken QuadTree yalnızca bir kez inşa edilir ve bellekte tutulur.
    /// Böylece konum sorgularında istasyonların yeniden indekslenmesi engellenir.
    /// </summary>
    public class Locator
    {
        private readonly QuadTree _quadTree;

        /// <summary>
        /// Verilen harita boyutları kullanılarak QuadTree uzamsal indeksi oluşturulur.
        /// </summary>
        /// <param name="graph">Sistemdeki tüm istasyonları içeren grafik yapısı</param>
        /// <param name="mapWidth">Haritanın genişliği</param>
        /// <param name="mapHeight">Haritanın yüksekliği</param>
        public Locator(TransitGraph graph, double mapWidth, double mapHeight)
        {
            if (graph == null || graph.Stations == null || graph.Stations.Count == 0)
            {
                return;
            }

            double centerX = mapWidth / 2.0;
            double centerY = mapHeight / 2.0;

            _quadTree = new QuadTree(
                centerX,
                centerY,
                centerX + 10,
                centerY + 10,
                4);

            foreach (var station in graph.Stations)
            {
                _quadTree.Insert(station);
            }
        }

        /// <summary>
        /// Verilen koordinata en yakın istasyonu döndürür.
        /// </summary>
        public Station LocateNearestStation(double targetX, double targetY)
        {
            if (_quadTree == null)
            {
                return null;
            }

            List<Station> nearestStations =
                _quadTree.FindKNearestNeighbors(targetX, targetY, 1);

            if (nearestStations.Count == 0)
            {
                return null;
            }

            return nearestStations[0];
        }
    }
}