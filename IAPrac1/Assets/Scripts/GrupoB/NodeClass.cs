using Navigation.World;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace grupoB
{
    public class Node
    {
        public CellInfo Cell { get; } //celda del nodo
        public Node Parent { get; } //nodo padre de el actual en la ruta
        public int GCost { get; } // Coste acumulado desde el inicio hasta este nodo
        public int HCost { get; } // Heur�stica hasta el objetivo (distancia de Manhattan)
        public int FCost => GCost + HCost; // GCost + HCost (coste total del nodo)

        // Constructor para el nodo inicial
        public Node(CellInfo cell, int gCost, int hCost)
        {
            Cell = cell;
            GCost = gCost;
            HCost = hCost;
            Parent = null; //el inicial no tendr� nodo padre
        }

        // Constructor para dem�s nodos
        public Node(CellInfo cell, int gCost, int hCost, Node parent)
        {
            Cell = cell;
            GCost = gCost;
            HCost = hCost;
            Parent = parent;
        }
    }
}

