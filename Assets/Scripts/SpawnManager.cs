using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SpawnManager : NetworkBehaviour
{
    public GameObject[] networkObjects;
    public static SpawnManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Ensure there's only one instance
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Optional: Persist through scenes if needed
    }
    public void RequestSpawn(ObjectType objectType, Vector3 position, Quaternion rotation)
    {
        ServerSpawnObjectServerRpc(objectType, position, rotation);
    }
    [ServerRpc(RequireOwnership = false)]
    public void ServerSpawnObjectServerRpc(ObjectType objectType, Vector3 position, Quaternion rotation)
    {
        int objectId = (int)objectType;
        if(IsServer)
        {
            GameObject spawnedObject = Instantiate(networkObjects[objectId], position, rotation);
            spawnedObject.GetComponent<Projectile>()?.FireProjectile(rotation);
            spawnedObject.GetComponent<NetworkObject>().Spawn();
        }
    }
    public enum ObjectType
    {
        AkBullet,
    }
}
