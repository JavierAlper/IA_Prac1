using Navigation.World;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace grupoB
{
    public class Node : MonoBehaviour
    {
        public CellInfo NodeCellInfo { get; private set; }
        public int totalCost { get; set; }
        public int pathCost { get; set; }
        public Node father { get; set; }

        public Node(CellInfo nodeCellInfo, int cost, int pathCost, Node father)
        {
            NodeCellInfo = nodeCellInfo;
            this.totalCost = cost;
            this.pathCost = pathCost;
            this.father = father;
        }

        public Node(CellInfo nodeCellInfo, int cost, int pathCost)
        {
            NodeCellInfo = nodeCellInfo;
            this.totalCost = cost;
            this.pathCost = pathCost;
            this.father = null;
        }

        public Node(CellInfo nodeCellInfo, int cost)
        {
            NodeCellInfo = nodeCellInfo;
            this.totalCost = cost;
            this.pathCost = 0;
            this.father = null;
        }

        public Node(CellInfo nodeCellInfo)
        {
            NodeCellInfo = nodeCellInfo;
            this.totalCost = 0;
            this.pathCost = 0;
            this.father = null;
        }
    }
}

