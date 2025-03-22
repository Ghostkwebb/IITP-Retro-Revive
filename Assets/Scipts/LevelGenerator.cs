using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Mirror;

public class LevelGenerator : Mirror.NetworkBehaviour
{
    public int width = 20;
    public int height = 15;
    private int[,] grid;
    public Tilemap tilemap;
    public TileBase wallTile;
    public GameObject pacmanPrefab;
    public GameObject ghostPrefab;
    public GameObject coinPrefab; // For regular coins
    public GameObject bigCoinPrefab; // For power-up coins
    public int numberOfPlayers = 2; // Example for multiplayer
    public int numberOfGhosts = 2; // Example for multiplayer

    private List<Vector3Int> pacmanSpawnPositions = new List<Vector3Int>();
    private List<Vector3Int> ghostSpawnPositions = new List<Vector3Int>();

    void Start()
    {
        if (!isServer) return;

        grid = new int[width, height];
        InitializeGrid();
        GeneratePaths(1, 1);

        SendLevelToClients();
        SpawnGameElementsServer();
    }

    void InitializeGrid()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                grid[x, y] = 0; // 0 represents a wall
            }
        }
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

                // Carve a path (mark as 1) between current and next
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

        // Check neighbors (up, down, left, right)
        TryAddNeighbor(neighbors, x + 2, y);
        TryAddNeighbor(neighbors, x - 2, y);
        TryAddNeighbor(neighbors, x, y + 2);
        TryAddNeighbor(neighbors, x, y - 2);

        return neighbors;
    }

    void TryAddNeighbor(List<Vector2Int> neighbors, int x, int y)
    {
        if (x > 0 && x < width - 1 && y > 0 && y < height - 1 && grid[x, y] == 0)
        {
            neighbors.Add(new Vector2Int(x, y));
        }
    }

    void VisualizeLevelServer()
    {

    }

    void SendLevelToClients()
    {
        int[] flatGrid = new int[width * height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                flatGrid[y * width + x] = grid[x, y];
            }
        }
        VisualizeLevelClient(flatGrid); // Send the 1D array
    }

    [ClientRpc]
    public void VisualizeLevelClient(int[] levelData)
    {
        Debug.Log("VisualizeLevelClient called on client.");
        if (levelData == null)
        {
            Debug.LogError("Level data is null on the client!");
            return;
        }
        Debug.Log("Level data length on client: " + levelData.Length);
        
        if (tilemap == null || wallTile == null)
        {
            Debug.LogError("Tilemap or Wall Tile is not assigned in the Inspector!");
            return;
        }

        tilemap.ClearAllTiles();

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // same formula => if (levelData[y * width + x] == 0)
                if (levelData[y * width + x] == 0)
                {
                    Vector3Int tilePosition = new Vector3Int(x, -y, 0);
                    tilemap.SetTile(tilePosition, wallTile);
                }

            }
        }


    }

    void SpawnGameElementsServer()
    {
        SpawnPlayersServer();
        SpawnGhostsServer();
        PlaceCoinsServer();
        PlaceBigCoinsServer();
    }

    void SpawnPlayersServer()
    {
        pacmanSpawnPositions.Clear();
        List<Vector3Int> possibleSpawns = new List<Vector3Int>();
        for (int x = 1; x < width - 1; x++)
        {
            for (int y = 1; y < height - 1; y++)
            {
                if (grid[x, y] == 1)
                {
                    possibleSpawns.Add(new Vector3Int(x, -y, 0));
                }
            }
        }

        possibleSpawns.Shuffle();

        int playersSpawned = 0;
        foreach (Vector3Int spawnPos in possibleSpawns)
        {
            if (playersSpawned < numberOfPlayers)
            {
                GameObject player = Instantiate(pacmanPrefab, tilemap.GetCellCenterWorld(spawnPos), Quaternion.identity);
                NetworkServer.Spawn(player); // Use NetworkServer.Spawn
                pacmanSpawnPositions.Add(spawnPos);
                playersSpawned++;
            }
            else
            {
                break;
            }
        }

        if (pacmanSpawnPositions.Count < numberOfPlayers)
        {
            Debug.LogWarning("Could not find enough spawn points for all Pac-Men.");
        }
    }

    void SpawnGhostsServer()
    {
        ghostSpawnPositions.Clear();
        int centerX = width / 2;
        int centerY = height / 2;
        int ghostsSpawned = 0;
        int spawnAttempts = 0;
        int maxSpawnAttempts = 10; // To prevent infinite loops

        while (ghostsSpawned < numberOfGhosts && spawnAttempts < maxSpawnAttempts)
        {
            int randomX = Random.Range(Mathf.Max(1, centerX - 5), Mathf.Min(width - 1, centerX + 6));
            int randomY = Random.Range(Mathf.Max(1, centerY - 5), Mathf.Min(height - 1, centerY + 6));
            Vector3Int spawnPos = new Vector3Int(randomX, -randomY, 0);

            if (grid[randomX, randomY] == 1 && !pacmanSpawnPositions.Contains(spawnPos) && !ghostSpawnPositions.Contains(spawnPos))
            {
                GameObject ghost = Instantiate(ghostPrefab, tilemap.GetCellCenterWorld(spawnPos), Quaternion.identity);
                NetworkServer.Spawn(ghost); // Use NetworkServer.Spawn
                ghostSpawnPositions.Add(spawnPos);
                ghostsSpawned++;
            }
            spawnAttempts++;
        }

        if (ghostsSpawned < numberOfGhosts)
        {
            Debug.LogWarning($"Could only spawn {ghostsSpawned} out of {numberOfGhosts} ghosts in the central area.");
        }
    }
    void PlaceCoinsServer()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3Int coinPos = new Vector3Int(x, -y, 0);
                bool isSpawnPoint = false;
                foreach (var spawn in pacmanSpawnPositions)
                {
                    if (coinPos == spawn)
                    {
                        isSpawnPoint = true;
                        break;
                    }
                }
                foreach (var spawn in ghostSpawnPositions)
                {
                    if (coinPos == spawn)
                    {
                        isSpawnPoint = true;
                        break;
                    }
                }

                if (grid[x, y] == 1 && !isSpawnPoint)
                {
                    GameObject coin = Instantiate(coinPrefab, tilemap.GetCellCenterWorld(coinPos), Quaternion.identity);
                    NetworkServer.Spawn(coin); // Use NetworkServer.Spawn
                }
            }
        }
    }

    void PlaceBigCoinsServer()
    {
        int bigCoinsToPlace = 4; // You can adjust this number
        List<Vector3Int> possibleLocations = new List<Vector3Int>();

        for (int x = 1; x < width - 1; x++)
        {
            for (int y = 1; y < height - 1; y++)
            {
                if (grid[x, y] == 1)
                {
                    possibleLocations.Add(new Vector3Int(x, -y, 0));
                }
            }
        }

        possibleLocations.Shuffle();

        for (int i = 0; i < Mathf.Min(bigCoinsToPlace, possibleLocations.Count); i++)
        {
            GameObject bigCoin = Instantiate(bigCoinPrefab, tilemap.GetCellCenterWorld(possibleLocations[i]), Quaternion.identity);
            NetworkServer.Spawn(bigCoin); // Use NetworkServer.Spawn
        }
    }
}

// Extension method to shuffle a list (from StackOverflow)
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