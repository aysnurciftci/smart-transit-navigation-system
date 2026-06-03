/*
Yazarlar: Ege Başaran, Bora Pektaş
Bu kısım Multigraph veri yapısıdır. Multigraph, sadece İstasyonları
ve Rotaları tutan bir yapıdan ibarettir ve kendi içinde bunları işlevsel olarak
birleştirmez. Sadece depolama ve erişim amaçlıdır.

THREAD-SAFETY: Asenkron işlemlere karşı korumalı (ReaderWriterLockSlim).
*/

using System;
using System.Collections.Generic;
using System.Threading;
using SmartTransit.Models;
using SmartTransit.DataStructures;

namespace SmartTransit.MultiGraph
{
    public class TransitGraph : IDisposable
    {//Ege Basaran, Bora Pektas
        public List<Station> Stations { get; set; } = new List<Station>();
        public List<Route> Routes { get; set; } = new List<Route>();
        
        // İstasyon ID'sini o istasyona bağlı rotaların listesine bağlayan Adjacency List
        public HashTable<int, List<Route>> AdjacencyList { get; set; }

        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        
        public void BuildAdjacencyList()
        {//Ege Basaran, Bora Pektas
            _lock.EnterWriteLock();
            try
            {
                AdjacencyList = new HashTable<int, List<Route>>();
                
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
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        // Herhangi bir istasyondan çıkan rotaları O(1) karmaşıklığında veren fonksiyon
        public List<Route> GetOutgoingRoutes(Station station)
        {//Ege Basaran, Bora Pektas
            if (AdjacencyList == null)
            {
                BuildAdjacencyList();
            }
            
            _lock.EnterReadLock();
            try
            {
                if (AdjacencyList.TryGetValue(station.Id, out var connectedRoutes))
                {
                    return connectedRoutes;
                }
                
                return new List<Route>();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public void Dispose()
        {
            _lock?.Dispose();
        }
    }
}
