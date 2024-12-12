using UnityEngine;

public class Overlap_Sphere
{
    public float DetectionDistance;
    public float DetectionRadius;
    private readonly int maxTargetCount;
    private readonly LayerMask targetMask;
    public LayerMask blockageMask;
    public bool blockageCheck;
    private readonly GameObject source;
    private readonly DetectionBias bias;

    private float bestReading;
    private float currReading;

    public GameObject TargetOutput;
    public Collider[] TargetColliders;
    private int TargetCount;
    public bool TargetDetected;

    public Overlap_Sphere(GameObject _source, int _maxTargetCount, LayerMask _layerMask, float _distance, float _radius, LayerMask _blockMask = new(), DetectionBias _detectionBias = DetectionBias.Proximity)
    {
        maxTargetCount = _maxTargetCount;
        targetMask = _layerMask;
        DetectionDistance = _distance;
        DetectionRadius = _radius;
        source = _source;
        bias = _detectionBias;
        if (_blockMask != new LayerMask())
        {
            blockageCheck = true;
            blockageMask = _blockMask;
        }
    }

    public void Execute(Vector3 _position, Vector3 _direction)
    {
        _direction.Normalize();
        TargetOutput = null;
        TargetColliders = new Collider[maxTargetCount];
        TargetCount = Physics.OverlapSphereNonAlloc(_position + (_direction * DetectionDistance), DetectionRadius, TargetColliders, targetMask);
        TargetDetected = TargetCount > 0;

        if (TargetDetected)
        {
            switch (bias)
            {
                case DetectionBias.Proximity:
                    {
                        bestReading = Mathf.Infinity;
                        for (int i = 0; i < TargetCount; i++)
                        {
                            if (blockageCheck && BlockCheck(TargetColliders[i].transform.position))
                            {
                                if (TargetOutput == null)
                                {
                                    TargetDetected = false;
                                }
                                break;
                            }

                            currReading = Vector3.Distance(_position, TargetColliders[i].transform.position);
                            if (currReading < bestReading)
                            {
                                bestReading = currReading;
                                TargetOutput = TargetColliders[i].gameObject;
                            }
                        }
                        break;
                    }
                case DetectionBias.Direction:
                    {
                        bestReading = -1;
                        for (int i = 0; i < TargetCount; i++)
                        {
                            if (BlockCheck(TargetColliders[i].transform.position))
                            {
                                if (TargetOutput == null)
                                {
                                    TargetDetected = false;
                                }
                                break;
                            }

                            currReading = Vector3.Dot((TargetColliders[i].transform.position - source.transform.position).normalized, _direction);
                            if (currReading > bestReading)
                            {
                                bestReading = currReading;
                                TargetOutput = TargetColliders[i].gameObject;
                            }
                        }
                        break;
                    }
            }
        }
    }

    private bool BlockCheck(Vector3 _tgtPos)
    {
        return Physics.Linecast(source.transform.position, _tgtPos, blockageMask);
    }
}

public enum DetectionBias
{
    Proximity,
    Direction,
}