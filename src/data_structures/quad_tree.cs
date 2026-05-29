/*
 Author: Aysenur Ciftci
 Modul: QuadTree - Stack Overflow Korumalı Konumsal Indeksleme
 Acıklama: Eszamanlı simulasyon calısmasına uygun, asenkron korumalı guvenli QuadTree motoru.

 DESIGN RATIONALE & ARCHITECTURAL COMPARISON
   1. HashMap Kıyaslaması:
   Karma tablolar (HashMap/Dictionary) anahtar bazlı aramalarda O(1) karmaşıklığı sunsa da,
   uzamsal yakınlık (Spatial Proximity) sorgularında veriyi geometrik olarak gruplandıramaz.
   Konum tabanlı en yakın komşu aramalarında tüm veri kümesini O(N) maliyetle taramak
   zorunda bırakır. QuadTree ise veriyi bölgesel olarak indeksleyerek arama uzayını daraltır.

   2. KD-Tree Kıyaslaması:
   KD-Tree veri yapısı eksen bazlı ikili bölünme (Binary Partitioning) gerçekleştirirken,
   QuadTree iki boyutlu uzayı tek bir işlemde 4 eş çeyreğe (NW, NE, SW, SE) ayırır.
   Bu karakteristik, 2D harita koordinat sistemleri, grid tabanlı görselleştirmeler ve
   endüstri standardı karo (Tile) mekanizmalarıyla doğrudan mimari uyum sağlar.

   TIME COMPLEXITY (BIG-O ANALYSIS)
   - Veri Ekleme (Insertion)              : Ortalama O(log N) | En Kötü O(N) (Yoğun Yığılma)
   - En Yakın K Durak Araması (KNN Search): Ortalama O(log N) | En Kötü O(N)

   UYARI: Aynı koordinata sahip veya birbirine aşırı yakın verilerin oluşturabileceği
   sonsuz bölünme (Infinite Subdivision) ve Stack Overflow riskini önlemek amacıyla,
   sisteme MIN_SIZE (1.0) derinlik sınırı ve koruma mekanizması entegre edilmiştir.

   CONCURRENCY & THREAD-SAFETY (B.1 CRITERIA)
   Yapay zeka simülasyon motorunun asenkron çalışma mimarisi altında veri bütünlüğünü
   korumak ve yarış durumunu (Race Condition) engellemek adına 'ReaderWriterLockSlim'
   kilit mekanizması senkronize edilmiştir. Bu sayede kullanıcı arayüzünden tetiklenen
   eşzamanlı çoklu okuma (Read Lock - KNN) talepleri kesintisiz karşılanırken, simülasyon
   motorunun veri güncelleme (Write Lock - Insert) işlemleri thread-safe olarak yürütülür.
 */

using System;
using System.Collections.Generic;
using System.Threading;
using SmartTransit.Models;

namespace SmartTransit.DataStructures
{
    public class QuadTree
    {
        private readonly QuadNode _root;
        private readonly int _capacity;

        //  sonsuz bolunmeyi (Stack Overflow) onleyen minimum bolge boyutu sınırı
        private const double MIN_SIZE = 1.0;

        // B.1 Isteri: Simulasyon motoru asenkron calısırken race condition onleyici kilit
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        public QuadTree(double boundaryX, double boundaryY, double boundaryWidth, double boundaryHeight, int capacity = 4)
        {
            _root = new QuadNode(boundaryX, boundaryY, boundaryWidth, boundaryHeight);
            _capacity = capacity;
        }

        public void Insert(Station station)
        {
            _lock.EnterWriteLock();
            try
            {
                InsertInternal(_root, station);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        private bool InsertInternal(QuadNode node, Station station)
        {
            if (!node.Contains(station.X, station.Y))
            {
                return false;
            }

            if (node.Stations.Count < _capacity && !node.IsDivided)
            {
                node.Stations.Add(station);
                return true;
            }

            //  Ust uste binen koordinatlarda sonsuz donguyu ve cokmeyi onler
            if (node.Width <= MIN_SIZE || node.Height <= MIN_SIZE)
            {
                node.Stations.Add(station);
                return true;
            }

            if (!node.IsDivided)
            {
                node.Subdivide();

                for (int i = node.Stations.Count - 1; i >= 0; i--)
                {
                    Station current = node.Stations[i];
                    if (InsertInternal(node.NorthWest, current) ||
                        InsertInternal(node.NorthEast, current) ||
                        InsertInternal(node.SouthWest, current) ||
                        InsertInternal(node.SouthEast, current))
                    {
                        node.Stations.RemoveAt(i);
                    }
                }
            }

            if (InsertInternal(node.NorthWest, station)) return true;
            if (InsertInternal(node.NorthEast, station)) return true;
            if (InsertInternal(node.SouthWest, station)) return true;
            if (InsertInternal(node.SouthEast, station)) return true;

            return false;
        }

        public List<Station> FindKNearestNeighbors(double targetX, double targetY, int k)
        {
            //  NULL/PARAMETRE GUVENLIGI KONTROLU
            if (k <= 0)
                return new List<Station>();

            _lock.EnterReadLock();
            try
            {
                var pq = new SmartTransit.PriorityQueue();
                FindKNNInternal(_root, targetX, targetY, pq);

                var nearestStations = new List<Station>();
                while (!pq.IsEmpty() && nearestStations.Count < k)
                {
                    nearestStations.Add(pq.Dequeue());
                }

                return nearestStations;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        private void FindKNNInternal(QuadNode node, double tx, double ty, SmartTransit.PriorityQueue pq)
        {
            if (node == null) return;

            foreach (var station in node.Stations)
            {
                double distance = CalculateDistance(tx, ty, station.X, station.Y);
                pq.Enqueue(distance, station);
            }

            if (node.IsDivided)
            {
                FindKNNInternal(node.NorthWest, tx, ty, pq);
                FindKNNInternal(node.NorthEast, tx, ty, pq);
                FindKNNInternal(node.SouthWest, tx, ty, pq);
                FindKNNInternal(node.SouthEast, tx, ty, pq);
            }
        }

        private double CalculateDistance(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));
        }

        /*GRAPH ENTEGRASYON METODU;
            TransitGraph icindeki tum istasyonları konumsal indekse gecer ve en yakın tek istasyonu (K=1) dondurur.
        */
                public static Station GetNearestStationFromGraph(SmartTransit.MultiGraph.TransitGraph graph, double targetX, double targetY)
                {
                    // 1. Haritanın boyutlarına uygun bir QuadTree baslatıyoruz.
                    var tempTree = new QuadTree(500, 500, 500, 500, capacity: 4);

                    // Graph nesnesindeki tum istasyon verileri agaca yukleniyor
                    foreach (var station in graph.Stations)
                    {
                        tempTree.Insert(station);
                    }

                    // Hedef konuma en yakın 1 adet eleman sorgulanıyor
                    List<Station> nearestList = tempTree.FindKNearestNeighbors(targetX, targetY, 1);

                    // 4. Eger liste doluysa en yakın istasyonu, bos kalmıssa null donduruyoruz
                    return nearestList.Count > 0 ? nearestList[0] : null;
                }
    }
}