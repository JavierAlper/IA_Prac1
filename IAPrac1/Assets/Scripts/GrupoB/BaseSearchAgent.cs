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
using System.Linq;
using Navigation.Interfaces;
using Navigation.World;
using UnityEngine;

namespace grupoB
{
    public class BaseSearchAgent : INavigationAgent
    {
        public CellInfo CurrentObjective { get; private set; } // Objetivo actual
        public Vector3 CurrentDestination { get; private set; } // Coordenadas del objetivo actual
        public int NumberOfDestinations { get; private set; } // Número de destinos a alcanzar

        private WorldInfo _worldInfo;
        private INavigationAlgorithm _navigationAlgorithm; // Para calcular las rutas
        private Queue<CellInfo> _path; // Cola de ruta hasta el objetivo actual 
        private List<CellInfo> _objectives; // Lista de objetivos (tesoros y salida)

        public void Initialize(WorldInfo worldInfo, INavigationAlgorithm navigationAlgorithm)
        {
            _worldInfo = worldInfo;
            _navigationAlgorithm = navigationAlgorithm;
            _navigationAlgorithm.Initialize(worldInfo);
            _objectives = new List<CellInfo>(GetDestinations()); // Cargar los objetivos en la lista
            NumberOfDestinations = _objectives.Count; // Cantidad de objetivos
        }

        public Vector3? GetNextDestination(Vector3 position)
        {
            CellInfo currentPosition = _worldInfo.FromVector3(position);

            // Establecer el primer objetivo al inicio si no está asignado
            if (CurrentObjective == null)
            {
                SetClosestObjective(currentPosition); // Seleccionar el objetivo más cercano
            }

            // Verificar si se ha llegado al objetivo actual
            if (CurrentObjective != null && currentPosition == CurrentObjective)
            {
                _objectives.Remove(CurrentObjective); // Eliminar el objetivo alcanzado

                if (_objectives.Count > 0)
                {
                    SetClosestObjective(currentPosition); // Cambiar al siguiente objetivo más cercano
                }
                else
                {
                    // No hay más cofres, ahora podemos ir a la salida
                    CurrentObjective = _worldInfo.Exit; // Establecer la salida como el nuevo objetivo
                }
                _path = null; // Limpiar la ruta para recalcularla
            }

            // Si no existe una ruta calculada o la actual se ha agotado, se recalcula hacia el objetivo actual
            if (_path == null || _path.Count == 0)
            {
                CellInfo[] path = _navigationAlgorithm.GetPath(currentPosition, CurrentObjective);
                if (path == null) return null; // Si no hay ruta disponible, retorna null
                _path = new Queue<CellInfo>(path); // Cargar la nueva ruta en la cola
            }

            // Si la ruta tiene puntos, se establece el siguiente destino
            if (_path.Count > 0)
            {
                CellInfo destination = _path.Dequeue(); // Siguiente punto en la ruta
                CurrentDestination = _worldInfo.ToWorldPosition(destination);
            }

            return CurrentDestination; // Devuelve el siguiente destino
        }

        // Método para calcular el objetivo más cercano
        private void SetClosestObjective(CellInfo currentPosition)
        {
            float minDistance = float.MaxValue;
            CellInfo closestObjective = null;

            foreach (var objective in _objectives)
            {
                float distance = CalculateEuclideanDistance(currentPosition, objective);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestObjective = objective;
                }
            }
            CurrentObjective = closestObjective; // Establece el objetivo más cercano
        }

        // Método para calcular la distancia 
        private float CalculateEuclideanDistance(CellInfo a, CellInfo b)
        {
            return Mathf.Sqrt(Mathf.Pow(a.x - b.x, 2) + Mathf.Pow(a.y - b.y, 2));
        }

        // Obtener destinos incluyendo todos los cofres antes de la salida
        private CellInfo[] GetDestinations()
        {
            List<CellInfo> targets = new List<CellInfo>(_worldInfo.Targets); // Añade todos los cofres
            return targets.ToArray(); // Devuelve un array de objetivos
        }
    }
}