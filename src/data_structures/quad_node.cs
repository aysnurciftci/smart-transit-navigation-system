/*
 * Bu sınıf, Quadtree (Dörtlü Ağaç) veri yapısının temel birimini temsil eden Düğüm (Node) sınıfıdır.
 * Haritayı (2D düzlemi) hiyerarşik olarak dört ana çeyreğe (Kuzeybatı, Kuzeydoğu, Güneybatı, Güneydoğu) 
 * bölerek durakların konumsal olarak saklanmasını sağlar.
 * * Temel amacı: Geniş bir alandaki durak aramalarını tüm listeyi taramak yerine 
 * sadece ilgili bölgeye odaklayarak O(n) karmaşıklığından O(log n) seviyesine indirmektir.
 */
using System.Collections.Generic;
using SmartTransit.Models;

namespace SmartTransit.DataStructures
{
    public class QuadNode
    {
        // Bölgenin sınırları
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }

        // Bir düğümün bölünüp bölünmediğini tutar
        public bool IsDivided { get; set; } = false;

        // Bu düğümdeki duraklar (Başlangıç kapasitesi dolana kadar burada tutulur)
        public List<Station> Stations { get; set; }

        // Alt bölmeler
        public QuadNode NorthWest, NorthEast, SouthWest, SouthEast;

        public QuadNode(double x, double y, double w, double h)
        {
            X = x; Y = y; Width = w; Height = h;
            Stations = new List<Station>();
        }
    }
}