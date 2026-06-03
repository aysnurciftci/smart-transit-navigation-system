/*
Yazarlar: Bora Pektaş

Özet
Özel Hash Table emplentasyonu.
Jenerik anahtar destekli yani string veya int çalışıyor. Ortalama O(1) zaman karmaşıklığı
Collision (çarpışma) durumunda kovalama kullanıyor (Bağlı liste).
0.75'ten fazla dolduğunda ekstra alan tahsis ediyor

THREAD-SAFETY: Asenkron işlemlere karşı korumalı (ReaderWriterLockSlim).
*/

using System;
using System.Collections.Generic;
using System.Threading;

namespace SmartTransit.DataStructures
{
    
    public class HashTable<TKey, TValue> : IDisposable
    {//Bora Pektas
        private class HashNode
        {
            public TKey Key { get; set; }
            public TValue Value { get; set; }
            public HashNode Next { get; set; }

            public HashNode(TKey key, TValue value)
            {
                Key = key;
                Value = value;
                Next = null;
            }
        }

        private HashNode[] buckets;
        private int count;
        private const int InitialCapacity = 16;
        private const float LoadFactorThreshold = 0.75f;

        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        public int Count 
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    return count;
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
        }

        public HashTable(int capacity = InitialCapacity)
        {
            buckets = new HashNode[capacity];
            count = 0;
        }

        //Girdinin array içinde nerede olduğunu bulma
        private int GetBucketIndex(TKey key)
        {//Bora Pektas
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            
            int hashCode = key.GetHashCode();
            
            //Csharp hash kodunun pozitif olmasından emin olmak için eksi işareti bit and ile kaldırılıyor
            return (hashCode & 0x7FFFFFFF) % buckets.Length;
        }

        //Ekleme fonksiyonu, %75 dolu ise daha fazla alan ister. Girdinin zaten olup olmadığına bakar,
        public void Add(TKey key, TValue value)
        {//Bora Pektas
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            _lock.EnterWriteLock();
            try
            {
                if ((float)count / buckets.Length >= LoadFactorThreshold)
                {
                    Resize();
                }

                int index = GetBucketIndex(key);
                HashNode head = buckets[index];

                //Girdi zaten var mı kontrolü
                HashNode current = head;
                while (current != null)
                {
                    if (current.Key.Equals(key))
                    {
                        throw new ArgumentException($"An item with the same key has already been added. Key: {key}");
                    }
                    current = current.Next;
                }

                //Girdi yeni ise bağlı listenin sonuna ekler
                HashNode newNode = new HashNode(key, value)
                {
                    Next = head
                };
                buckets[index] = newNode;
                count++;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public bool ContainsKey(TKey key)
        {//Bora Pektas
            _lock.EnterReadLock();
            try
            {
                int index = GetBucketIndex(key);
                HashNode current = buckets[index];

                while (current != null)
                {
                    if (current.Key.Equals(key))
                        return true;
                    current = current.Next;
                }

                return false;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
        
        //Girdinin Bağlı Listede olup olmadığını kontrol eder
        public bool TryGetValue(TKey key, out TValue value)
        {//Bora Pektas
            _lock.EnterReadLock();
            try
            {
                int index = GetBucketIndex(key);
                HashNode current = buckets[index];

                while (current != null)
                {
                    if (current.Key.Equals(key))
                    {
                        value = current.Value;
                        return true;
                    }
                    current = current.Next;
                }

                value = default(TValue);
                return false;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
        
        //Verilen anahtarın değerini bulur
        public TValue Get(TKey key)
        {//Bora Pektas
            if (TryGetValue(key, out TValue value))
            {
                return value;
            }
            throw new KeyNotFoundException($"The given key '{key}' was not present in the hash table.");
        }
        
        
        //Verilen yeni girdiyi eski anahtar ile değiştirir
        public void Set(TKey key, TValue value)
        {//Bora Pektas
            _lock.EnterWriteLock();
            try
            {
                int index = GetBucketIndex(key);
                HashNode current = buckets[index];

                while (current != null)
                {
                    if (current.Key.Equals(key))
                    {
                        current.Value = value;
                        return;
                    }
                    current = current.Next;
                }

                // Eğer bulunamazsa Add zaten WriteLock isteyecek (Recursion destekli)
                Add(key, value);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
        
        //Verilen anahtarı siler
        public bool Remove(TKey key)
        {//Bora Pektas
            _lock.EnterWriteLock();
            try
            {
                int index = GetBucketIndex(key);
                HashNode current = buckets[index];
                HashNode prev = null;

                while (current != null)
                {
                    if (current.Key.Equals(key))
                    {
                        if (prev == null)
                        {
                            buckets[index] = current.Next; // Remove head
                        }
                        else
                        {
                            prev.Next = current.Next; // Bypass current node
                        }
                        count--;
                        return true;
                    }
                    prev = current;
                    current = current.Next;
                }

                return false;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
        
        //Alanını 2 kat'a çıkarır (alan bitmeye başladığında)
        private void Resize()
        {//Bora Pektas
            int newCapacity = buckets.Length * 2;
            HashNode[] newBuckets = new HashNode[newCapacity];

            foreach (var head in buckets)
            {
                HashNode current = head;
                while (current != null)
                {
                    HashNode next = current.Next;
                    
                    int hashCode = current.Key.GetHashCode();
                    int newIndex = (hashCode & 0x7FFFFFFF) % newCapacity;

                    current.Next = newBuckets[newIndex];
                    newBuckets[newIndex] = current;

                    current = next;
                }
            }

            buckets = newBuckets;
        }

        // Hash table'ın array gibi kullanılmasını sağlayan bir C# özelliği.
        public TValue this[TKey key]
        {
            get => Get(key);
            set => Set(key, value);
        }

        public void Dispose()
        {
            _lock?.Dispose();
        }
    }
}
