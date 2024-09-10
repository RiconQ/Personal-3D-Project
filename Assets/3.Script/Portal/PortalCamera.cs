using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using RenderPipeline = UnityEngine.Rendering.RenderPipelineManager;


public class PortalCamera : MonoBehaviour
{
    [SerializeField] private Portal[] _portals = new Portal[2];
    [SerializeField] private Camera _portalCamera;
    [SerializeField] private int _iteration = 7;

    private RenderTexture _tempTexture1;
    private RenderTexture _tempTexture2;

    private Camera _mainCamera;

    private const int _maskID1 = 1;
    private const int _maskID2 = 2;

    private void Awake()
    {
        _mainCamera = GetComponent<Camera>();

        _tempTexture1 = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.ARGB32);
        _tempTexture2 = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.ARGB32);
    }

    private void Start()
    {
        _portals[0].Renderer.material.mainTexture = _tempTexture1;
        _portals[1].Renderer.material.mainTexture = _tempTexture2;
    }

    private void OnEnable()
    {
        RenderPipeline.beginCameraRendering += UpdateCamera;
    }

    private void OnDisable()
    {
        RenderPipeline.beginCameraRendering -= UpdateCamera;
    }

    private void UpdateCamera(ScriptableRenderContext SRC, Camera camera)
    {
        if (!_portals[0].isPlaced || !_portals[1].isPlaced)
        {
            return;
        }

        if (_portals[0].Renderer.isVisible)
        {
            _portalCamera.targetTexture = _tempTexture1;
            for (int i = _iteration - 1; i >= 0; --i)
            {
                RenderCamera(_portals[0], _portals[1], i, SRC);
            }
        }

        if (_portals[1].Renderer.isVisible)
        {
            _portalCamera.targetTexture = _tempTexture2;
            for (int i = _iteration - 1; i >= 0; --i)
            {
                RenderCamera(_portals[1], _portals[0], i, SRC);
            }
        }
    }
    private void RenderCamera(Portal inPortal, Portal outPortal, int iterationsID, ScriptableRenderContext SRC)
    {
        Transform inTransform = inPortal.transform;
        Transform outTransform = outPortal.transform;

        Transform cameraTransform = _portalCamera.transform;
        cameraTransform.position = transform.position;
        cameraTransform.rotation = transform.rotation;

        for (int i = 0; i <= iterationsID; ++i)
        {
            //position the camera behind the other portal
            Vector3 relativePos = inTransform.InverseTransformPoint(cameraTransform.position);
            relativePos = Quaternion.Euler(0.0f, 180.0f, 0.0f) * relativePos;
            cameraTransform.position = outTransform.TransformPoint(relativePos);

            //Rotate the camera to look through the other portal
            Quaternion relativeRot = Quaternion.Inverse(inTransform.rotation) * cameraTransform.rotation;
            relativeRot = Quaternion.Euler(0.0f, 180.0f, 0.0f) * relativeRot;
            cameraTransform.rotation = outTransform.rotation * relativeRot;
        }

        //Set the camera's Oblique view frustum
        Plane p = new Plane(-outTransform.forward, outTransform.position);
        Vector4 clipPlaneWorldSpace = new Vector4(p.normal.x, p.normal.y, p.normal.z, p.distance);
        Vector4 clipPlaneCameraSpace =
            Matrix4x4.Transpose(Matrix4x4.Inverse(_portalCamera.worldToCameraMatrix)) * clipPlaneWorldSpace;

        var newMatrix = _mainCamera.CalculateObliqueMatrix(clipPlaneCameraSpace);
        _portalCamera.projectionMatrix = newMatrix;

        //Render camera to its render target
        UniversalRenderPipeline.RenderSingleCamera(SRC, _portalCamera);
    }
}
