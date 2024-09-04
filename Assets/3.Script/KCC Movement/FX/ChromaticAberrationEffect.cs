using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ChromaticAberrationEffect : MonoBehaviour
{
    [SerializeField] private float _intensity = 1f;

    private VolumeProfile _profile;
    private ChromaticAberration _chromatic;

    public void Initialize(VolumeProfile profile)
    {
        _profile = profile;
        if(!profile.TryGet(out _chromatic))
            _chromatic = profile.Add<ChromaticAberration>();

        _chromatic.intensity.Override(_intensity);
    }

    public void SetIntensity(float intensity)
    {
        _intensity = intensity;
        _chromatic.intensity.Override(_intensity);
    }

    public void ActiveChromatic(bool active)
    {
        _chromatic.active = active;
    }
}
