﻿using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using UnityObject = UnityEngine.Object;

public class LevelGenerator : IInitializable
{
    private readonly ElevatorDirection[] ALL_DIRECTIONS = {ElevatorDirection.North, ElevatorDirection.East,
            ElevatorDirection.South, ElevatorDirection.West};

    [Inject]
    private ScoreManager _scoreManager;

    public double MinDistance = 40;
    public double MaxDistance = 130;

    private System.Random _rng;
    private Dictionary<ElevatorDirection, List<Enemy>> _enemies = new Dictionary<ElevatorDirection, List<Enemy>>();
    private List<GameObject> _targets = new List<GameObject>();
    private int _seed;

    public int Seed
    {
        get
        {
            if (_seed == 0)
            {
                _seed = new System.Random().Next();
            }
            return _seed;
        }
        set { _seed = value; }
    }

    public int NumberOfTargetsAlive
    {
        get
        {
            var count = 0;
            foreach (var enemies in _enemies.Values)
            {
                foreach (var enemy in enemies)
                {
                    if (enemy.Alive)
                    {
                        count++;
                    }
                }
            }
            return count;
        }
    }

    public void Initialize()
    {
        foreach (var direction in ALL_DIRECTIONS)
        {
            _enemies[direction] = new List<Enemy>();
        }
        foreach (var enemy in UnityObject.FindObjectsOfType<Enemy>())
        {
            _enemies[enemy.Direction].Add(enemy);
        }
        foreach (var target in GameObject.FindGameObjectsWithTag("Target"))
        {
            _targets.Add(target);
        }
        Reset();
    }

    public void Reset()
    {
        _rng = new System.Random(Seed);
        FinishLevel();
    }

    public void FinishLevel()
    {
        foreach (var enemies in _enemies.Values)
        {
            foreach (var enemy in enemies)
            {
                enemy.ResetEnemy();
            }
        }
        _scoreManager.NextLevel();

        var directions = GetElevatorSidesForLevel();
        Debug.Log("Starting level " + _scoreManager.Level);

        var availableDirections = new List<ElevatorDirection>(ALL_DIRECTIONS);
        var spawnableEnemies = new List<Enemy>();
        for (var i = 0; i < directions; i++)
        {
            var directionIndex = _rng.Next(availableDirections.Count);
            var direction = availableDirections[directionIndex];
            availableDirections.RemoveAt(directionIndex);

            spawnableEnemies.AddRange(_enemies[direction]);
        }
        var numberOfSpawns = GetTargetSpawnsForLevel();
        for (var i = 0; i < Math.Min(numberOfSpawns, spawnableEnemies.Count); i++)
        {
            var enemyIndex = _rng.Next(spawnableEnemies.Count);
            var enemy = spawnableEnemies[_rng.Next(enemyIndex)];
            enemy.Show();
            spawnableEnemies.RemoveAt(enemyIndex);
        }
        Debug.Log("Spawned " + numberOfSpawns + " targets");
    }

    private int GetTargetSpawnsForLevel()
    {
        if (_scoreManager.Level <= 3)
        {
            return _scoreManager.Level + 1;
        }
        if (_scoreManager.Level < 10)
        {
            return _rng.Next(3, 5);
        }
        if (_scoreManager.Level < 20)
        {
            return _rng.Next(3, 10);
        }
        if (_scoreManager.Level < 25)
        {
            return _rng.Next(10, 20);
        }
        if (_scoreManager.Level < 30)
        {
            return _rng.Next(15, 30);
        }
        return _scoreManager.Level < 40 ? _rng.Next(25, 40) : 40;
    }

    private int GetElevatorSidesForLevel()
    {
        if (_scoreManager.Level < 3)
        {
            return 1;
        }
        if (_scoreManager.Level == 3)
        {
            return 2;
        }
        if (_scoreManager.Level < 10)
        {
            return _rng.Next(1, 2);
        }
        if (_scoreManager.Level < 15)
        {
            return _rng.Next(1, 3);
        }
        if (_scoreManager.Level < 20)
        {
            return _rng.Next(2, 3);
        }
        return _scoreManager.Level < 25 ? _rng.Next(3, 4) : 4;
    }

    public ElevatorDirection GetRandomDirection(List<ElevatorDirection> directions)
    {
        return directions[_rng.Next(directions.Count)];
    }

    public enum ElevatorDirection
    {
        North = 5,
        South = 95,
        West = 185,
        East = 275
    }
}