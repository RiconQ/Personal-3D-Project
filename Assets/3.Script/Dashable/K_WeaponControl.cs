using System.Collections.Generic;
using UnityEngine;

public class K_WeaponControl : MonoBehaviour
{
    public static K_WeaponControl instance;

    private RaycastHit _hit;
    public List<Transform> allWeapon = new List<Transform>();

    private float _dist;
    private float _maxDist;
    private float _closestAngle;
    private float _currentAngle;
    public int index = -1;
    private bool _targetUnreachable;

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        if (!Player.instance)
        { return; }
    
        index = -1;
        _maxDist = 11f;
        _closestAngle = 35f;
        _currentAngle = 0f;
        if (true) //if(canDash)
        {
            for (int i = 0; i < allWeapon.Count; i++)
            {
                if (!allWeapon[i].gameObject.activeInHierarchy)
                {
                    continue;
                }
                _dist = Vector3.Distance(Player.instance.PlayerCamera.transform.position, allWeapon[i].position);
                if (_dist > 11f) //Too far
                {
                    continue;
                }
                Physics.Raycast(
                    Player.instance.PlayerCamera.transform.position,
                    Player.instance.PlayerCamera.transform.position.DirTo(allWeapon[i].position),
                    out _hit, 11f);
                if((_hit.distance != 0f && _hit.collider.gameObject.layer != 11) || !(_dist < _maxDist))
                {
                    continue; //Hit is not weapon or hit is too far
                }
                Vector3 to = Player.instance.PlayerCamera.transform.position.DirTo(allWeapon[i].position);
                _currentAngle = Vector3.Angle(Player.instance.PlayerCamera.transform.forward, to);
                if(_currentAngle < _closestAngle)
                {
                    _currentAngle = _closestAngle;
                    if (index != i)
                    {
                        index = i;
                    }
                    _targetUnreachable = (Player.instance.PlayerCamera.transform.position - allWeapon[i].position).y.Abs() > 4f;
                }
            }
        }
        if(index > -1)
        {
            //VFX
        }
    }

    public Transform GetClosestTarget()
    {
        if (index == -1)
            return null;
        if (Player.instance.PlayerCharacter.Motor.GroundingStatus.IsStableOnGround && _targetUnreachable)
            return null;
        return allWeapon[index];
    }
}
