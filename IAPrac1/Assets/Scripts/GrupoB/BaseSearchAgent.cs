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
        public int NumberOfDestinations => _zombies.Count + _treasures.Count; // Número total de destinos

        private WorldInfo _worldInfo;
        private INavigationAlgorithm _navigationAlgorithm; // Para calcular las rutas
        private Queue<CellInfo> _path; // Cola de ruta hasta el objetivo actual 

        private List<CellInfo> _zombies; // Lista de zombies en el mundo
        private List<CellInfo> _treasures; // Lista de cofres en el mundo

        public void Initialize(WorldInfo worldInfo, INavigationAlgorithm navigationAlgorithm)
        {
            _worldInfo = worldInfo;
            _navigationAlgorithm = navigationAlgorithm;
            _navigationAlgorithm.Initialize(worldInfo);

            // Inicializar listas con los zombies y cofres del mundo
            _zombies = _worldInfo.Enemies.ToList();
            _treasures = _worldInfo.Targets.ToList();
            SetClosestObjective(_worldInfo.FromVector3(Vector3.zero)); // Inicializar con el objetivo más cercano
        }

        public Vector3? GetNextDestination(Vector3 position)
        {
            CellInfo currentPosition = _worldInfo.FromVector3(position);

            // Actualizar posición de los zombies en cada ciclo
            if (_zombies.Count > 0)
            {
                UpdateZombiePositions();
            }

            // Verificar si el agente alcanzó el objetivo actual
            if (CurrentObjective != null && currentPosition == CurrentObjective)
            {
                OnCollisionWithObjective(CurrentObjective); // Actualizar lista de objetivos alcanzados
                                                            // Volver a evaluar el objetivo más cercano después de alcanzar uno
                SetClosestObjective(currentPosition); // Establecer el nuevo objetivo más cercano
                _path = null; // Limpiar la ruta para recalcularla
            }

            // Si ya no hay más zombies ni cofres, dirigirse a la salida
            if (NumberOfDestinations == 0 && _worldInfo.Exit != null)
            {
                if (CurrentObjective != _worldInfo.Exit) // Solo actualizar si no estamos ya en la salida
                {
                    CurrentObjective = _worldInfo.Exit; // Ir a la salida
                    _path = null; // Limpiar ruta para recalcular hacia la salida
                    Debug.Log("No hay más objetivos. Dirigiéndose a la salida.");
                }
            }

            // Recalcular la ruta si no existe o si se agotó al intentar alcanzar el objetivo
            if (_path == null || _path.Count == 0)
            {
                CellInfo[] path = _navigationAlgorithm.GetPath(currentPosition, CurrentObjective);
                if (path == null) return null;
                _path = new Queue<CellInfo>(path);
            }

            // Asignar el siguiente punto en la ruta hacia el objetivo actual
            if (_path.Count > 0)
            {
                CellInfo destination = _path.Dequeue();
                CurrentDestination = _worldInfo.ToWorldPosition(destination);
            }

            return CurrentDestination;
        }

        public void OnCollisionWithObjective(CellInfo objective)
        {
            Debug.Log($"Colisión detectada con: {objective}");

            if (_zombies.Contains(objective))
            {
                // Remover el zombie alcanzado y marcarlo como inactivo
                _zombies.Remove(objective);
                objective.GameObject.SetActive(false);  // Desactiva el GameObject del zombie atrapado
                Debug.Log($"Zombie {objective} atrapado.");

                if (_zombies.Count == 0)
                {
                    Debug.Log("Todos los zombies han sido atrapados. Ahora buscando cofres.");
                    SetClosestObjective(objective); // Cambiar el objetivo a los cofres más cercanos
                }
            }
            else if (_treasures.Contains(objective))
            {
                _treasures.Remove(objective); // Eliminar cofre alcanzado
                Debug.Log($"Cofre {objective} recogido.");

                if (_treasures.Count == 0 && _zombies.Count == 0 && _worldInfo.Exit != null)
                {
                    CurrentObjective = _worldInfo.Exit;
                    Debug.Log("No quedan zombies ni cofres. Dirigiéndose a la salida.");
                }
            }
            else
            {
                Debug.Log($"Objetivo no reconocido: {objective.Type}");
            }

            SetClosestObjective(objective);  // Actualiza el objetivo más cercano tras la colisión
        }

        private void UpdateZombiePositions()
        {
            // Asegúrate de que _worldInfo y _worldInfo.Enemies no son null
            if (_worldInfo == null || _worldInfo.Enemies == null)
            {
                Debug.LogError("WorldInfo or its Enemies property is null.");
                return;
            }

            // Filtrar enemigos que no sean null y que tengan un GameObject activo
            _zombies = _worldInfo.Enemies
                .Where(enemy => enemy != null && enemy.GameObject != null && enemy.GameObject.activeSelf)
                .ToList();

            // Debug para verificar cuántos zombies quedan
            Debug.Log($"Zombies restantes: {_zombies.Count}");
        }


        // Método para calcular el objetivo más cercano
        private void SetClosestObjective(CellInfo currentPosition)
        {
            float minDistance = float.MaxValue;
            CellInfo closestObjective = null;

            // Prioridad 1: Si hay zombies, selecciona el más cercano
            if (_zombies.Count > 0)
            {
                foreach (var zombie in _zombies)
                {
                    float distance = CalculateEuclideanDistance(currentPosition, zombie);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        closestObjective = zombie;
                    }
                }
            }
            // Prioridad 2: Si no quedan zombies, busca el cofre más cercano
            else if (_treasures.Count > 0)
            {
                foreach (var treasure in _treasures)
                {
                    float distance = CalculateEuclideanDistance(currentPosition, treasure);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        closestObjective = treasure;
                    }
                }
            }

            // Prioridad 3: Si no quedan ni zombies ni cofres, dirigirse a la salida
            CurrentObjective = closestObjective ?? _worldInfo.Exit;

            // Log para debug cuando se dirige a la salida
            if (CurrentObjective == _worldInfo.Exit)
            {
                Debug.Log("No hay más objetivos de interés. Dirigiéndose a la salida.");
            }
        }


        // Método para calcular la distancia entre dos puntos
        private float CalculateEuclideanDistance(CellInfo a, CellInfo b)
        {
            return Mathf.Sqrt(Mathf.Pow(a.x - b.x, 2) + Mathf.Pow(a.y - b.y, 2));
        }
    }
}