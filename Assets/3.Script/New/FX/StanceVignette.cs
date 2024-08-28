using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class StanceVignette : MonoBehaviour
{
    [SerializeField] private float _min = 0.1f;
    [SerializeField] private float _max = 0.35f;
    [SerializeField] private float _response = 10f;

    private VolumeProfile _profile;
    private Vignette _vignette;
    public void Initialize(VolumeProfile profile)
    {
        _profile = profile;
        if(!profile.TryGet(out _vignette))
            _vignette = profile.Add<Vignette>();

        _vignette.intensity.Override(_min);
    }

    public void UpdateVignette(float deltaTime, EStance stance)
    {
        var targetIntensity = stance is EStance.Stand ? _min : _max;
        _vignette.intensity.value = Mathf.Lerp
        (
            a: _vignette.intensity.value,
            b: targetIntensity,
            t: 1f - Mathf.Exp(-_response * deltaTime)
        );
    }
}
