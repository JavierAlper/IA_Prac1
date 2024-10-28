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
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace grupoB
{
    public class PathFinding : INavigationAlgorithm
    {
        public enum Directions
        {
            None,
            Up,
            Right,
            Down,
            Left
        }

        private WorldInfo _world;
        private System.Random _random;
        private Directions _currentDirection = Directions.None;
        private int stepCount = 0;
        private List<Node> openList;
        private List<Node> visitedNode;

        public void Initialize(WorldInfo worldInfo)
        {
            _world = worldInfo;
            _random = new System.Random();
            openList = new List<Node>();
            visitedNode = new List<Node>();
        }

        public CellInfo[] GetPath(CellInfo startNode, CellInfo targetNode)
        {
            Debug.Log("primeraPatata");

            Node initNode = new Node(startNode, Heuristic(startNode,targetNode),0);
            Debug.Log("segundaPatata" + initNode.totalCost);
            openList.Add(initNode); //introducir en la openlist el nodo inicial
            Debug.Log("patata");
            int maxIterations = 8000;//para que no pete por bucle infinito
            int currentIteration = 0;
            while (openList.Any())// && currentIteration < maxIterations) //hacer mientras la openList no este vacia
            {
                currentIteration++;
                Debug.Log("patataEntro");
                //1. Coger y eliminar el nodo con menos coste de la open list 
                Node currentNode = openList.OrderBy(node => node.totalCost).FirstOrDefault(); //(se ordena la openList por coste y se coge el primer nodo)
                if(currentNode != null) openList.Remove(currentNode);

                //2. Si este nodo es igual al objetivo, reconstruimos el camino desde este nodo (fin if)
                if(currentNode.NodeCellInfo == targetNode) return ReconstructPath(currentNode);

                //3.Si no esta en la lista de visitados, se introduce
                if (!visitedNode.Contains(currentNode))
                {
                    visitedNode.Add(currentNode);

                    //4. para todos los sucesores del nodo se calcula su coste (for) (comprobar que el nodo sea accesible Walkable)
                    //creamos array con los vecinos de currentNode
                    CellInfo[] successors = new CellInfo[] { GetNeighbour(currentNode.NodeCellInfo, Directions.Up), GetNeighbour(currentNode.NodeCellInfo, Directions.Down), GetNeighbour(currentNode.NodeCellInfo, Directions.Left), GetNeighbour(currentNode.NodeCellInfo, Directions.Right) };
                    foreach(CellInfo successor in successors)
                    {
                        //5. si no esta en la lista de visitados y es Walkable se introduce en la openList
                        Node successorNode = new Node(successor); //!!!!!!!!!!!!!!!!!!!!!!!!!! falta calcular el coste que no se como hacer g(N)
                        if (!visitedNode.Contains(successorNode) && successorNode.NodeCellInfo.Walkable)
                        {

                            //6. se le pone como predecesor el nodo actual
                            successorNode.father = currentNode;

                            //7.calcular el coste del nodo y añadimos a la openList
                            successorNode.pathCost = successorNode.father.pathCost + 1; //calculamos coste acumulado
                            successorNode.totalCost = successorNode.pathCost + Heuristic(successorNode.NodeCellInfo, targetNode);//le sumamos al coste acumulado la heuristica

                            Node existingNode = openList.FirstOrDefault(node => node.NodeCellInfo == successorNode.NodeCellInfo);//miramos si ya esta el nodo en la openList
                            if(existingNode == null)
                            {
                                openList.Add(successorNode);//si no esta se introduce
                            }
                            else if(existingNode.totalCost > successorNode.totalCost)// si esta y tiene mayor coste se actualiza con el nuevo coste
                            {
                                existingNode.totalCost = successorNode.totalCost;
                                existingNode.pathCost = successorNode.pathCost;
                                existingNode.father = currentNode;
                            }
                           

                           
                        }

                    }

                }
               
            }
            Debug.Log("patataNoEntro");
            CellInfo[] nullPath = new CellInfo[1];
            return nullPath;
            /*CellInfo[] path = new CellInfo[1];

            if (_currentDirection == Directions.None || stepCount == 0)
            {
                _currentDirection = GetRandomDirection();
                stepCount = UnityEngine.Random.Range(3, 8);
            }

            CellInfo nextCell = GetNeighbour(startNode, _currentDirection);
            while (!nextCell.Walkable)
            {
                _currentDirection = GetRandomDirection();
                nextCell = GetNeighbour(startNode, _currentDirection);
                stepCount = UnityEngine.Random.Range(3, 8);
            }

            stepCount--;
            path[0] = nextCell;
            return path;*/
        }

        public CellInfo GetNeighbour(CellInfo current, Directions direction)
        {
            CellInfo neighbour;

            switch (direction)
            {
                case Directions.Up:
                    neighbour = _world[current.x, current.y - 1];
                    break;
                case Directions.Right:
                    neighbour = _world[current.x + 1, current.y];
                    break;
                case Directions.Down:
                    neighbour = _world[current.x, current.y + 1];
                    break;
                default:
                    neighbour = _world[current.x - 1, current.y];
                    break;
            }

            return neighbour;
        }

        public CellInfo GetRandomNeighbour(CellInfo cell)
        {
            CellInfo neighbour;

            do
            {
                Directions direction = GetRandomDirection();
                neighbour = GetNeighbour(cell, direction);
            } while (!neighbour.Walkable);

            return neighbour;
        }

        public Directions GetRandomDirection()
        {
            float randomFloat = (float)_random.NextDouble() * 100f;

            if (randomFloat < 25f)
            {
                return Directions.Up;
            }
            if (randomFloat < 50f)
            {
                return Directions.Right;
            }
            if (randomFloat < 75f)
            {
                return Directions.Down;
            }
            {
                return Directions.Left;
            }
        }

        public CellInfo[] ReconstructPath(Node finalNode)
        {
            Node currentNode = finalNode;
            List<CellInfo> cellPath = new List<CellInfo>();//creamos lista de cellInfo
            while(currentNode.father != null)//hasta que no tenga padre (hasta que no lleguemos al nodo inicio del camino)
            {
                cellPath.Add(currentNode.NodeCellInfo);//insertamos en la lista
                currentNode = currentNode.father;//vamos al nodo padre

            }
            cellPath.Add(currentNode.NodeCellInfo);//insertamos el nodo de inicio
            return cellPath.ToArray();//lo devolvemos en un array
        }
        public int Heuristic(CellInfo current, CellInfo obj)
        {
            //Manhattan Distance
            return Mathf.Abs(current.x - obj.x) + Mathf.Abs(current.y - obj.y);
            
        }
    }
}