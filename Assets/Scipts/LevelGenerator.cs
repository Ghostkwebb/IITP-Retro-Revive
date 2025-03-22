using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Mirror;  // Import Mirror namespace

public class LevelGenerator : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnSeedChanged))]
    public int mazeSeed;

    public int width = 20;
    public int height = 15;
    private int[,] grid;
    public Tilemap tilemap;
    public TileBase wallTile;
    
    // Networked prefabs – these will be spawned only by the server.
    public GameObject pacmanPrefab;
    public GameObject ghostPrefab;
    public GameObject coinPrefab;
    public GameObject bigCoinPrefab;
    public int numberOfPlayers = 2;
    public int numberOfGhosts = 2;

    private List<Vector3Int> pacmanSpawnPositions = new List<Vector3Int>();
    private List<Vector3Int> ghostSpawnPositions = new List<Vector3Int>();

    // This method is called only on the server.
    public override void OnStartServer()
    {
        // Pick a random seed
        mazeSeed = Random.Range(0, 100000);
        
        // Generate the maze for the tilemap on the server
        Debug.Log("OnStartClient called, mazeSeed = " + mazeSeed);
        GenerateMaze();

        // Spawn networked objects (players, ghosts, coins)
        SpawnNetworkedObjects();
    }

    // This method is called on clients when they start.
    public override void OnStartClient()
    {
        GenerateMaze();
    }

    // This hook is called when mazeSeed changes.
    void OnSeedChanged(int oldSeed, int newSeed)
    {
        // When the seed updates on a client, regenerate the maze.
        Debug.Log($"OnSeedChanged: oldSeed={oldSeed}, newSeed={newSeed}");
        GenerateMaze();
    }

    // --- Maze Generation Logic (runs on both server and client) ---

    void GenerateMaze()
    {
        Debug.Log("OnStartClient called, mazeSeed = " + mazeSeed);
        Random.InitState(mazeSeed);

        grid = new int[width, height];
        InitializeGrid();
        GeneratePaths(1, 1);
        VisualizeLevel();
    }

    void InitializeGrid()
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                grid[x, y] = 0; // 0 represents a wall
    }

    void GeneratePaths(int startX, int startY)
    {
        Stack<Vector2Int> stack = new Stack<Vector2Int>();
        stack.Push(new Vector2Int(startX, startY));
        grid[startX, startY] = 1; // Mark starting cell as path

        while (stack.Count > 0)
        {
            Vector2Int current = stack.Peek();
            int x = current.x;
            int y = current.y;

            List<Vector2Int> neighbors = GetUnvisitedNeighbors(x, y);

            if (neighbors.Count > 0)
            {
                Vector2Int next = neighbors[Random.Range(0, neighbors.Count)];

                // Carve a path between current and next
                int dx = next.x - x;
                int dy = next.y - y;
                if (dx != 0) grid[x + dx / Mathf.Abs(dx), y] = 1;
                if (dy != 0) grid[x, y + dy / Mathf.Abs(dy)] = 1;

                grid[next.x, next.y] = 1;
                stack.Push(next);
            }
            else
            {
                stack.Pop();
            }
        }
    }

    List<Vector2Int> GetUnvisitedNeighbors(int x, int y)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();
        TryAddNeighbor(neighbors, x + 2, y);
        TryAddNeighbor(neighbors, x - 2, y);
        TryAddNeighbor(neighbors, x, y + 2);
        TryAddNeighbor(neighbors, x, y - 2);
        return neighbors;
    }

    void TryAddNeighbor(List<Vector2Int> neighbors, int x, int y)
    {
        if (x > 0 && x < width - 1 && y > 0 && y < height - 1 && grid[x, y] == 0)
            neighbors.Add(new Vector2Int(x, y));
    }

    void VisualizeLevel()
    {
        if (tilemap == null || wallTile == null)
        {
            Debug.LogError("Tilemap or Wall Tile is not assigned!");
            return;
        }

        tilemap.ClearAllTiles();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (grid[x, y] == 0)
                {
                    Vector3Int tilePosition = new Vector3Int(x, -y, 0);
                    tilemap.SetTile(tilePosition, wallTile);
                }
            }
        }
    }

    // --- Spawning Networked Objects (Server Only) ---

    void SpawnNetworkedObjects()
    {
        SpawnPlayers();
        SpawnGhosts();
        PlaceCoins();
        PlaceBigCoins();
    }

    void SpawnPlayers()
    {
        pacmanSpawnPositions.Clear();
        List<Vector3Int> possibleSpawns = new List<Vector3Int>();
        for (int x = 1; x < width - 1; x++)
            for (int y = 1; y < height - 1; y++)
                if (grid[x, y] == 1)
                    possibleSpawns.Add(new Vector3Int(x, -y, 0));

        possibleSpawns.Shuffle();
        int playersSpawned = 0;
        foreach (Vector3Int spawnPos in possibleSpawns)
        {
            if (playersSpawned < numberOfPlayers)
            {
                GameObject pacman = Instantiate(pacmanPrefab, tilemap.GetCellCenterWorld(spawnPos), Quaternion.identity);
                NetworkServer.Spawn(pacman);
                pacmanSpawnPositions.Add(spawnPos);
                playersSpawned++;
            }
            else
            {
                break;
            }
        }
        if (pacmanSpawnPositions.Count < numberOfPlayers)
            Debug.LogWarning("Could not find enough spawn points for all Pac-Men.");
    }

    void SpawnGhosts()
    {
        ghostSpawnPositions.Clear();
        int centerX = width / 2;
        int centerY = height / 2;
        int ghostsSpawned = 0;
        int spawnAttempts = 0;
        int maxSpawnAttempts = 10;

        while (ghostsSpawned < numberOfGhosts && spawnAttempts < maxSpawnAttempts)
        {
            int randomX = Random.Range(Mathf.Max(1, centerX - 5), Mathf.Min(width - 1, centerX + 6));
            int randomY = Random.Range(Mathf.Max(1, centerY - 5), Mathf.Min(height - 1, centerY + 6));
            Vector3Int spawnPos = new Vector3Int(randomX, -randomY, 0);
            if (grid[randomX, randomY] == 1 &&
                !pacmanSpawnPositions.Contains(spawnPos) &&
                !ghostSpawnPositions.Contains(spawnPos))
            {
                GameObject ghost = Instantiate(ghostPrefab, tilemap.GetCellCenterWorld(spawnPos), Quaternion.identity);
                NetworkServer.Spawn(ghost);
                ghostSpawnPositions.Add(spawnPos);
                ghostsSpawned++;
            }
            spawnAttempts++;
        }
        if (ghostsSpawned < numberOfGhosts)
            Debug.LogWarning($"Could only spawn {ghostsSpawned} out of {numberOfGhosts} ghosts.");
    }

    void PlaceCoins()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3Int coinPos = new Vector3Int(x, -y, 0);
                bool isSpawnPoint = pacmanSpawnPositions.Contains(coinPos) || ghostSpawnPositions.Contains(coinPos);
                if (grid[x, y] == 1 && !isSpawnPoint)
                {
                    GameObject coin = Instantiate(coinPrefab, tilemap.GetCellCenterWorld(coinPos), Quaternion.identity);
                    NetworkServer.Spawn(coin);
                }
            }
        }
    }

    void PlaceBigCoins()
    {
        int bigCoinsToPlace = 4;
        List<Vector3Int> possibleLocations = new List<Vector3Int>();
        for (int x = 1; x < width - 1; x++)
            for (int y = 1; y < height - 1; y++)
                if (grid[x, y] == 1)
                    possibleLocations.Add(new Vector3Int(x, -y, 0));
        possibleLocations.Shuffle();
        for (int i = 0; i < Mathf.Min(bigCoinsToPlace, possibleLocations.Count); i++)
        {
            GameObject bigCoin = Instantiate(bigCoinPrefab, tilemap.GetCellCenterWorld(possibleLocations[i]), Quaternion.identity);
            NetworkServer.Spawn(bigCoin);
        }
    }
}

// Extension method to shuffle a list
public static class ListExtensions
{
    public static void Shuffle<T>(this IList<T> list)
    {
        System.Random rng = new System.Random();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}
