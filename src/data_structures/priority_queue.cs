/*
Yazarlar: Taylan Özer
*/
//A* ve Dijkstra'da kullanılacak sıralı kuyruk


using System;
using System.Collections.Generic;
using SmartTransit.Models; 

namespace SmartTransit 
{
    public class PriorityQueue
    {//Taylan Ozer
        // DEPOMUZ: Liste içinde (Mesafe, İstasyon) ikililerini tutacağız.
        // Mesafe (Priority) ne kadar küçükse, o istasyon kuyruğun en önüne geçecek.
        private List<(double Priority, Station Node)> _heap;

        // BAŞLANGIÇ (Constructor): Bu sınıf çağrıldığında boş bir depo oluşturur.
        public PriorityQueue()
        {
            _heap = new List<(double Priority, Station Node)>();
        }

        public int Count 
        {
            get { return _heap.Count; }
        }

        public bool IsEmpty()
        {
            return _heap.Count == 0;
        }

        //Ekleme (Enqueue)
        // Dijkstra algoritması yeni bir istasyon bulduğunda bu metodu çağıracak.
        public void Enqueue(double priority, Station node)
        {//Taylan Ozer
            //Yeni elemanı listenin en sonuna ekle
            _heap.Add((priority, node));
            
            //Eklenen elemanın indeksi 
            int newElementIndex = _heap.Count - 1;
            
            //Elemanı doğru yerine kadar yukarı taşı
            BubbleUp(newElementIndex);
        }

        private void BubbleUp(int index)
        {//Taylan Ozer
            while (index > 0)
            {
                // Ağaç yapısında bir çocuğun ebeveyninin indeksini bulma formülü: (index - 1) / 2
                int parentIndex = (index - 1) / 2;

                // Eğer çocuğun mesafesi (Priority), ebeveyninden BÜYÜK veya EŞİTSE kural bozulmamıştır, döngüyü bitir.
                if (_heap[index].Priority >= _heap[parentIndex].Priority)
                {
                    break;
                }

                // Eğer çocuğun mesafesi KÜÇÜKSE (yani daha öncelikliyse), ebeveyni ile yer değiştir.
                var temp = _heap[index];
                _heap[index] = _heap[parentIndex];
                _heap[parentIndex] = temp;

                // Aynı kontrolü bir üst seviye için yapmaya devam et
                index = parentIndex;
            }
        }
        //Çıkarma (Dequeue)
        public Station Dequeue()
        {//Taylan Ozer
            if (IsEmpty())
            {
                throw new InvalidOperationException("Kuyruk boş, çıkarılacak eleman yok!");
            }

            // En tepeki istasyonu alıp cebimize koyuyoruz
            var minNode = _heap[0].Node;

            // Listenin en sonundaki elemanı alıp, en tepeye koyuyoruz
            var lastElement = _heap[_heap.Count - 1];
            _heap[0] = lastElement;
            
            _heap.RemoveAt(_heap.Count - 1);

            // Eğer kuyrukta hala eleman varsa, tepedeki elemanı doğru yerine kadar aşağı indiriyoruz
            if (_heap.Count > 0)
            {
                BubbleDown(0);
            }

            // Cebimize koyduğumuz en yakın istasyonu Dijkstra'ya teslim ediyoruz
            return minNode;
        }

        private void BubbleDown(int index)
        {//Taylan Ozer
            // Ağaç yapısında sol ve sağ çocukların indekslerini bulma formülleri
            int leftChildIndex = 2 * index + 1;
            int rightChildIndex = 2 * index + 2;
            int smallestIndex = index;

            // Sol çocuk var mı ve mevcut elemandan daha mı küçük?
            if (leftChildIndex < _heap.Count && _heap[leftChildIndex].Priority < _heap[smallestIndex].Priority)
            {
                smallestIndex = leftChildIndex;
            }

            // Sağ çocuk var mı ve şu ana kadarki en küçükten daha mı küçük?
            if (rightChildIndex < _heap.Count && _heap[rightChildIndex].Priority < _heap[smallestIndex].Priority)
            {
                smallestIndex = rightChildIndex;
            }

            // Eğer en küçük eleman bizim mevcut elemanımız değilse  yer değiştir
            if (smallestIndex != index)
            {
                var temp = _heap[index];
                _heap[index] = _heap[smallestIndex];
                _heap[smallestIndex] = temp;

                // Değişim yapıldıktan sonra alt seviyeler için kuralı kontrol etmeye devam et
                BubbleDown(smallestIndex);
            }
        }
    }
}
