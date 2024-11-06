#region Copyright
// MIT License
// 
// Copyright (c) 2023 David María Arribas
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using Navigation.Interfaces;
using Navigation.World;
using UnityEngine;


namespace grupoB
{
    public class AStarAlgorithm : INavigationAlgorithm
    {
        private WorldInfo _worldInfo;

        public void Initialize(WorldInfo worldInfo)
        {
            _worldInfo = worldInfo;
        }

        public CellInfo[] GetPath(CellInfo startNode, CellInfo targetNode)
        {
            var openList = new List<Node>(); // Lista abierta de nodos pendientes por visitar
            var closedList = new HashSet<CellInfo>(); // Lista cerrada de nodos ya visitados

            // Nodo inicial con coste inicial y heurística
            Node start = new Node(startNode, 0, ManhattanDistance(startNode, targetNode));
            openList.Add(start);

            while (openList.Count > 0)//mientras haya nodos por explorar itera
            {
                openList.Sort((a, b) => a.FCost.CompareTo(b.FCost)); //ordena por coste F menor
                Node currentNode = openList[0]; // cge el nodo con F menor
                openList.RemoveAt(0); // lo quita de la lista

                if (currentNode.Cell == targetNode) //si se ha alcanzado el objetivo, devuelve ruta
                    return ReconstructPath(currentNode);

                closedList.Add(currentNode.Cell); //y añadimos el nodo a la lista de nodos visitados

                foreach (var neighbor in GetNeighbors(currentNode.Cell)) //obtiene los nodos vecinos de la celda
                {
                    if (closedList.Contains(neighbor) || !neighbor.Walkable) continue; //ignorar celdas visitadas o inaccesibles

                    int tentativeGCost = currentNode.GCost + 1; //coste a cada casilla vecina es uno
                    int hCost = ManhattanDistance(neighbor, targetNode);// heuristuca hasta el objetivo
                    Node neighborNode = new Node(neighbor, tentativeGCost, hCost, currentNode); //crea el nodo vecino

                    if (openList.Exists(n => n.Cell == neighbor && tentativeGCost >= n.GCost)) continue; // Ignora si el vecino ya tiene menor G

                    openList.Add(neighborNode); // Añade el vecino a la lista
                }
            }

            return null; // Ruta no encontrada
        }

        private int ManhattanDistance(CellInfo a, CellInfo b)
        {
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
        }

        // Reconstruye la ruta desde el nodo final hasta el nodo inicial usando nodos padres
        private CellInfo[] ReconstructPath(Node currentNode)
        {
            var path = new List<CellInfo>();
            while (currentNode != null)// Recorre los nodos padres desde el nodo final
            {
                path.Add(currentNode.Cell);// Añade la celda del nodo actual a la ruta
                currentNode = currentNode.Parent;// Avanza al nodo padre
            }
            path.Reverse();// Invierte la ruta para que vaya del inicio al final
            return path.ToArray();// Devuelve la ruta como un array
        }

        // Devuelve los vecinos 
        private IEnumerable<CellInfo> GetNeighbors(CellInfo cell)
        {
            int x = cell.x;
            int y = cell.y;
            List<CellInfo> neighbors = new List<CellInfo>();

            // Vecinos (arriba, abajo, izquierda, derecha)
            if (x > 0) neighbors.Add(_worldInfo[x - 1, y]);
            if (x < _worldInfo.WorldSize.x - 1) neighbors.Add(_worldInfo[x + 1, y]);
            if (y > 0) neighbors.Add(_worldInfo[x, y - 1]);
            if (y < _worldInfo.WorldSize.y - 1) neighbors.Add(_worldInfo[x, y + 1]);

            return neighbors;
        }
    }
}