# Smart Transit Navigation System

Bu proje, **Veri Yapıları (Data Structures)** dersi kapsamında, tamamen sıfırdan ve hiçbir hazır C# koleksiyonu (`Dictionary`, `HashSet`, `Stack`, `Queue` vb.) veya `System.Linq` kütüphanesi kullanılmadan geliştirilmiş, yüksek performanslı bir uzamsal navigasyon ve toplu taşıma simülasyonu altyapısıdır.

**Proje Raporu:** [https://docs.google.com/document/d/1IBOHJYe3yoHdQ8k2MnMZtiVLaSC_uoi0WtdJv7NUChI/edit?usp=sharing](Proje Raporu)
**Proje Raporu .pdf:** [https://docs.google.com/document/d/1IBOHJYe3yoHdQ8k2MnMZtiVLaSC_uoi0WtdJv7NUChI/edit?usp=sharing](Proje Raporu Pdf)
**Proje Videosu:** [https://docs.google.com/document/d/1IBOHJYe3yoHdQ8k2MnMZtiVLaSC_uoi0WtdJv7NUChI/edit?usp=sharing](Demo Videosu (daha yok))

## Proje Özeti
Sistem, rastgele oluşturulan bir şehir haritası (Multigraph) üzerinde istasyonları ve ulaşım ağlarını modeller. Kullanıcının harita üzerinde tıkladığı herhangi bir konuma **en yakın istasyonu anında bulur** ve bu istasyondan hedef noktaya **A*** veya **Dijkstra** algoritmalarını kullanarak en ideal rotayı çizer.

## Nasıl Çalıştırılır?

Projemiz platform bağımsızdır ve tam kapsamlı çalışabilmesi için **Docker** mimarisine geçirilmiştir. Yerel makinenizde herhangi bir .NET veya web sunucusu kurmanıza gerek yoktur.

Sistemi tam yığınıyla (Nginx Frontend + C# Kestrel Backend) ayağa kaldırmak için terminalde projenin kök dizininde şu komutu çalıştırın:

```bash
sudo docker compose up -d
```

Derleme tamamlandıktan sonra tarayıcınızda **`http://localhost`** adresine giderek Smart Transit arayüzünü kullanmaya başlayabilirsiniz.

## Proje Mimarisi ve Docker Konfigürasyonları

Sistem, modern web standartlarına uygun olarak mikroservis benzeri bir container mimarisiyle tasarlanmıştır. Tüm sistem bağımlılık sorunu yaşamadan, tek bir `docker-compose up` komutuyla entegre şekilde ayağa kalkar:

- **Frontend (Nginx Container):** `frontend/Dockerfile` kullanılarak oluşturulur. Alpine Linux tabanlı hafif bir Nginx sunucusu, kullanıcı arayüzünü (HTML/JS/CSS) `80` portundan yayınlar. Aynı zamanda bir Reverse Proxy (Ters Vekil Sunucu) görevi görerek `/api/transit` isteklerini doğrudan Backend servisine yönlendirir.
- **Backend (.NET 9 Kestrel Container):** `src/Dockerfile` kullanılarak oluşturulur. C# algoritmalarımızı ve Minimal API hizmetimizi barındıran çekirdek sunucudur. Nginx'ten gelen talepleri dinler ve JSON formatında rota sonuçlarını döndürür.
- **Docker Compose Orchestration:** `docker-compose.yml` dosyası, Frontend ve Backend container'larını aynı sanal ağ (network) üzerinde birleştirerek bağımlılıkları çözer ve sistemin tek tıkla tam yığın (full-stack) olarak ayağa kalkmasını sağlar.
## Kullanılan Özgün Veri Yapıları
Akademik isterler doğrultusunda, C#'ın hazır kütüphaneleri reddedilerek aşağıdaki tüm veri yapıları sıfırdan yazılmıştır:

- **`HashTable<K,V>`**: O(1) erişim süresine sahip karma tablo. Çarpışma çözümü (Collision Resolution) için **Kovalama (Chaining / Linked List)** yöntemi kullanılmıştır. Doluluk oranı (Load Factor) %75'i geçtiğinde otomatik olarak `Resize` olarak kapasiteyi iki katına çıkarır.
- **`PriorityQueue`**: Min-Heap (Asgari Yığın) mantığıyla çalışan, O(log N) karmaşıklığıyla dizi (Array) tabanlı öncelik kuyruğu. A* ve Dijkstra algoritmalarında uç düğüm (Frontier) yönetimi için kullanılır.
- **`QuadTree` & `QuadNode`**: Uzamsal konumlandırma (Spatial Indexing) için kullanılan ağaç yapısı. `Locator` servisi tarafından 2D harita koordinatları üzerinden O(log N) sürede en yakın komşu (KNN) araması yapar.
- **`TransitGraph`**: Düğümleri (Stations) ve kenarları (Routes) O(1) erişim süresi sağlayan, arka planda özel `HashTable` barındıran Adjacency List (Komşuluk Listesi) mimarisi ile yönetilen Multigraph veri yapısı.

## Çekirdek Algoritmalar
- **Kruskal & Union-Find (MST)**: Harita üretilirken (Graph Generator), istasyonların kesinlikle birbirine bağlı kalmasını sağlamak için Euclidean Minimum Spanning Tree (Minimum Kapsayan Ağaç) mantığı kullanılmıştır.
- **Dijkstra Algoritması**: Öncelik kuyruğu (PriorityQueue) ve özel HashTable kullanarak `Mesafe`, `Süre`, `Maliyet` veya `Aktarma Sayısı` kriterlerine göre en kısa yolu bulur.
- **A* Algoritması (A-Star)**: Sezgi (Heuristic) olarak "Kuş Uçuşu Mesafe" (Euclidean Distance) kullanarak yönlü ve optimize mesafe araması yapar.
- **Force-Directed Edge Bundling**: Düğüm bağlantı kavislerini görsel olarak birleştirmek için simüle edilmiş yay ve çekim kuvveti matematiği.

## Eşzamanlılık ve Thread-Safety (B.1 İsteri)
Proje, asenkron "Simülasyon Motoru" veya mikroservislerin arka planda veri değiştirebilme ihtimaline karşı **Thread-Safe** olarak tasarlanmıştır.
- `QuadTree`, `HashTable` ve `TransitGraph` üzerinde `ReaderWriterLockSlim` mekanizması uygulanmıştır.
- Çoklu okuma işlemleri (ReadLock) aynı anda asenkron yapılabilirken, yazma ve değiştirme (WriteLock) işlemleri Race Condition (Yarış Durumu) engellenerek güvenle kuyruğa alınır.


## Üyeler:
032490020 - Ege Başaran  
032490021 - Hatice Nur Topcu  
032490025 - Ayşe Nur Çiftçi  
032490028 - Bora Pektaş  
032490029 - Taylan Özer  

