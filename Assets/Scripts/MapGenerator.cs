using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapGenerator : MonoBehaviour
{
    class Point {
        public int x, y;
    }
    Tilemap tilemap;
    public Tilemap backgroundTilemap;
    public TileBase wallTile, plantTile;
    public TileBase[] singlePlantTiles;
    public TileBase[] empty = new TileBase[32 * 32];
    public GameObject player;
    public Vector2 caveScale = new Vector2(0.7f,1.4f), plantScale = new Vector2(0.2f, 1f);
    public float smoothing = 12, plantSmoothing = 6, plantCutoff = 0.5f;
    public Collectable[] collectables;
    public MapManager mapManager;
    List<Point> generatedChunks = new List<Point>();
    int lastGenX = -1000, lastGenY = -1000;

    void Start() {
        tilemap = GetComponent<Tilemap>();
        InitialGenerate(Mathf.FloorToInt(player.transform.position.x/32), Mathf.FloorToInt(player.transform.position.y/32));
    }

    void Update() {
        GenerateMapAroundPlayer();
    }

    void InitialGenerate(int xint, int yint) {
        for (int x = -1; x <= 1; x++) {
            for (int y = -1; y <= 1; y++) {
                StartCoroutine(GenerateChunk(xint+x, yint+y));
            }
        }
        lastGenX = xint;
        lastGenY = yint;
    }

    void GenerateMapAroundPlayer() {

        Transform pos = player.transform;
        int xOffset = Mathf.FloorToInt(pos.position.x/32);
        int yOffset = Mathf.FloorToInt(pos.position.y/32);

        if (lastGenX == xOffset && lastGenY == yOffset) {
            return;
        }

        int dirX = lastGenX - xOffset;
        int dirY = lastGenY - yOffset;

        if (dirX == -1) {
            for (int y = -1; y <= 1; y++) {
                StartCoroutine(GenerateChunk(xOffset+1, lastGenY+y));
                DestroyChunk(lastGenX-1, lastGenY+y);
            }
        }
        if (dirX == 1) {
            for (int y = -1; y <= 1; y++) {
                StartCoroutine(GenerateChunk(xOffset-1, lastGenY+y));
                DestroyChunk(lastGenX+1, lastGenY+y);
            }
        }
        if (dirY == -1) {
            for (int x = -1; x <= 1; x++) {
                StartCoroutine(GenerateChunk(lastGenX+x, yOffset+1));
                DestroyChunk(lastGenX+x, lastGenY-1);
            }
        }
        if (dirY == 1) {
            for (int x = -1; x <= 1; x++) {
                StartCoroutine(GenerateChunk(lastGenX+x, yOffset-1));
                DestroyChunk(lastGenX+x, lastGenY+1);
            }
        }

        lastGenX = xOffset;
        lastGenY = yOffset;
    }

    IEnumerator GenerateChunk(int chunkX, int chunkY) {
        Point point = new Point();
        point.x = chunkX;
        point.y = chunkY;
        bool genTerrain = !generatedChunks.Contains(point);
        TileBase[] newTerrain = null, bgTerrain = null;
        if (genTerrain) {
            newTerrain = new TileBase[32*32];
            bgTerrain = new TileBase[32*32];
        }
        long seed = GameManager.INSTANCE.seed % 1654654;
        for (int x = 0; x < 32; x++) {
            List<int> plants = new List<int>();
            List<int> floors = new List<int>();
            for (int y = 0; y < 32; y++) {
                int pointX = x+(chunkX*32);
                int pointY = y+(chunkY*32);
                Vector3Int pos = new Vector3Int(pointX, pointY, 0);
                Vector3Int floor = new Vector3Int(pointX, pointY-1, 0);

                if (genTerrain) {
                    float wallChance = Mathf.PerlinNoise(((pointX / caveScale.x) / smoothing) + seed, ((pointY / caveScale.y) / smoothing) + seed);

                    float wallCutoff = (pointY > -28 ? (0.05f * pointY + 2f) : (0.0002f * pointY) + 0.6f);
                    wallCutoff = Mathf.Min(Mathf.Max(wallCutoff, 0.1f), 2f);

                    if (wallChance > wallCutoff) {
                        newTerrain[x+y*32] = wallTile;
                        floors.Add(y);
                        continue;
                    }

                    float plantChance = Mathf.PerlinNoise(((pointX / plantScale.x) / plantSmoothing) + seed + 1337, ((pointY / plantScale.y) / plantSmoothing) + seed + 1337);
                    if (plantChance > plantCutoff) {
                        
                        int cnt = singlePlantTiles.Length;
                        int plantChoice = (x+Mathf.Abs(chunkY))%(cnt*2+1);
                        if (plantChoice <= cnt) {
                            if ((plants.Contains(y-1) || floors.Contains(y-1))
                                && newTerrain[x+y*32] != wallTile) {
                                    
                                bgTerrain[x+y*32] = plantTile;
                                plants.Add(y);
                            }
                        } else {
                            if (floors.Contains(y-1) && newTerrain[x+y*32] != wallTile) {
                                bgTerrain[x+y*32] = singlePlantTiles[(plantChoice-cnt)-1];
                            }
                        }
                    }
                }

                float collectableCutoff = 0.001f * pointY + 0.99f; //why not this formula
                float collectableChance = Mathf.PerlinNoise(((pointX / caveScale.x) / smoothing) + seed + 654, ((pointY / caveScale.y) / smoothing) + seed + 654);
                if (collectableChance > collectableCutoff) {

                    if (genTerrain) {
                        if (!floors.Contains(y-1)) {
                            continue;
                        }
                    } else {
                        if (tilemap.GetTile(floor) != wallTile) {
                            continue;
                        }
                    }


                    //java hashes, why not???
                    int result = 1;
                    result = 4987 * result + (x+chunkX*32);
                    result = 4987 * result + (x+chunkY*32);

                    if (mapManager.collectedCollectables.Contains(result) || mapManager.spawnblockedCollectables.Contains(result)) {
                        continue;
                    }
                    
                    bool br = false;
                    foreach (Collider2D coll in Physics2D.OverlapCircleAll(pos + new Vector3(0.25f,0.25f), 20f)) {
                        if (coll.tag == "Collectable") {
                            br = true;
                            break;
                        }
                    }
                    if (br) {
                        mapManager.spawnblockedCollectables.Add(result);
                        continue;
                    }

                    Collectable collectable = ChooseCollectable(collectableChance, collectableCutoff, pointY);

                    Collectable newCollectable = GameObject.Instantiate(collectable, pos + new Vector3(0.25f,0.25f,0), Quaternion.identity);
                    
                    newCollectable.ID = result;
                    newCollectable.chunkX = chunkX;
                    newCollectable.chunkY = chunkY;

                    mapManager.collectables.Add(newCollectable);
                }
            }
            if (x%4==0) yield return null;
        }
        if (genTerrain) {
            tilemap.SetTilesBlock(new BoundsInt(chunkX*32, chunkY*32, 1, 32, 32, 1), newTerrain);
            yield return null;
            backgroundTilemap.SetTilesBlock(new BoundsInt(chunkX*32, chunkY*32, 1, 32, 32, 1), bgTerrain);
        }
    }

    Collectable ChooseCollectable(float chance, float cutoff, int y) {
        int collectiveRarities = 0;
        foreach (Collectable c in collectables) {
            if (y < c.spawnDepth) {
                collectiveRarities += c.rarity;
            }
        }
        int yes = Mathf.Abs(y)%collectiveRarities;
        foreach (Collectable c in collectables) {
            if (y > c.spawnDepth) {
                continue;
            }
            if (yes <= c.rarity) {
                return c;
            }
            yes -= c.rarity;
        }

        return collectables[0]; //pabnic
    }

    void DestroyChunk(int x, int y) {
        // tilemap.SetTilesBlock(new BoundsInt(x*32, y*32, 0, 32, 32, 1), empty);
        mapManager.RemoveCollectablesInChunk(x, y);
    }
}
