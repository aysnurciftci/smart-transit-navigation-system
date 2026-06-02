using System;
using System.Collections.Generic;
using SmartTransit.Models;
using SmartTransit.Generator;
using SmartTransit.MultiGraph;
using SmartTransit.Pathfinding;

namespace SmartTransit.Tests
{
    public static class TestProgram
    {
        public static void RunAllTests()
        {
            Console.WriteLine("=== SMART TRANSIT SYSTEM TESTS ===");
            Console.WriteLine("1. Graph Generation & Multigraph Duplicates...");
            
            // Generate graph
            TransitGraph myCityMap = GraphGenerator.CreateFullGraph(50, 800, 600, 100, enableVisualBundling: false);
            
            Console.WriteLine($"   -> Graph generated successfully.");
            Console.WriteLine($"   -> Total Stations: {myCityMap.Stations.Count}");
            Console.WriteLine($"   -> Total Routes (Including Duplicates): {myCityMap.Routes.Count}");

            Console.WriteLine("\n2. Testing HashTable-backed Properties & Randomized Attributes...");
            if (myCityMap.Routes.Count > 0)
            {
                var sampleRoute = myCityMap.Routes[0];
                Console.WriteLine($"   -> Route {sampleRoute.Id}: Station {sampleRoute.Source.Id} to Station {sampleRoute.Target.Id}");
                Console.WriteLine($"   -> Real Euclidean Distance: {sampleRoute.Distance:F2}");
                Console.WriteLine($"   -> Randomized Time: {sampleRoute.Time:F2}");
                Console.WriteLine($"   -> Randomized Cost: {sampleRoute.Cost:F2}");
            }

            Console.WriteLine("\n3. Testing O(1) Adjacency List Lookups...");
            var startStation = myCityMap.Stations[0];
            var outgoingRoutes = myCityMap.GetOutgoingRoutes(startStation);
            Console.WriteLine($"   -> Station {startStation.Id} has {outgoingRoutes.Count} outgoing routes:");
            foreach (var route in outgoingRoutes)
            {
                var neighbor = route.Source.Id == startStation.Id ? route.Target : route.Source;
                Console.WriteLine($"      - To Station {neighbor.Id} (Route ID: {route.Id}, Dist: {route.Distance:F1})");
            }

            Console.WriteLine("\n4. Testing A* Pathfinding Algorithm (Using Distance)...");
            
            // Pick a random goal that is guaranteed to be in the graph 
            // (Since the MST guarantees connectivity, any node is reachable)
            var goalStation = myCityMap.Stations[myCityMap.Stations.Count - 1]; 
            
            Console.WriteLine($"   -> Finding shortest path from Station {startStation.Id} to Station {goalStation.Id}");
            
            AStar astar = new AStar(myCityMap);
            var (path, totalDistance) = astar.FindPath(startStation, goalStation);

            if (path.Count > 0)
            {
                Console.WriteLine($"   -> PATH FOUND! Total Distance: {totalDistance:F2}");
                Console.WriteLine("   -> Route Sequence:");
                
                double accumulatedTime = 0;
                double accumulatedCost = 0;

                int currentStationId = startStation.Id;
                foreach (var route in path)
                {
                    var nextStationId = route.Source.Id == currentStationId ? route.Target.Id : route.Source.Id;
                    
                    Console.WriteLine($"      [Route {route.Id}] Station {currentStationId} -> Station {nextStationId} | Dist: {route.Distance:F1}, Time: {route.Time:F1}, Cost: {route.Cost:F2}");
                    
                    accumulatedTime += route.Time;
                    accumulatedCost += route.Cost;
                    currentStationId = nextStationId;
                }
                
                Console.WriteLine($"\n   -> Final Path Stats: Distance: {totalDistance:F2}, Total Time: {accumulatedTime:F2}, Total Cost: {accumulatedCost:F2}");
            }
            else
            {
                Console.WriteLine("   -> NO PATH FOUND. (This should not happen with the MST generator unless stations are 0)");
            }

            Console.WriteLine("\n=== TESTS COMPLETE ===");
        }
    }
}
