using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class RedRing_Collection : CollectableBase
{
    public StageData CurStageData;
    public int Place;
    public int ScoreValue;
    public UnityEvent OnAllRedRingsCollected;
    public Material CollectedMaterial;

    void Awake()
    {
        IEnumerator WaitForDataUpdate()
        {
            yield return new WaitForFixedUpdate();
            if (CurStageData.RedRings[Place])
            {
                transform.GetChild(0).GetComponentInChildren<MeshRenderer>().material = CollectedMaterial;
            }
        }
        StartCoroutine(WaitForDataUpdate());
    }

    public override void Collection(Collider _triggerer)
    {
        if (_triggerer.transform.TryGetComponent(out Sonic_PlayerStateMachine _ctx))
        {
            CollectionEvent.Invoke();
            CurStageData.RedRings[Place] = true;
            _ctx.Chs.Score += ScoreValue;

            if (AllRedRingsCollected())
            {
                Debug.Log("All red rings collected!");
                OnAllRedRingsCollected.Invoke();
            }

            //DataSaving.RecordStageData(CurStageData); // saves to disk
        }
    }

    private bool AllRedRingsCollected()
    {
        foreach (bool ring in CurStageData.RedRings)
            if (!ring) return false;
        return true;
    }
}