using System;
using System.Collections.Generic;
using UnityEngine;
using EasyButtons;
using Random = UnityEngine.Random;

[ExecuteInEditMode]
public class BrickGenrator : MonoBehaviour
{
    [SerializeField] private Vector2 _spawnAreaSize = Vector2.one;

    private Vector2 _tileSize;

    private Queue<GameObject> _spannedBricks = new Queue<GameObject>();

    [SerializeField] private GameObject[] _tilePrefab;

    [SerializeField] private float[] _tileSpawnningRate;

    private int[] _tileSpawnned;
    private int[] _maxTileSpawn;
    int _tileSpawned;
    int _totalTileToSpawn;
    int _totalCanSpawn;

    /// <summary>
    /// Makes the tiles based on the size given with the spawn area size.
    /// </summary>
    [Button]
    public void GenrateTiles()
    {
        if (_tilePrefab.Length != _tileSpawnningRate.Length)
        {
            // if both arrays dont have the same length.
            Debug.LogError(
                "Not all Tiles have a percentage Defined. Please Check array Tile Spanning Rate & Tile Prefab");
            return;
        }

        DeleteAllBricks();

        _tileSize = Vector2.zero;
        _tileSize = _tilePrefab[0].GetComponent<BoxCollider2D>().size;

        _totalCanSpawn = 0;
        _tileSpawned = 0;

        _totalTileToSpawn = 0;
        _totalTileToSpawn = (((int) _spawnAreaSize.x * (int) _spawnAreaSize.y) * 2);

        _tileSpawnned = new int[_tileSpawnningRate.Length];
        _maxTileSpawn = new int[_tileSpawnningRate.Length];

        CalculateMaxSpawning();

        if (_totalCanSpawn < _totalTileToSpawn)
        {
            Debug.LogError(
                "Total Tiles to Spawn is Less the Number of tiles that can be spawnned. Check Percentage Numbers and if their total is 100");
            return;
        }


        float spawnLocY = 0f;
        spawnLocY = transform.position.y; // 1
        spawnLocY += (_spawnAreaSize.y * 0.5f); // 1 + (4*0.5)
        spawnLocY -= _tileSize.y * 0.5f;

        // multiplying this by 2 because tile size is 0.5 on y.
        //can be made more robust.
        for (int y = 0; y < _spawnAreaSize.y * 2; y++)
        {
            float spawnLocX = 0f;
            spawnLocX = transform.position.x;
            spawnLocX += -(_spawnAreaSize.x * 0.5f);
            spawnLocX += _tileSize.x * 0.5f; //  0.5f

            for (int x = 0; x < _spawnAreaSize.x; x++)
            {
                var randomObj = GetRandomTile();
                if (randomObj != null)
                {
                    var instobj = Instantiate(randomObj);
                    var pos = new Vector2(spawnLocX, spawnLocY);
                    instobj.transform.position = pos;
                    instobj.transform.parent = this.transform;
                    _spannedBricks.Enqueue(instobj);
                    spawnLocX += _tileSize.x;
                }
            }

            spawnLocY -= _tileSize.y;
        }
    }

    /// <summary>
    /// Gets a random available tile based on the spawning percent defined in Calculate Max Spawnning
    /// </summary>
    /// <returns></returns>
    private GameObject GetRandomTile()
    {
        bool foundTile = false;
        if (_tileSpawned >= _totalTileToSpawn)
        {
            return null;
        }

        while (foundTile == false)
        {
            int tileToSpawn = Random.Range(0, _tilePrefab.Length);
            if (_tileSpawnned[tileToSpawn] <= _maxTileSpawn[tileToSpawn])
            {
                foundTile = true;
                _tileSpawned++;
                _tileSpawnned[tileToSpawn]++;
                return _tilePrefab[tileToSpawn];
            }
        }

        return null;
    }

    /// <summary>
    /// calculates how many times a tile can spawn, calculated by the formula -> (value*total)/100
    /// </summary>
    private void CalculateMaxSpawning()
    {
        for (int i = 0; i < _tileSpawnningRate.Length; i++)
        {
            _maxTileSpawn[i] = 0;
            var value = _tileSpawnningRate[i];
            _maxTileSpawn[i] = Mathf.CeilToInt((value * _totalTileToSpawn) / 100);
            _totalCanSpawn += _maxTileSpawn[i];
        }
    }

    /// <summary>
    /// Deletes all the bricks that are the child of this gameObject
    /// </summary>
    [Button]
    private void DeleteAllBricks()
    {
        if (transform.childCount < 0)
        {
            return;
        }

        // If case some tile didnt enqueue while spawnning then add it to the queue.
        //NOTE: This is not efficient. can do better by not making a queue and deleting all child object. 
        foreach (Transform child in transform)
        {
            if (_spannedBricks.Contains(child.gameObject) == false)
            {
                _spannedBricks.Enqueue(child.gameObject);
            }
        }

        // Delete all tiles in the queue.
        int num = _spannedBricks.Count;
        for (int i = 0; i < num; i++)
        {
            var obj = _spannedBricks.Dequeue();
            DestroyImmediate(obj);
        }
    }

    /// <summary>
    /// Visually draw a box to show the area of to be spawnned.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireCube(transform.position, _spawnAreaSize);
    }
}