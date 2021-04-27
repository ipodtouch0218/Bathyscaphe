using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectable : MonoBehaviour {
    public int value;
    public int rarity;
    public float spawnDepth;
    public long ID;

    public int chunkX, chunkY;
    
    void Start() {
        ParticleSystem.EmissionModule emiss = GetComponentsInChildren<ParticleSystem>()[1].emission;
        emiss.enabled = GameManager.INSTANCE.finderUpgrade;
    }
}
