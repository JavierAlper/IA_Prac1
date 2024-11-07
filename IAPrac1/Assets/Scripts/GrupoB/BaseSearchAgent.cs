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
        public CellInfo CurrentObjective { get; private set; }
        public Vector3 CurrentDestination { get; private set; } //Para la ruta

        private WorldInfo _worldInfo;
        private INavigationAlgorithm _navigationAlgorithm;
        private Queue<CellInfo> _path;

        private List<CellInfo> _zombies; //Lista de cofres
        private List<CellInfo> _treasures; //Lista de tesoros
        public int NumberOfDestinations => _zombies.Count + _treasures.Count;

        public void Initialize(WorldInfo worldInfo, INavigationAlgorithm navigationAlgorithm)
        {
            _worldInfo = worldInfo;
            _navigationAlgorithm = navigationAlgorithm;
            _navigationAlgorithm.Initialize(worldInfo);

            _zombies = _worldInfo.Enemies.ToList();
            _treasures = _worldInfo.Targets.ToList();
            //para que establezca el CurrentObjetive más cercano
            SetClosestObjective(_worldInfo.FromVector3(Vector3.zero)); 
        }

        public Vector3? GetNextDestination(Vector3 position)
        {
            CellInfo currentPosition = _worldInfo.FromVector3(position);

            if (_zombies.Count > 0)
            {
                UpdateZombiesList();
            }

            // Procesar colisiones con objetos en el camino al CurrentObjetive
            HandleCollisions(currentPosition);

            // Si alcanzó el objetivo actual se busca el siguiente objetivo más cercano
            // y se iguala la ruta a null para recalcularla
            if (CurrentObjective != null && currentPosition == CurrentObjective)
            {
                OnCollisionWithObjective(CurrentObjective);
                SetClosestObjective(currentPosition);
                _path = null;
            }

            // Recalcular la ruta al nuevo CurrentObjetive
            if (_path == null || _path.Count == 0)
            {
                CellInfo[] path = _navigationAlgorithm.GetPath(currentPosition, CurrentObjective);
                if (path == null) return null;
                _path = new Queue<CellInfo>(path);
            }
            // si la cola _path tiene elementos
            // (si hay puntos pendientes en la ruta hacia el objetivo)
            if (_path.Count > 0)
            {
                //el destino es el primer elemento de la cola
                CellInfo destination = _path.Dequeue(); 
                //(siguiente celda en la ruta del agente hacia el CurrenteObjetive)
                CurrentDestination = _worldInfo.ToWorldPosition(destination);
            }

            return CurrentDestination;
        }

        private void HandleCollisions(CellInfo currentPosition)
        {
            // Si se encuentra con un objetivo aunque no sea el objetivo lo manejará
            //Eliminandolo de la lista correspondiente
            if (_zombies.Contains(currentPosition))
            {
                OnCollisionWithObjective(currentPosition);
            }
            else if (_treasures.Contains(currentPosition))
            {
                OnCollisionWithObjective(currentPosition);
            }
        }

        public void OnCollisionWithObjective(CellInfo objective)
        {
            Debug.Log($"Colisión detectada con: {objective}");

            if (_zombies.Contains(objective))
            {
                //Eliminar zombie de la lista
                _zombies.Remove(objective);
                Debug.Log($"Zombie {objective} atrapado.");
            }
            else if (_treasures.Contains(objective))
            {
                //Eliminar cofre de la lista
                _treasures.Remove(objective);
                Debug.Log($"Cofre {objective} recogido.");
            }
            else
            {
                Debug.Log($"Objetivo no reconocido: {objective.Type}");
            }
            //Actualiza el objetivo al más cercano desde la posición actual
            //(pos del objeto recien capturado)
            SetClosestObjective(objective); 
        }

        private void UpdateZombiesList()
        {
            //Gestionar que ni worldInfo ni la lista de enemigos sea nula
            if (_worldInfo == null || _worldInfo.Enemies == null)
            {
                return;
            }
            //Filtrar enemigos que no sean null y que tenga un gameObject activo
            _zombies = _worldInfo.Enemies
                .Where(enemy => enemy != null && enemy.GameObject != null && enemy.GameObject.activeSelf)
                .ToList();

            Debug.Log($"Zombies restantes: {_zombies.Count}");
        }

        private void SetClosestObjective(CellInfo currentPosition)
        {
            float minDistance = float.MaxValue;
            CellInfo closestObjective = null;
            //Prioridad 1: si hay zombies, selecciona el más cercano
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
            //Prioridad 2: Cuando no haya cofres pero si tesoros, selecciona el más cercano
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
            //Prioridad 3: no hay zombies ni tesoros objetivo = meta
            CurrentObjective = closestObjective ?? _worldInfo.Exit;
        }

        private float CalculateEuclideanDistance(CellInfo a, CellInfo b)
        {
            return Mathf.Sqrt(Mathf.Pow(a.x - b.x, 2) + Mathf.Pow(a.y - b.y, 2));
        }
    }
}