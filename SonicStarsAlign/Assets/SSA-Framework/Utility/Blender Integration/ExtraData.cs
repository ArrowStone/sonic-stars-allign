// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using UnityEngine;

class ExtraData : MonoBehaviour
{
    public GLTFast.Newtonsoft.Schema.UnclassifiedData extras;

void Awake()
{
    MeshFilter meshFilter;
    if(gameObject.TryGetComponent(out meshFilter))
    {
        if(extras.TryGetValue("Collidable", out bool collidable) && !collidable) return;

        gameObject.AddComponent<MeshCollider>().sharedMesh = meshFilter.sharedMesh;
    }
}
}