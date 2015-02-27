using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubmarinesWars.SubmarinesGameLibrary.Field
{
    class AStar
    {
        class PathNode
        {
            public Cell Position { get; set; }
            public float PathLengthFromStart { get; set; }
            public PathNode CameFrom { get; set; }
            public float HeuristicEstimatePathLength { get; set; }
            public float EstimateFullPathLength
            {
                get
                {
                    return this.PathLengthFromStart + this.HeuristicEstimatePathLength;
                }
            }
        }

        public AStar()
        {
        }

        public List<Cell> FindPath(GameField field, Cell start, Cell goal, Func<Cell, Cell, float> weightFunc = null)
        {
            var closedSet = new Collection<PathNode>();
            var openSet = new Collection<PathNode>();
            PathNode startNode = new PathNode()
            {
                Position = start,
                CameFrom = null,
                PathLengthFromStart = 0,
                HeuristicEstimatePathLength = GetHeuristicPathLength(start, goal)
            };
            openSet.Add(startNode);
            while (openSet.Count > 0)
            {
                var currentNode = openSet.OrderBy(node =>
                  node.EstimateFullPathLength).First();
                if (currentNode.Position == goal)
                    return GetPathForNode(currentNode);
                openSet.Remove(currentNode);
                closedSet.Add(currentNode);
                foreach (var neighbourNode in GetNeighbours(currentNode, goal, field, weightFunc))
                {
                    if (closedSet.Count(node => node.Position == neighbourNode.Position) > 0)
                        continue;
                    var openNode = openSet.FirstOrDefault(node =>
                      node.Position == neighbourNode.Position);
                    if (openNode == null)
                        openSet.Add(neighbourNode);
                    else
                        if (openNode.PathLengthFromStart > neighbourNode.PathLengthFromStart)
                        {
                            openNode.CameFrom = currentNode;
                            openNode.PathLengthFromStart = neighbourNode.PathLengthFromStart;
                        }
                }
            }
            return null;
        }


        //float Critetion(Cell) {
        //    return 0.5;
        //}

        private float GetDistanceBetweenNeighbours(Cell from, Cell to, Func<Cell, Cell, float> weightFunc = null)
        {

            //FindPath(0, 0, 0, Critetion);
            //return ((from.Type == CellType.DEEP) && (from.Type == to.Type)) ? 1 : 9999;
            if (weightFunc == null)
            {
                return 1;
            }
            return weightFunc.Invoke(from, to);
        }

        private int GetHeuristicPathLength(Cell from, Cell to)
        {
            return Math.Abs(from.I - to.I) + Math.Abs(from.J - to.J);
        }

        private Collection<PathNode> GetNeighbours(PathNode pathNode, Cell goal, GameField field, Func<Cell, Cell, float> weightFunc = null)
        {
            var result = new Collection<PathNode>();

            List<Cell> neighboursCell = pathNode.Position.Neighbours;

            foreach (var point in neighboursCell)
            {
                if (point.Type == CellType.LAND)
                    continue;
                
                var neighbourNode = new PathNode()
                {
                    Position = point,
                    CameFrom = pathNode,
                    PathLengthFromStart = pathNode.PathLengthFromStart + GetDistanceBetweenNeighbours(pathNode.Position, point, weightFunc),
                    HeuristicEstimatePathLength = GetHeuristicPathLength(point, goal)
                };
                result.Add(neighbourNode);
            }
            return result;
        }

        private List<Cell> GetPathForNode(PathNode pathNode)
        {
            var result = new List<Cell>();
            var currentNode = pathNode;
            while (currentNode != null)
            {
                result.Add(currentNode.Position);
                currentNode = currentNode.CameFrom;
            }
            result.Reverse();
            return result;
        }
    }
}
