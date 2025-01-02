using UnityEngine;

public class Overlap_Sphere
{
    public float DetectionDistance;
    public float DetectionRadius;
    
    public LayerMask blockageMask;
    public bool blockageCheck;

    public GameObject TargetOutput;
    public Collider[] TargetColliders;

    public bool TargetDetected;    
    
    private int _count;
    private readonly int _maxCount;
    private readonly LayerMask _mask;

    private readonly GameObject _source;
    private readonly DetectionBias _bias;

    private float _bestReading;
    private float _currReading;

    public Overlap_Sphere(GameObject _source, int _maxTargetCount, LayerMask _layerMask, float _distance, float _radius, LayerMask _blockMask = new(), DetectionBias _detectionBias = DetectionBias.Proximity)
    {   
        DetectionDistance = _distance;
        DetectionRadius = _radius;
        _maxCount = _maxTargetCount;
        _mask = _layerMask;
        this._source = _source;
        _bias = _detectionBias;
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
        TargetColliders = new Collider[_maxCount];
        _count = Physics.OverlapSphereNonAlloc(_position + (_direction * DetectionDistance), DetectionRadius, TargetColliders, _mask);
        TargetDetected = _count > 0;

        if (TargetDetected)
        {
            switch (_bias)
            {
                case DetectionBias.Proximity:
                    {
                        _bestReading = Mathf.Infinity;
                        for (int i = 0; i < _count; i++)
                        {
                            if (blockageCheck && BlockCheck(TargetColliders[i].transform.position))
                            {
                                if (TargetOutput == null)
                                {
                                    TargetDetected = false;
                                }
                                break;
                            }

                            _currReading = Vector3.Distance(_position, TargetColliders[i].transform.position);
                            if (_currReading < _bestReading)
                            {
                                _bestReading = _currReading;
                                TargetOutput = TargetColliders[i].gameObject;
                            }
                        }
                        break;
                    }
                case DetectionBias.Direction:
                    {
                        _bestReading = -1;
                        for (int i = 0; i < _count; i++)
                        {
                            if (BlockCheck(TargetColliders[i].transform.position))
                            {
                                if (TargetOutput == null)
                                {
                                    TargetDetected = false;
                                }
                                break;
                            }

                            _currReading = Vector3.Dot((TargetColliders[i].transform.position - _source.transform.position).normalized, _direction);
                            if (_currReading > _bestReading)
                            {
                                _bestReading = _currReading;
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
        return Physics.Linecast(_source.transform.position, _tgtPos, blockageMask);
    }
}

public enum DetectionBias
{
    Proximity,
    Direction,
}