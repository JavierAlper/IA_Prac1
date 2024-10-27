using Navigation.World;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace grupoB
{
    public class Node : MonoBehaviour
    {
        public CellInfo NodeCellInfo { get; private set; }
        public int cost { get; set; }
        public Node father { get; set; }

        public Node(CellInfo nodeCellInfo, int cost, Node father)
        {
            NodeCellInfo = nodeCellInfo;
            this.cost = cost;
            this.father = father;
        }

        public Node(CellInfo nodeCellInfo, int cost)
        {
            NodeCellInfo = nodeCellInfo;
            this.cost = cost;
        }

        public Node(CellInfo nodeCellInfo)
        {
            NodeCellInfo = nodeCellInfo;
        }
    }
}

