/*
  AUTHOR: Aysenur Ciftci
  MODULE: QuadTree - Eşzamanlı Simülasyon Korumalı Konumsal İndeksleme Engine

  DESIGN RATIONALE & ARCHITECTURAL COMPARISON (MİMARİ TASARIM GEREKÇESİ)
  --------------------------------------------------------------------------------------
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

  TIME COMPLEXITY & PERFORMANCE OPTIMIZATION (BIG-O VE OPTİMİZASYON)
  --------------------------------------------------------------------------------------
  - Veri Ekleme (Insertion)              : Ortalama O(log N) | En Kötü O(N) (Yoğun Yığılma)
  - En Yakın K Durak Araması (KNN Search): Mevcut sürümde O(N) (Tüm aktif düğümler taranır)

  NOT: Mevcut sürümde KNN sorgusu doğrusal arama uzayını optimize etmek adına tüm aktif
  düğümleri taramaktadır. Gelecek sürümlerde bölgesel budama (spatial pruning) mekanizmalarının
  eklenmesiyle arama alanının daraltılması ve ortalama sorgu performansının iyileştirilmesi
  hedeflenmektedir.

  KAREKÖK OPTİMİZASYONU (PERFORMANCE OPTIMIZATION):
  Mesafe sıralama işlemlerinde CPU'yu yoğun şekilde yoran 'Math.Sqrt()' (Karekök)
  hesaplaması kaldırılarak 'Öklid Mesafesinin Karesi' (dx*dx + dy*dy) modeline geçilmiştir.
  Matematiksel olarak uzaklıkların karelerinin sıralaması ile gerçek uzaklıkların
  sıralaması birebir aynı olduğundan, KNN öncelikli kuyruğu (PriorityQueue) sıfır
  maliyetle ve çok daha yüksek performansla çalışmaktadır.

  UYARI: Aynı koordinata sahip veya birbirine aşırı yakın verilerin oluşturabileceği
  sonsuz bölünme (Infinite Subdivision) ve Stack Overflow riskini önlemek amacıyla,
  sisteme MIN_SIZE (1.0) derinlik sınırı ve koruma mekanizması entegre edilmiştir.

  CONCURRENCY & THREAD-SAFETY (B.1 CRITERIA)
  --------------------------------------------------------------------------------------
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
    public class QuadTree : IDisposable
    {
        private readonly QuadNode _root;
        private readonly int _capacity;

        // Sonsuz bölünmeyi (Stack Overflow) önleyen minimum bölge boyutu sınırı
        private const double MIN_SIZE = 1.0;

        // B.1 İsteri: Simülasyon motoru asenkron çalışırken race condition önleyici kilit
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
            if (!node.Contains(station.X, station.Y)) return false;

            if (node.Stations.Count < _capacity && !node.IsDivided)
            {
                node.Stations.Add(station);
                return true;
            }

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
            if (k <= 0) return new List<Station>();

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
                // Karekök optimizasyonlu mesafe hesabı çağrılıyor
                double distanceSquared = CalculateDistanceSquared(tx, ty, station.X, station.Y);
                pq.Enqueue(distanceSquared, station);
            }

            if (node.IsDivided)
            {
                FindKNNInternal(node.NorthWest, tx, ty, pq);
                FindKNNInternal(node.NorthEast, tx, ty, pq);
                FindKNNInternal(node.SouthWest, tx, ty, pq);
                FindKNNInternal(node.SouthEast, tx, ty, pq);
            }
        }

        private double CalculateDistanceSquared(double x1, double y1, double x2, double y2)
        {
            double dx = x1 - x2;
            double dy = y1 - y2;
            return (dx * dx) + (dy * dy);
        }

        public void Dispose()
        {
            _lock?.Dispose();
        }
    }
}