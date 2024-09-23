using UnityEngine;

public class K_DaggerController : MonoBehaviour
{
    public enum EDaggerState
    {
        Idle,
        Throw,
        Return
    }

    [Header("Reference")]
    [SerializeField] private LineRenderer _lr;
    [SerializeField] private Transform _pivot;
    [SerializeField] private GameObject _chainDaggerPrefab;

    private Vector3[] _chainPosition = new Vector3[24];
    public ChainDagger dagger { get; private set; }
    public float holding { get; private set; }
    public EDaggerState daggerState { get; private set; }
    private RequestedControllerInput _requestedInput;
    private Vector3 _posA;
    private Vector3 _posB;
    private Vector3 _offset;
    private float dist;

    public void Initialize()
    {
        _lr.positionCount = _chainPosition.Length;
        _lr.enabled = false;

        dagger = Instantiate(_chainDaggerPrefab, Vector3.zero, Quaternion.identity).GetComponent<ChainDagger>();
        dagger.gameObject.SetActive(false);

        daggerState = EDaggerState.Idle;
    }
    public void ThrowDagger()
    {
        Debug.Log("ThrowDagger");
        dagger.Activate(_pivot);
        holding = 0f;
        daggerState = EDaggerState.Throw;
    }

    public void StopDagger()
    {
        _lr.enabled = false;
        daggerState = EDaggerState.Idle;
        dagger.Reset();
    }

    public void UpdateInputDagger(ControllerInput input, float deltaTime)
    {
        _requestedInput.RightMouse = input.RightMouse;
        _requestedInput.RightMouseReleased = input.RightMouseReleased;
        _requestedInput.LeftMouse = input.LeftMouse;

        switch (daggerState)
        {
            case EDaggerState.Idle:
                //Dagger Cooldown
                if (_requestedInput.RightMouse && !_requestedInput.LeftMouse)
                {
                    ThrowDagger();
                }
                break;
            case EDaggerState.Throw:
                //if(!dagger.CheckState())
                //{
                //
                //}
                if (true)
                {
                    Debug.Log("Throwing Dagger");
                    _posA = _pivot.position;
                    _posB = dagger.ChainPos.position;
                    _offset = (transform.up + transform.right).normalized;
                    for (int j = 0; j < _chainPosition.Length; j++)
                    {
                        dist = (float)j / (float)(_chainPosition.Length - 1);
                        _chainPosition[j] = Vector3.Lerp(_posA, _posB, dist);
                        _chainPosition[j] += _offset * Mathf.Sin(dist * (float)Mathf.PI * 2f) * Mathf.Sin(deltaTime * 8f);
                    }

                    _lr.widthMultiplier = 0.25f;
                    _lr.SetPositions(_chainPosition);
                    if (!_lr.enabled)
                    {
                        _lr.enabled = true;
                    }
                }
                else
                {
                    if(_lr.enabled)
                    {
                        _lr.enabled = false;
                    }
                    if(holding == 1f)
                    {
                        StopDagger();
                        break;
                    }
                    dagger.AlignChainMesh(_pivot);
                    holding = Mathf.MoveTowards(holding, 1f, deltaTime * 0.5f);
                }
                break;
            case EDaggerState.Return:
                break;
        }
    }

}
