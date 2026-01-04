using UnityEngine;
using Unity.Cinemachine;
using UnityEditor;

public class CinemachineShake : MonoBehaviour
{

    public static CinemachineShake Instance { get; private set; } // Singleton 



    [SerializeField] private CinemachineCamera cam;

    private CinemachineBasicMultiChannelPerlin noise;

    private float startAmplitude;
    private float timer;
    private float duration;
    private bool shaking;

    private void Awake()
    {

        Instance = this;  //setting singleton instance

        noise = cam
            .GetCinemachineComponent(CinemachineCore.Stage.Noise)
            as CinemachineBasicMultiChannelPerlin;

        if (noise == null)
            Debug.LogError("Missing BasicMultiChannelPerlin on CinemachineCamera");
    }

    private void Update()
    {
        if (!shaking) return;

        timer += Time.deltaTime;
        float t = timer / duration;

        // Lerp from full kick → zero
        noise.AmplitudeGain = Mathf.Lerp(startAmplitude, 0f, t);

        if (t >= 1f)
        {
            noise.AmplitudeGain = 0f;
            shaking = false;
        }
    }

    
    public void Shake(float intensity, float shakeDuration)
    {
        startAmplitude = intensity;
        duration = shakeDuration;
        timer = 0f;
        shaking = true;

        // instant kick
        noise.AmplitudeGain = intensity;
    }
}
