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
    public Renderer renderer { get; private set; }
    private new BoxCollider _collider;

    private void Awake()
    {
        _collider = GetComponent<BoxCollider>();
        renderer = GetComponent<Renderer>();
    }

    private void Start()
    {
        _outlineRenderer.material.SetColor("_OutlineColor", portalColor);

        //gameObject.SetActive(false);
    }

    private void Update()
    {
        renderer.enabled = otherPortal.isPlaced;
        //    for (int i = 0; i < _portalObjects.Count; ++i)
        //    {
        //        Vector3 objPos = transform.InverseTransformPoint(_portalObjects[i].transform.position);
        //
        //        if (objPos.z > 0.0f)
        //        {
        //            _portalObjects[i].Wrap();
        //        }
        //    }
    }



    public void PlacePortal(Collider wallCollider, Vector3 pos, Quaternion rot)
    {
        transform.position = pos;
        transform.rotation = rot;
        transform.position -= transform.forward * 0.001f;

        //FixOverHangs
        //FixIntersects
    }

    public void RemovePortal()
    {
        gameObject.SetActive(false);
        isPlaced = false;
    }
}
