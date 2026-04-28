/*
program.cs ana dosyamızdır ve program buradan başlayacaktır. Burada sadece
başka yerlerde yazılmış ve hazır olarak kapsüllenmiş kısımları çağırın.
TransitGraph, bizim multigraph nesnemizdir. Aşağıdaki örnekteki gibi
kullanarak istediğiniz kadar graph oluşturabilir ve kullanabilirsiniz.

Girdilerinin anlamları için lütfen multi_graph_generator.cs'e bakın.

Graphlar için GetOutgoingRoutes() diye bir metod ekledim, bu metod bir
istasyona bağlı olan bütün rotaları liste halinde döndürüyor. Yol bulma
için lazımdı, kullanabilirsiniz.
*/


using SmartTransit.Models;
using SmartTransit.Generator;
using SmartTransit.MultiGraph;

class Program
{
    static void Main()
    {
        // Fonksiyonu çağır ve objeyi al
        TransitGraph myCityMap = GraphGenerator.CreateFullGraph(60, 800, 600, 100, enableVisualBundling: true);

        // Artık 'myCityMap' üzerinden tüm istasyonlara ve rotalara erişebilirsin
        Console.WriteLine($"Toplam İstasyon: {myCityMap.Stations.Count}");
        Console.WriteLine($"Toplam Rota: {myCityMap.Routes.Count}");
        
        var rotalar = myCityMap.GetOutgoingRoutes(myCityMap.Stations[0]);
        
        Console.WriteLine($"Listedeki İlk İstasyonun Bağlantıları:");
        
        foreach (var rota in rotalar)
        {
        // Using string interpolation to print the details of each route
            Console.WriteLine($"İstasyon: {rota.Target.Id} | Mesafe: {rota.Distance:F2} metre");
        }

       
         
    }
}
