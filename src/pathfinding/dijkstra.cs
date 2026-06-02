/*
Yazarlar: Taylan Özer
*/

//Dijkstra yol bulma algoritması


using System;
using System.Collections.Generic;
using SmartTransit.Models;
using SmartTransit.MultiGraph;

namespace SmartTransit
{
    public enum OptimizationCriteria
    {
        Distance, 
        Time,     
        Cost,
        Transfers      
    }

    public class Dijkstra
    {
        public static List<Station> FindShortestPath(TransitGraph graph, Station startNode, Station endNode, OptimizationCriteria criteria = OptimizationCriteria.Distance)
        {
            var costs = new SmartTransit.DataStructures.HashTable<int, double>();
            var previousNodes = new SmartTransit.DataStructures.HashTable<int, Station>();
            var pq = new PriorityQueue();

            costs[startNode.Id] = 0;
            pq.Enqueue(0, startNode);

            while (!pq.IsEmpty())
            {
                Station currentNode = pq.Dequeue();

                // Başlangıç istasyonunun kendisine olan mesafesi sıfır.
                if (currentNode.Id == endNode.Id)
                {
                    break;
                }

                var outgoingRoutes = graph.GetOutgoingRoutes(currentNode);

                foreach (var route in outgoingRoutes)
                {
                    Station neighbor = route.Target;
                    
                    double routeWeight = 0;
                 
                    switch (criteria)
                    {
                        case OptimizationCriteria.Distance:
                            routeWeight = route.Distance;
                            break;                          
                        case OptimizationCriteria.Time:
                            routeWeight = route.Time;
                            break;
                        case OptimizationCriteria.Cost:
                            routeWeight = route.Cost;
                            break;
                        case OptimizationCriteria.Transfers:
                            routeWeight = 1; // En az aktarma, her durağı 1 say
                            break;    
            
                    }

                    double newCost = costs[currentNode.Id] + routeWeight;
                    // Komşuya daha önce uğramadıysak veya şu anki istasyon üzerinden gitmek 
                    // daha önce bulduğumuz yoldan daha ucuz maliyetliyse veriyi güncelliyoruz.
                    if (!costs.ContainsKey(neighbor.Id) || newCost < costs[neighbor.Id])
                    {
                        costs[neighbor.Id] = newCost;
                        previousNodes[neighbor.Id] = currentNode;
                        pq.Enqueue(newCost, neighbor);
                    }
                }
            }

            var path = new List<Station>();

            if (!previousNodes.ContainsKey(endNode.Id) && startNode.Id != endNode.Id)
            {
                return path; 
            }
            // Hedef istasyondan geriye doğru, bizi bu istasyona getiren bir önceki istasyonları 
            // (previousNodes) sorgulayarak başlangıç noktasına kadar bir zincir oluşturuyoruz.
            Station? step = endNode;
            while (step != null)
            {
                path.Add(step);
                if (previousNodes.ContainsKey(step.Id))
                {
                    step = previousNodes[step.Id];
                }
                else
                {
                    step = null;
                }
            }
            // Liste hedef -> başlangıç sırasında biriktiği için navigasyon rotasını ters çeviriyoruz
            path.Reverse();
            return path;
        }
    }
}
