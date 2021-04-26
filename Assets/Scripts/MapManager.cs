using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    public List<Collectable> collectables = new List<Collectable>();
    public List<long> collectedCollectables = new List<long>();
    public List<long> spawnblockedCollectables = new List<long>();

    public void RemoveCollectablesInChunk(int chunkX, int chunkY) {
        List<Collectable> toRemove = new List<Collectable>();
        foreach (Collectable c in collectables) {
            if (c == null) {
                toRemove.Add(c);
                continue;
            }
            if (c.chunkX == chunkX && c.chunkY == chunkY) {
                GameObject.Destroy(c.gameObject);
                toRemove.Add(c);
            }
        }
        foreach (Collectable c in toRemove) {
            collectables.Remove(c);
        }
    }
}
