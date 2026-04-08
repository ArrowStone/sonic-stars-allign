// Based on the example provided by Unity:
// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.IO;
using System.Threading.Tasks;
using GLTFast.Addons;
using GLTFast;
using GLTFast.Logging;
using UnityEngine;
using GltfImport = GLTFast.Newtonsoft.GltfImport;

class CustomGltfImport : MonoBehaviour
{
    // Path to the gltf asset to be imported
    public string uri;
    public int GroundLayer;
    static int s_GroundLayer;

    async void Start()
    {
        await LoadGltf();
    }

    public async Task LoadGltf()
    {
        try
        {
            s_GroundLayer = GroundLayer;
            ImportAddonRegistry.RegisterImportAddon(new ExtraDataExtractor());
            var gltfImport = new GltfImport(logger:new ConsoleLogger());
            await gltfImport.Load(ResolveLoadUri(uri));
            await gltfImport.InstantiateMainSceneAsync(transform);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    static string ResolveLoadUri(string source)
    {
        DirectoryInfo projectRoot = Directory.GetParent(Application.dataPath);
        string fullPath = Path.GetFullPath(projectRoot.FullName + "/Assets/Resources/Meshes/" + source);
        return fullPath;
    }

    class ExtraDataExtractor : ImportAddon<ExtractorInstance> { }

    class ExtractorInstance : ImportAddonInstance
    {
        GltfImport m_GltfImport;

        public override void Dispose() { }

        public override void Inject(GltfImportBase gltfImport)
        {
            var newtonsoftGltfImport = gltfImport as GltfImport;
            if (newtonsoftGltfImport == null)
                return;

            m_GltfImport = newtonsoftGltfImport;
            newtonsoftGltfImport.AddImportAddonInstance(this);
        }

        public override void Inject(IInstantiator instantiator)
        {
            var goInstantiator = instantiator as GameObjectInstantiator;
            if (goInstantiator == null)
                return;
            _ = new MyInstantiatorAddon(m_GltfImport, goInstantiator, s_GroundLayer);
        }

        public override bool SupportsGltfExtension(string extensionName)
        {
            return false;
        }
    }
}

class MyInstantiatorAddon
{
    readonly GltfImport m_GltfImport;
    readonly GameObjectInstantiator m_Instantiator;
    readonly int m_GroundLayer;

    public MyInstantiatorAddon(GltfImport gltfImport, GameObjectInstantiator instantiator, int GroundLayer)
    {
        m_GltfImport = gltfImport;
        m_Instantiator = instantiator;
        m_GroundLayer = GroundLayer;
        m_Instantiator.NodeCreated += OnNodeCreated;
        m_Instantiator.EndSceneCompleted += () =>
        {
            m_Instantiator.NodeCreated -= OnNodeCreated;
        };
    }

    void OnNodeCreated(uint nodeIndex, GameObject gameObject)
    {
        // De-serialize glTF JSON
        
        GLTFast.Newtonsoft.Schema.Root gltf = m_GltfImport.GetSourceRoot();

        GLTFast.Newtonsoft.Schema.Node node = gltf.Nodes[(int)nodeIndex] as GLTFast.Newtonsoft.Schema.Node;
        GLTFast.Newtonsoft.Schema.UnclassifiedData extras = node?.extras;

        if (extras == null)
            return;

        // Access values in the extras property
        ExtraData component = gameObject.AddComponent<ExtraData>();
        component.extras = extras;

        gameObject.layer = m_GroundLayer;
    }
}