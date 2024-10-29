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

using System.Collections.Generic;
using Navigation.Interfaces;
using Navigation.World;
using UnityEngine;

namespace grupoB
{
    public class BaseSearchAgent : INavigationAgent
    {
        public CellInfo CurrentObjective { get; private set; } //Objetivo actual (salida)
        public Vector3 CurrentDestination { get; private set; } //Coordenadas del iobj actual
        public int NumberOfDestinations { get; private set; } //nº destinos a alcanzar (para mas adelante)

        private WorldInfo _worldInfo;
        private INavigationAlgorithm _navigationAlgorithm; //para calcular las rutas
        private CellInfo[] _objectives; //objetivos
        private Queue<CellInfo> _path; //cola de ruta hasta el obj actual 

        public void Initialize(WorldInfo worldInfo, INavigationAlgorithm navigationAlgorithm)
        {
            _worldInfo = worldInfo;
            _navigationAlgorithm = navigationAlgorithm;
            _navigationAlgorithm.Initialize(worldInfo); 
        }

        public Vector3? GetNextDestination(Vector3 position)
        {
            // Si no se han cargado los objetivos, se inicializan y establece el objetivo actual
            if (_objectives == null)
            {
                _objectives = GetDestinations(); // Obtiene los destinos (por ahora la salida)
                CurrentObjective = _objectives[_objectives.Length - 1];
                NumberOfDestinations = _objectives.Length; 
            }
            // Si no existe una ruta calculada o la actual se ha agotado, se recalcula
            if (_path == null || _path.Count == 0)
            {
                CellInfo currentPosition = _worldInfo.FromVector3(position);
                CellInfo[] path = _navigationAlgorithm.GetPath(currentPosition, CurrentObjective); //calcula la ruta
                _path = new Queue<CellInfo>(path); // Carga la ruta calculada en una cola
            }
            // Si la ruta tiene puntos, se establece el siguiente destino
            if (_path.Count > 0)
            {
                CellInfo destination = _path.Dequeue(); // siguiente punto en la ruta
                CurrentDestination = _worldInfo.ToWorldPosition(destination);
            }

            return CurrentDestination; //devuelve el siguiente destino
        }

        //Obtener destinos 
        private CellInfo[] GetDestinations()
        {
            var targets = new List<CellInfo> { _worldInfo.Exit };
            return targets.ToArray(); //array de objetivos
        }
    }
}