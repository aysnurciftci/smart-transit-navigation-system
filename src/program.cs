using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using SmartTransit.Models;
using SmartTransit.Generator;
using SmartTransit.MultiGraph;
using SmartTransit.Pathfinding;
using SmartTransit;
using SmartTransit.Tests;
using System.Collections.Generic;
using System.Linq;

// Proje başlarken core testleri çalıştırıyoruz
TestProgram.RunAllTests();

var builder = WebApplication.CreateBuilder(args);

// 1. CORS Servisini ekledik
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// 2. CORS'u aktif et (Middleware sırası önemlidir!)
app.UseCors("AllowAll");

// API Durum değişkenleri
TransitGraph? myCityMap = null;
SmartTransit.DataStructures.Locator? myLocator = null;

app.MapPost("/api/transit/generate", (int? stationCount, double? maxRouteLength) => {
    int actualStationCount = stationCount ?? 35;
    double actualMaxRouteLength = maxRouteLength ?? 220;
    double width = 800;
    double height = 600;
    bool enableBundling = true;

    myCityMap = GraphGenerator.CreateFullGraph(actualStationCount, width, height, actualMaxRouteLength, enableBundling);
    myLocator = new SmartTransit.DataStructures.Locator(myCityMap, width, height);

    return Results.Ok(new {
        stations = myCityMap.Stations.Select(s => new { id = s.Id, name = s.Name, x = s.X, y = s.Y }),
        routes = myCityMap.Routes.Select(r => new {
            id = r.Id,
            sourceId = r.Source.Id,
            targetId = r.Target.Id,
            distance = r.Distance,
            time = r.Time,
            cost = r.Cost,
            pathPoints = r.PathPoints.Select(p => new { x = p.X, y = p.Y }).ToList()
        })
    });
});

// 2. ENDPOINT: En Yakın İstasyon
app.MapGet("/api/transit/locate", (double x, double y) => {
    if (myLocator == null) return Results.BadRequest("Harita henüz üretilmedi.");
    var nearest = myLocator.LocateNearestStation(x, y);
    if (nearest == null) return Results.NotFound();
    return Results.Ok(new { id = nearest.Id, name = nearest.Name, x = nearest.X, y = nearest.Y });
});

// 3. ENDPOINT: Rota Hesaplama
app.MapGet("/api/transit/find-path", (int startId, int endId, string algorithm, string criteria) => {
    if (myCityMap == null) return Results.BadRequest("Şehir grafı yüklenmedi.");

    var startNode = myCityMap.Stations.FirstOrDefault(s => s.Id == startId);
    var endNode = myCityMap.Stations.FirstOrDefault(s => s.Id == endId);
    if (startNode == null || endNode == null) return Results.BadRequest("Geçersiz istasyon.");

    var pathRoutesResult = new List<object>();
    var pathStationIds = new List<int>();
    double totalWeight = 0;

    if (algorithm.ToLower() == "dijkstra")
    {
        OptimizationCriteria optCriteria = criteria.ToLower() switch {
            "zaman" => OptimizationCriteria.Time,
            "maliyet" => OptimizationCriteria.Cost,
            "aktarma" => OptimizationCriteria.Transfers,
            _ => OptimizationCriteria.Distance
        };

        List<Station> stationPath = Dijkstra.FindShortestPath(myCityMap, startNode, endNode, optCriteria);
        if (stationPath != null && stationPath.Count > 0)
        {
            pathStationIds = stationPath.Select(s => s.Id).ToList();
            for (int i = 0; i < stationPath.Count - 1; i++)
            {
                var cur = stationPath[i]; var next = stationPath[i + 1];
                var route = myCityMap.Routes.FirstOrDefault(r => 
                    (r.Source.Id == cur.Id && r.Target.Id == next.Id) || 
                    (r.Source.Id == next.Id && r.Target.Id == cur.Id));
                
                if (route != null) {
                    pathRoutesResult.Add(new { id = route.Id, sourceId = route.Source.Id, targetId = route.Target.Id, pathPoints = route.PathPoints.Select(p => new { x = p.X, y = p.Y }) });
                    totalWeight += (optCriteria == OptimizationCriteria.Distance) ? route.Distance :
                                   (optCriteria == OptimizationCriteria.Time) ? route.Time :
                                   (optCriteria == OptimizationCriteria.Cost) ? route.Cost : 1.0;
                }
            }
        }
    }
    else // A*
    {
        AStar astar = new AStar(myCityMap);
        var result = astar.FindPath(startNode, endNode);
        if (result.Path != null && result.Path.Count > 0)
        {
            totalWeight = result.TotalDistance;
            pathStationIds.Add(startNode.Id);
            foreach (var route in result.Path) {
                pathRoutesResult.Add(new { id = route.Id, sourceId = route.Source.Id, targetId = route.Target.Id, pathPoints = route.PathPoints.Select(p => new { x = p.X, y = p.Y }) });
                int nextId = (pathStationIds.Last() == route.Source.Id) ? route.Target.Id : route.Source.Id;
                pathStationIds.Add(nextId);
            }
        }
    }

    return Results.Ok(new { yol = pathStationIds, yolRotalar = pathRoutesResult, toplamMaliyet = totalWeight, kriter = criteria });
});

app.Run("http://0.0.0.0:5000");