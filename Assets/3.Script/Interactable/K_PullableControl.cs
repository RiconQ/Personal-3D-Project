using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class K_PullableControl : MonoBehaviour
{
    public static K_PullableControl instance;


    [Header("LayerMask")]
    [SerializeField] private LayerMask _pullableLayer;

    [Header("Debug SerializeField")]
    [SerializeField]private List<Transform> _pullables = new List<Transform>(50);
    public List<Transform> Pullables => _pullables;

    public static Transform target;

    private RaycastHit _hit;
    private float _cameraAspect;
    private float _screenDist;
    private float _maxScreenDist = 0.25f;
    private float _maxDist = 8f;
    private float _closestAngle;
    private float _currentAngle;
    private Transform _tempTrans;
    private Vector2 _screenDir;
    private Vector2 _screenPos;
    private Vector2 _screenCenter = new Vector2(0.5f, 0.5f);

    private bool _targetUnreachable;

    private void Awake()
    {
        if (instance == null)
            instance = this;

    }

    public void Initialize()
    {
        if (Player.instance)
        {
            _cameraAspect = Player.instance.PlayerCamera.MainCamera.aspect;
        }
    }

    public static bool GetCurrent(out Transform trans)
    {
        trans = target;
        return trans;
    }

    private void FixedUpdate()
    {
        if (!Player.instance)
        {
            return;
        }
        target = null;
        _maxScreenDist = 0.25f;

        if (Player.instance.DaggerController.daggerState != K_DaggerController.EDaggerState.Idle)
        {
            return;
        }


        foreach (Transform pullable in _pullables)
        {
            var headPos = Player.instance.PlayerCamera.transform;
            if (!pullable.gameObject.activeInHierarchy ||
                pullable.position.y - headPos.position.y < 0f ||
                Vector3.Dot(headPos.forward, headPos.position.DirTo(pullable.position)) < 0f)
            {
                continue;
            }
            Physics.Raycast(headPos.position, headPos.position.DirTo(pullable.position), out _hit, 19f, _pullableLayer);

            if(_hit.distance != 0f && _hit.collider.gameObject.layer == 8) //Interactable
            {
                //Debug.Log("Interactable Detect");
                _screenPos = Player.instance.PlayerCamera.MainCamera.WorldToScreenPoint(pullable.position);
                _screenPos.x /= Screen.width;
                _screenPos.y /= Screen.height;
                _screenDir = _screenCenter - _screenPos;
                _screenDir.x *= _cameraAspect;
                _screenDist = _screenDir.magnitude;
                if(_screenDist < _maxScreenDist)
                {
                    _maxScreenDist = _screenDist;
                    target = pullable;
                }
            }
        }
    }

}
