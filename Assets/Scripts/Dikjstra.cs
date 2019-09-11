using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections;

public class DijkstraTile
{
    public Tile t;

    public int Cost;

    public int Distance;
    public bool Completed;

    public List<Tile> AdjascentTiles;

    public List<Tile> Path;

    public DijkstraTile(Tile t)
    {
        this.t = t;
        this.Cost = t.MovementCost;
        this.Completed = false;
        this.Distance = int.MaxValue;
        AdjascentTiles = new List<Tile>();
        Path = new List<Tile>();
    }
}

public static class Dijsktra
{
    public static List<DijkstraTile> GetDistancesWithoutCost(List<Tile> tiles, Tile startPosition)
    {
        if (tiles.Count == 0)
            return null;

        var dijkTiles = InitializeDistance(tiles, startPosition, new List<Tile>());
        foreach (var t in dijkTiles) {
            t.Cost = 1;
        }
        return CalculateDistances(dijkTiles);
    }

    public static List<DijkstraTile> GetDistances(List<Tile> tiles, Tile startPosition, List<Tile> blockedTiles)
    {
        if (tiles.Count == 0)
            return null;
        var dijkTiles = InitializeDistance(tiles, startPosition,blockedTiles);

        return CalculateDistances(dijkTiles);
    }

    private static List<DijkstraTile> CalculateDistances(List<DijkstraTile> dijkTiles)
    {
        DijkstraTile lastChecked = dijkTiles.FirstOrDefault(d => d.Completed);

        while (dijkTiles.Any(d => d.Completed == false))
        {
            List<DijkstraTile> adjTiles = dijkTiles.Where(d => lastChecked.AdjascentTiles.Any(l => l.Coordinate == d.t.Coordinate && !d.Completed)).ToList();

            foreach (var tile in adjTiles)
            {
                if (tile.Distance > lastChecked.Distance + tile.Cost)
                {
                    tile.Distance = lastChecked.Distance + tile.Cost;
                    tile.Path = lastChecked.Path.ToList();
                    tile.Path.Add(lastChecked.t);
                }
            }

            lastChecked.Completed = true;
            var uncompletedDijk = dijkTiles.Where(d => !d.Completed).ToList();
            if (uncompletedDijk.Count == 0)
                break;
            var minDistance = uncompletedDijk.Min(a => a.Distance);
            var minVal = uncompletedDijk.FirstOrDefault(m => m.Distance == minDistance);
            //var minAdj = adjTiles.First(m=>m.distance == adjTiles.Min(a => a.distance));

            lastChecked = minVal;
        }
        return dijkTiles;
    }

    private static List<DijkstraTile> InitializeDistance(List<Tile> tiles, Tile start, List<Tile> blockedTiles)
    {
        
        List<DijkstraTile> dijTiles = new List<DijkstraTile>();
        foreach (var tile in tiles)
        {
            if (tile.Coordinate != start.Coordinate)
            {
                var d = new DijkstraTile(tile);
                d.AdjascentTiles = tiles.Where(t =>
                    (
                        (Math.Abs(tile.Coordinate.x - t.Coordinate.x) == 1 && tile.Coordinate.y - t.Coordinate.y == 0) ||
                        (Math.Abs(tile.Coordinate.y - t.Coordinate.y) == 1 && tile.Coordinate.x - t.Coordinate.x == 0)
                    ) &&
                    !(Math.Abs(tile.Coordinate.x - t.Coordinate.x) == 1 && Math.Abs(tile.Coordinate.y - t.Coordinate.y) == 1)).ToList();
                
                dijTiles.Add(d);
            }
            else
            {
                var d = new DijkstraTile(tile);
                d.Distance = 0;
                d.Completed = true;
                d.AdjascentTiles = tiles.Where(t =>
                (
                        (Math.Abs(tile.Coordinate.x - t.Coordinate.x) == 1 && tile.Coordinate.y - t.Coordinate.y == 0) ||
                        (Math.Abs(tile.Coordinate.y - t.Coordinate.y) == 1 && tile.Coordinate.x - t.Coordinate.x == 0)
                    ) &&
                    !(Math.Abs(tile.Coordinate.x - t.Coordinate.x) == 1 && Math.Abs(tile.Coordinate.y - t.Coordinate.y) == 1)).ToList();

                dijTiles.Add(d);
            }
        }
        foreach (var t in blockedTiles) {
            var dijk = dijTiles.FirstOrDefault(d => d.t.Coordinate == t.Coordinate);
            if (dijk != null)
                dijk.Cost = 1000;
        }
        return dijTiles;
    }
}