// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using UnityEngine;

class ExtraData : MonoBehaviour
{
    public GLTFast.Newtonsoft.Schema.UnclassifiedData extras;
    public Rigidbody Player;

    void Awake()
    {
        string objectType = "none";
        extras.TryGetValue("obj_type", out objectType);

        switch(objectType)
        {
            case "spawnpoint":
            {
            Debug.Log("Moving player!");
            Player.position = transform.position;
            Destroy(gameObject);
            return;
            }
        }

        MeshFilter meshFilter;
        if(gameObject.TryGetComponent(out meshFilter))
        {
            if(extras.TryGetValue("Collidable", out bool collidable) && !collidable) return;

            gameObject.AddComponent<MeshCollider>().sharedMesh = meshFilter.sharedMesh;
        }
    }
}