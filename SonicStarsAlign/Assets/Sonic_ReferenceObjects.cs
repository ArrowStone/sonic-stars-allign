using Unity.Collections;
using UnityEditor;
using UnityEngine;

public class Sonic_ReferenceObjects : MonoBehaviour
{
        [TextSpace("This component is just a central space for referencing player components, rather than having a bunch of references scattered across components.\n" +
                "External scripts like triggers can look for this singular component, for access to any other components they might need. ")]
        public bool DummyText;

        [AsButton("Find All References", "FindAllReferences", null)]
        public bool DummyButton;

        public CamBrain CameraBrain;

        public void FindAllReferences () {
#if UNITY_EDITOR

                CameraBrain = Object.FindFirstObjectByType<CamBrain>();

                EditorUtility.SetDirty(this);
#endif
        }

}
