using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
class Player
{
    static void Main(string[] args)
    {

        string[] inputs;
        int numberOfCells = int.Parse(Console.ReadLine()); // amount of hexagonal cells in this map
        Cell[] cells = new Cell[numberOfCells];

        for (int i = 0; i < numberOfCells; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            int type = int.Parse(inputs[0]); // 0 for empty, 1 for eggs, 2 for crystal
            int initialResources = int.Parse(inputs[1]); // the initial amount of eggs/crystals on this cell
            int neigh0 = int.Parse(inputs[2]); // the index of the neighbouring cell for each direction
            int neigh1 = int.Parse(inputs[3]);
            int neigh2 = int.Parse(inputs[4]);
            int neigh3 = int.Parse(inputs[5]);
            int neigh4 = int.Parse(inputs[6]);
            int neigh5 = int.Parse(inputs[7]);
            cells[i] = new Cell();
            cells[i].Index = i;
            cells[i].CellType = type;
            cells[i].Resources = initialResources;
            if (neigh0 != -1)
                cells[i].NeighboursIndex.Add(neigh0);
            if (neigh1 != -1)
                cells[i].NeighboursIndex.Add(neigh1);
            if (neigh2 != -1)
                cells[i].NeighboursIndex.Add(neigh2);
            if (neigh3 != -1)
                cells[i].NeighboursIndex.Add(neigh3);
            if (neigh4 != -1)
                cells[i].NeighboursIndex.Add(neigh4);
            if (neigh5 != -1)
                cells[i].NeighboursIndex.Add(neigh5);
        }
        int numberOfBases = int.Parse(Console.ReadLine());
        inputs = Console.ReadLine().Split(' ');
        for (int i = 0; i < numberOfBases; i++)
        {
            int myBaseIndex = int.Parse(inputs[i]);
            cells[myBaseIndex].IsMyBase = true;
        }
        inputs = Console.ReadLine().Split(' ');
        for (int i = 0; i < numberOfBases; i++)
        {
            int oppBaseIndex = int.Parse(inputs[i]);
            cells[oppBaseIndex].IsOppBase = true;
        }
        var nbInitTotalEggs = cells.Where(c => c.CellType == 1).Sum(c => c.Resources);
        int nbTurn = 0;
        var action = "";
        var nbInitCellWithRessourcesOnly = cells.Where(c => (c.CellType > 0)).Count();

        // game loop
        while (true)
        {
            inputs = Console.ReadLine().Split(' ');
            var myScore = int.Parse(inputs[0]);
            var oppScore = int.Parse(inputs[1]);
            for (int i = 0; i < numberOfCells; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int resources = int.Parse(inputs[0]); // the current amount of eggs/crystals on this cell
                int myAnts = int.Parse(inputs[1]); // the amount of your ants on this cell
                int oppAnts = int.Parse(inputs[2]); // the amount of opponent ants on this cell
                cells[i].Resources = resources;
                cells[i].MyAnts = myAnts;
                cells[i].OppAnts = oppAnts;
            }
            nbInitCellWithRessourcesOnly = cells.Where(c => (c.CellType > 0 && c.Resources > 0)).Count();

            var graphAll = Helpers.GenerateGraphFromCells(cells);
            var cellWithRessources = cells.Where(c => ((c.Resources > 0) && (c.CellType > 0)) || (c.IsMyBase == true) || (c.IsOppBase == true)).OrderBy(c => c.CellType).ToList();
            var allDistances = Helpers.CalculateDistance(cellWithRessources, graphAll).OrderBy(c => c.EndCellType).ToList();
            var nbTotalEggs = cells.Where(c => c.CellType == 1).Sum(c => c.Resources);
            var nbTotalCrystals = cells.Where(c => c.CellType == 2).Sum(c => c.Resources);
            var totalAnts = cells.Sum(c => c.MyAnts);
            var totalOppAnts = cells.Sum(c => c.OppAnts);
            var nbCellsWithAnt = cells.Where(c => c.MyAnts > 0).Count();
            var currentNbCrystals = cells.Where(c => c.CellType == 2).Sum(c => c.Resources);
            var nbRemainingEggs = cells.Where(c => c.CellType == 1).Sum(c => c.Resources);
            var nbCellsWithResources = cellWithRessources.Count();
            var averageAntByCell = Math.Floor((decimal)(totalAnts / nbCellsWithAnt));
            // Définition du chemin
            List<List<int>> allPaths = new();
            List<List<int>> paths = new();
            List<int> positionsMyBase = cells.Where(c => c.IsMyBase).Select(c => c.Index).ToList();
            foreach (var idxBase in positionsMyBase)
            {
                paths = new();
                paths = Helpers.GetFullPath(idxBase, cells, allDistances, paths, new(), totalAnts, totalOppAnts, new() { idxBase }, positionsMyBase.Count());
                allPaths.AddRange(paths);

            }
            // deduplicate for keeping smallest distance only
            allPaths = allPaths.GroupBy(p => p.Last()).Select(p => p.OrderBy(p => p.Count()).First()).ToList();


            List<int> visitedIdx = new();

            int cptTarget = 0;
            int nbCellWithCrystalOccupied = cells.Where(c => c.MyAnts > 0 && c.CellType == 2 && c.Resources > 0).Count();
            int nbCellWithEggOccupied = cells.Where(c => c.MyAnts > 0 && c.CellType == 1 && c.Resources > 0).Count();

            var maxPaths = Math.Ceiling((decimal)nbInitCellWithRessourcesOnly / 2);
            // if (nbTurn<3)
            //     maxPaths=1;
            action = "";

            cptTarget = 0;

            foreach (var path in allPaths)
            {
                if (cptTarget > maxPaths)
                    continue;
                foreach (var idx in path)
                {
                    var power = 1;
                    //if( (cells[idx].OppAnts > cells[idx].MyAnts) && (cells[idx].Resources>0))
                    //    power = cells[idx].OppAnts+1;



                    if (!visitedIdx.Contains(idx))
                    {
                        action += $"BEACON {idx} {power};";//{power};";
                        visitedIdx.Add(idx);
                    }
                }
                cptTarget++;

            }
            Console.WriteLine(string.IsNullOrWhiteSpace(action) ? "WAIT" : action);
            nbTurn++;
        }
    }
}
static class Helpers
{
    public static List<List<int>> GetFullPath(
        int idxBase,
        Cell[] cells,
        List<CellsDistance> distances,
        List<List<int>> curPath,
        List<int> visited,
        int totalAnts,
        int totalOppAnts,
        List<int> visitedCells,
        int myBaseCount)
    {
        if (totalAnts <= 0)
            return curPath;
        int start = -1;
        int end = -1;
        int antratio = 0;
        List<int> pathToEnd = new();
        CellsDistance c = new();
        if (distances.Where(d => visitedCells.Contains(d.StartIndex) && !visited.Contains(d.EndIndex)).Any())
        {

            antratio = 2;
            if (distances.Any(d => visitedCells.Contains(d.StartIndex) && !visited.Contains(d.EndIndex) && d.EndCellType == 1))
            {
                c = distances.Where(d => visitedCells.Contains(d.StartIndex) && !visited.Contains(d.EndIndex) && d.EndCellType == 1).OrderBy(d => d.Distance).First();
            }
            else if (distances.Any(d => visitedCells.Contains(d.StartIndex) && !visited.Contains(d.EndIndex) && d.EndCellType == 2))
                c = distances.Where(d => visitedCells.Contains(d.StartIndex) && !visited.Contains(d.EndIndex) && d.EndCellType == 2).OrderBy(d => d.Distance).First();
            else
                c = distances.Where(d => visitedCells.Contains(d.StartIndex) && !visited.Contains(d.EndIndex)).OrderBy(d => d.Distance).First();
            start = c.StartIndex;
            end = c.EndIndex;
            pathToEnd = c.PathToEnd;
            curPath.Add(pathToEnd);
            visited.Add(end);
            visitedCells.AddRange(pathToEnd);
            if (totalAnts > (pathToEnd.Count() * antratio))
                GetFullPath(idxBase, cells, distances, curPath, visited, totalAnts - (pathToEnd.Count() * antratio), totalOppAnts, visitedCells, myBaseCount);

        }
        return curPath;
    }
    public static Graph<int> GenerateGraphFromCells(Cell[] cells)
    {
        var vertices = cells.Select(c => c.Index).ToArray();
        Tuple<int, int>[] edgesall = cells
                .SelectMany(c => c.NeighboursIndex, (cell, neigh) => new { cell, neigh })
                .Select(cn => new Tuple<int, int>(cn.cell.Index, cn.neigh)).ToArray();

        return new Graph<int>(vertices, edgesall);
    }
    public static List<CellsDistance> CalculateDistance(List<Cell> cells, Graph<int> graphAll)
    {
        var allDistances = new List<CellsDistance>();
        var algorithms = new Algorithms();
        //Console.Error.WriteLine($"disin");
        foreach (var c in cells)
        {
            // Console.Error.WriteLine($"1 {c.Index}");
            var cellsToBrowse = cells.Where(c => !allDistances.Any(a => a.StartIndex == c.Index));
            foreach (var c2 in cellsToBrowse)
            {
                //Console.Error.WriteLine($"2 {c2.Index}");
                if (c.Index != c2.Index)
                {
                    if (!allDistances.Any(d => d.StartIndex == c.Index && d.EndIndex == c2.Index))
                    {
                        if (!c2.IsOppBase && !c2.IsMyBase)
                        {
                            allDistances.Add(new()
                            {
                                StartIndex = c.Index,
                                EndIndex = c2.Index,
                                EndCellType = c2.CellType,
                                EndCellResources = c2.Resources,
                                NbCellsWithCrystalInPath = algorithms.ShortestPathFunction(graphAll, c.Index)(c2.Index).Sum(i => cells.Where(cin => cin.Index == i && cin.CellType == 2).Count()),
                                PathToEnd = algorithms.ShortestPathFunction(graphAll, c.Index)(c2.Index).ToList(),
                                Distance = algorithms.ShortestPathFunction(graphAll, c.Index)(c2.Index).Count()
                            });
                        }
                    }
                }
            }
        }
        //Console.Error.WriteLine($"dis");
        return allDistances;
    }
    public static void DisplayDistances(List<CellsDistance> distances)
    {
        distances.ForEach(d => {
            Console.Error.WriteLine($"StartIndex:{d.StartIndex} EndIndex:{d.EndIndex} Distance:{d.Distance} nb:{d.NbCellsWithCrystalInPath}");
        });
    }
}

class Cell
{
    public bool IsMyBase { get; set; } = false;
    public bool IsOppBase { get; set; } = false;
    public int CellType { get; set; }
    public int Index { get; set; }
    public int MyAnts { get; set; }
    public int OppAnts { get; set; }
    public int Resources { get; set; }
    public List<int> NeighboursIndex { get; set; } = new List<int>();
}

class CellsDistance
{
    public int StartIndex { get; set; }
    public int EndIndex { get; set; }
    public int EndCellType { get; set; }
    public int EndCellResources { get; set; }
    public int NbCellsWithCrystalInPath { get; set; }
    public List<int> PathToEnd { get; set; } = new();
    public int Distance { get; set; }
}

class Graph<T>
{
    public Graph() { }
    public Graph(IEnumerable<T> vertices, IEnumerable<Tuple<T, T>> edges)
    {
        foreach (var vertex in vertices)
            AddVertex(vertex);

        foreach (var edge in edges)
            AddEdge(edge);
    }

    public Dictionary<T, HashSet<T>> AdjacencyList { get; } = new Dictionary<T, HashSet<T>>();

    public void AddVertex(T vertex)
    {
        AdjacencyList[vertex] = new HashSet<T>();
    }

    public void AddEdge(Tuple<T, T> edge)
    {
        if (AdjacencyList.ContainsKey(edge.Item1) && AdjacencyList.ContainsKey(edge.Item2))
        {
            AdjacencyList[edge.Item1].Add(edge.Item2);
            AdjacencyList[edge.Item2].Add(edge.Item1);
        }
    }
}

class Algorithms
{

    public Func<T, IEnumerable<T>> ShortestPathFunction<T>(Graph<T> graph, T start)
    {
        var previous = new Dictionary<T, T>();

        var queue = new Queue<T>();
        queue.Enqueue(start);

        while (queue.Count > 0)
        {
            var vertex = queue.Dequeue();
            foreach (var neighbor in graph.AdjacencyList[vertex])
            {
                if (previous.ContainsKey(neighbor))
                    continue;

                previous[neighbor] = vertex;
                queue.Enqueue(neighbor);
            }
        }

        Func<T, IEnumerable<T>> shortestPath = v => {
            var path = new List<T> { };

            var current = v;
            while (!current.Equals(start))
            {
                path.Add(current);
                current = previous[current];
            };

            path.Add(start);
            path.Reverse();

            return path;
        };

        return shortestPath;
    }
}