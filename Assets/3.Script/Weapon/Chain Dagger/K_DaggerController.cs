using System.Threading;
using UnityEngine;

public class K_DaggerController : MonoBehaviour, K_SoundManager
{
    public static K_DaggerController instance;
    public enum EDaggerState
    {
        Idle,
        Throw,
        Return
    }

    private AudioSource ad;
    [Header("Sound")]
    public AudioClip throwDagger;
    public AudioClip pull;
    public AudioClip click;
    
    [Header("Reference")]
    [SerializeField] private LineRenderer _lr;
    [SerializeField] private Transform _pivot;
    public Transform Pivot => _pivot;
    [SerializeField] private GameObject _chainDaggerPrefab;
    [SerializeField] private Animator _animator;

    [Header("Curve")]
    public AnimationCurve pullCurve;

    private Vector3[] _chainPosition = new Vector3[24];
    public ChainDagger dagger { get; private set; }
    public float holding { get; private set; }
    public EDaggerState daggerState { get; private set; }
    private RequestedControllerInput _requestedInput;
    private Vector3 _posA;
    private Vector3 _posB;
    private Vector3 _offset;
    private float dist;
    private float _timer;
    private float _amp;
    public bool isCooling => cooldown > 0f;
    public float cooldown { get; private set; }

    public void Initialize()
    {
        if (instance == null)
            instance = this;
        _lr.positionCount = _chainPosition.Length;
        _lr.enabled = false;

        dagger = Instantiate(_chainDaggerPrefab, Vector3.zero, Quaternion.identity).GetComponent<ChainDagger>();
        dagger.gameObject.SetActive(false);

        daggerState = EDaggerState.Idle;
        ad = GetComponent<AudioSource>();
    }
    public void ThrowDagger()
    {
        Debug.Log("ThrowDagger");
        dagger.Activate(_pivot);
        holding = 0f;
        daggerState = EDaggerState.Throw;
    }

    private void Reset()
    {
        _lr.enabled = false;
        float num = (cooldown = 0f);
        _timer = num;
        daggerState = EDaggerState.Idle;
        if (dagger && dagger.CheckState())
        {
            dagger.Reset();
        }
    }

    private void OnDisable()
    {
        cooldown = 0f;
        Reset();
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
        _requestedInput.RightMousePressing = input.RightMousePressing;
        _requestedInput.LeftMouse = input.LeftMouse;

        switch (daggerState)
        {
            case EDaggerState.Idle:
                //Dagger Cooldown
                if (_requestedInput.RightMouse && !_requestedInput.LeftMouse && !K_WeaponHolder.instance.HasWaepon())
                {
                    Play(throwDagger);
                    ThrowDagger();
                }
                break;
            case EDaggerState.Throw:
                if (!dagger.CheckState())
                {
                    //Debug.Log("Throw Catch Dagger");
                    _animator.SetTrigger("Catch");
                    Play(click);
                    StopDagger();
                    break;
                }
                if (!dagger.hoockedCol)
                {
                    _posA = _pivot.position;
                    _posB = dagger.ChainPos.position;
                    _offset = (transform.up + transform.right).normalized;

                    for (int j = 0; j < _chainPosition.Length; j++)
                    {
                        dist = (float)j / (float)(_chainPosition.Length - 1);
                        _chainPosition[j] = Vector3.Lerp(_posA, _posB, dist);
                        _chainPosition[j] += _offset * Mathf.Sin(dist * (float)Mathf.PI * 2f) * Mathf.Sin(Time.time * 8f);
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
                    if (_lr.enabled)
                    {
                        _lr.enabled = false;
                    }
                    if (holding == 1f)
                    {
                        StopDagger();
                        //Debug.Log("Holding 1 => Stop Dagger");
                        break;
                    }
                    dagger.AlignChainMesh(_pivot);
                    holding = Mathf.MoveTowards(holding, 1f, deltaTime * 0.5f);
                }
                if (!_requestedInput.RightMousePressing && dagger.hoockedCol)
                {
                    daggerState = EDaggerState.Return;
                    Play(pull);
                    dagger.Pull();
                    _timer = 0f;
                }
                break;
            case EDaggerState.Return:
                {
                    //Debug.Log($"!dagger.CheckState() : {!dagger.CheckState()}");
                    if (!dagger.CheckState())
                    {
                       // Debug.Log("Return Dagger - Catch");
                        _animator.SetTrigger("Catch");
                        Reset();
                        break;
                    }
                    _timer = Mathf.MoveTowards(_timer, 0.25f, deltaTime);
                    dagger.UpdateTargetPos();
                    _posA = _pivot.position;
                    _posB = dagger.targetPos;
                    _offset = (transform.up - transform.right).normalized;
                    _amp = _timer / 0.25f;
                    for (int i = 0; i < _chainPosition.Length; i++)
                    {
                        dist = (float)i / (float)(_chainPosition.Length - 1);
                        _chainPosition[i] = Vector3.Lerp(_posA, _posB, dist);
                        _chainPosition[i] += _offset * Mathf.LerpUnclamped(2f * Mathf.Sin(dist * (float)Mathf.PI), 0f, pullCurve.Evaluate(_amp));
                    }
                    _lr.widthMultiplier = Mathf.Sin(_timer / 0.25f * (float)Mathf.PI) * 0.35f;
                    _lr.SetPositions(_chainPosition);
                    if (!_lr.enabled)
                    {
                        _lr.enabled = true;
                    }
                    if (_timer == 0.25f)
                    {
                        _lr.enabled = false;
                        daggerState = EDaggerState.Idle;
                        dagger.Reset();
                    }
                    break;
                }
        }
    }

    public void Play(AudioClip clip)
    {
        ad.PlayOneShot(clip);
    }
}
