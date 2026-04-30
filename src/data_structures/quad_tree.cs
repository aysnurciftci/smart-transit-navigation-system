/*
 * Bu sınıf, Quadtree veri yapısının yönetim merkezidir. 
 * Sisteme yeni duraklar (Station) eklenmesini ve bu durakların 
 * hiyerarşik bir düzende organize edilmesini kontrol eder.
 * * Ara Rapor Notu: Mevcut yapıda temel ekleme (Insert) mantığı kurulmuştur. 
 * İlerleyen aşamalarda, kapasite aşımı durumunda alanı dört yeni çeyreğe bölecek 
 * olan 'Subdivide' mekanizması ve konumsal sorgulama (Query) özellikleri eklenecektir.
 */
using SmartTransit.Models;

namespace SmartTransit.DataStructures
{
    public class QuadTree
    {
        private QuadNode root;
        private int capacity = 4; // Her bölgeye en fazla kaç durak gelebilir?

        public QuadTree(double x, double y, double w, double h)
        {
            root = new QuadNode(x, y, w, h);
        }

        // Durak ekleme fonksiyonu (Temel düzey)
        public void Insert(Station station)
        {
            // İleride buraya konum kontrolü ve bölme mantığı gelecek
            root.Stations.Add(station);
            
            // Ara rapor için: Eğer kapasite aşılırsa ileride bölünecek
            if (root.Stations.Count > capacity && !root.IsDivided)
            {
                // Subdivide() fonksiyonu buraya gelecek
            }
        }
    }
}