using System.Collections.Generic;
using UnityEngine;

public class SpawnPrefab : MonoBehaviour
{
        public List<GameObject> prefabToSpawn = new List<GameObject>();

        public void Spawn () {
                for (int i = 0; i < prefabToSpawn.Count; i++) {
                        Instantiate(prefabToSpawn[i], transform.position, Quaternion.identity);
                }
        }
}
