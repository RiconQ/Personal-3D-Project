using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    [field: SerializeField] public Portal otherPortal { get; private set; }
    [SerializeField] private Renderer _outlineRenderer;

    [field: SerializeField] public Color portalColor { get; private set; }
    [SerializeField] private LayerMask _placementMask;

    [SerializeField] private Transform _testTransform;

    private List<PortalableObject> _portalObjects = new List<PortalableObject>();

    public bool isPlaced { get; private set; } = true;
    private Collider _wallCollider;

    //Component
    public Renderer Renderer { get; private set; }
    private new BoxCollider _collider;

    [SerializeField]private bool _isPlayerPortal = false;

    private PortalablePlayer _portalablePortal;

    private void Awake()
    {
        _collider = GetComponent<BoxCollider>();
        Renderer = GetComponent<Renderer>();
    }

    private void Start()
    {
        _outlineRenderer.material.SetColor("_OutlineColor", portalColor);

        _portalablePortal = FindObjectOfType<PortalablePlayer>();

        gameObject.SetActive(false);
    }

    private void Update()
    {
        Renderer.enabled = otherPortal.isPlaced;
        for (int i = 0; i < _portalObjects.Count; ++i)
        {
            Vector3 objPos = transform.InverseTransformPoint(_portalObjects[i].transform.position);

            if (objPos.z > 0.0f)
            {
                _portalObjects[i].Warp();
            }
        }
        if(_isPlayerPortal)
        {
            Vector3 objPos = transform.InverseTransformPoint(_portalablePortal.transform.position);
            //Debug.Log(objPos);
            if (objPos.z > 0.0f)
            {
                _portalablePortal.StartWarp();
            }
        }

    }   

    private void OnTriggerEnter(Collider other)
    {
        var obj = other.GetComponent<PortalableObject>();
        if (obj != null && !obj.IsPlayer)
        //if (obj != null)
        {
            _portalObjects.Add(obj);
            obj.SetIsInPortal(this, otherPortal, _wallCollider);
        }
        else if(obj != null && obj.IsPlayer)
        {
            _isPlayerPortal = true;
            var player = other.GetComponentInChildren<PortalablePlayer>();
            player.SetIsInPortal(this, otherPortal, _wallCollider);
            player.StartPortal();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("Trigger Exit");
        var obj = other.GetComponentInChildren<PortalableObject>();
        
        if (_portalObjects.Contains(obj))
        {
            _portalObjects.Remove(obj);
            obj.ExitPortal(_wallCollider);
        }
        if(obj.IsPlayer)
        {
            _isPlayerPortal = false;
            var player = other.GetComponentInChildren<PortalablePlayer>();
            player.ExitPortal(_wallCollider);
            player.StopPortal();
        }
    }



    public bool PlacePortal(Collider wallCollider, Vector3 pos, Quaternion rot)
    {
        transform.position = pos;
        transform.rotation = rot;
        transform.position -= transform.forward * 0.001f;

        FixOverhangs();
        FixIntersects();

        if (CheckOverlap())
        {
            this._wallCollider = wallCollider;
            transform.position = _testTransform.position;
            transform.rotation = _testTransform.rotation;

            gameObject.SetActive(true);
            isPlaced = true;
            Debug.Log("Place Portal True");
            return true;
        }

        Debug.Log("Place Portal False");

        return false;
    }

    //Ensure the portal cannot extend past the edge of a surface
    private void FixOverhangs()
    {
        var testPoints = new List<Vector3>
        {
            new Vector3(-1.1f,  0.0f, 0.1f),
            new Vector3( 1.1f,  0.0f, 0.1f),
            new Vector3( 0.0f, -2.1f, 0.1f),
            new Vector3( 0.0f,  2.1f, 0.1f)
        };

        var testDirs = new List<Vector3>
        {
             Vector3.right,
            -Vector3.right,
             Vector3.up,
            -Vector3.up
        };

        for (int i = 0; i < 4; ++i)
        {
            RaycastHit hit;
            Vector3 raycastPos = _testTransform.TransformPoint(testPoints[i]);
            Vector3 raycastDir = _testTransform.TransformDirection(testDirs[i]);

            if (Physics.CheckSphere(raycastPos, 0.05f, _placementMask))
            {
                break;
            }
            else if (Physics.Raycast(raycastPos, raycastDir, out hit, 2.1f, _placementMask))
            {
                var offset = hit.point - raycastPos;
                _testTransform.Translate(offset, Space.World);
            }
        }
    }

    //Ensure the portal cannot intersect a section of wall
    private void FixIntersects()
    {
        var testDirs = new List<Vector3>
        {
            Vector3.right,
            -Vector3.right,
            Vector3.up,
            -Vector3.up
        };

        var testDists = new List<float> { 1.1f, 1.1f, 2.1f, 2.1f };

        for (int i = 0; i < 4; ++i)
        {
            RaycastHit hit;
            Vector3 raycastPos = _testTransform.TransformPoint(0.0f, 0.0f, -0.1f);
            Vector3 raycastDir = _testTransform.TransformDirection(testDirs[i]);

            if (Physics.Raycast(raycastPos, raycastDir, out hit, testDists[i], _placementMask))
            {
                var offset = (hit.point - raycastPos);
                var newOffset = -raycastDir * (testDists[i] - offset.magnitude);
                _testTransform.Translate(newOffset, Space.World);
            }
        }
    }

    //Once positioning has taken place, ensure the portal isnt intersecting anything
    private bool CheckOverlap()
    {
        var checkExtents = new Vector3(0.9f, 1.9f, 0.05f);

        var checkPositions = new Vector3[]
        {
            _testTransform.position + _testTransform.TransformVector(new Vector3( 0.0f,  0.0f, -0.1f)),

            _testTransform.position + _testTransform.TransformVector(new Vector3(-1.0f, -2.0f, -0.1f)),
            _testTransform.position + _testTransform.TransformVector(new Vector3(-1.0f,  2.0f, -0.1f)),
            _testTransform.position + _testTransform.TransformVector(new Vector3( 1.0f, -2.0f, -0.1f)),
            _testTransform.position + _testTransform.TransformVector(new Vector3( 1.0f,  2.0f, -0.1f)),

            _testTransform.TransformVector(new Vector3(0.0f, 0.0f, 0.2f))
        };

        //ensure the portal does not intersect walls
        var intersections = Physics.OverlapBox(checkPositions[0], checkExtents, _testTransform.rotation, _placementMask);

        if (intersections.Length > 1)
        {
            Debug.Log("Return intersect>1 False");
            return false;
        }
        else if (intersections.Length == 1)
        {
            // allow to intersect the old portal position
            if (intersections[0] != _collider)
            {
                Debug.Log("Return intersect==1 False");
                return false;
            }
        }

        //Ensure the portal corners overlay a surface
        bool isOverlapping = true;

        for (int i = 1; i < checkPositions.Length - 1; ++i)
        {
            isOverlapping &= Physics.Linecast(checkPositions[i],
                checkPositions[i] + checkPositions[checkPositions.Length - 1], _placementMask);
        }
        return isOverlapping;
    }

    public void RemovePortal()
    {
        gameObject.SetActive(false);
        isPlaced = false;
    }
}
