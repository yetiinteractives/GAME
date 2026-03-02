using UnityEngine;
using System.Collections;

namespace KnowerCoder.BloodFX
{
    public class BloodFXInstance : MonoBehaviour
    {
        [SerializeField] private ParticleSystem[] particleSystems;
        [SerializeField] private Material decalMaterial;
        [SerializeField] private GameObject decalObject;
        [SerializeField] private float decalSpreadSpeed = 1.0f;
        [SerializeField] private float startSize = 4f;
        [SerializeField] private float endSize = 1.5f;
        [SerializeField] private float decalEnableDelay = 0.03f;

        private float darkMaskStartSize;
        private float lightMaskStartSize;
        private bool isPlaying;

        private void Awake()
        {
            if (decalObject != null)
            {
                var renderer = decalObject.GetComponent<Renderer>();
                if (renderer != null)
                {
                    decalMaterial = renderer.material; // runtime material instance (not shared)
                }
            }

            ResetDecalState();
        }

        private void OnEnable()
        {
            Play();
        }

        private void Update()
        {
            if (!isPlaying || decalMaterial == null) return;

            if (decalMaterial.GetFloat("_DarkMaskStartSize") > endSize)
            {
                darkMaskStartSize -= Time.deltaTime * decalSpreadSpeed;
                lightMaskStartSize -= Time.deltaTime * decalSpreadSpeed;

                decalMaterial.SetFloat("_DarkMaskStartSize", darkMaskStartSize);
                decalMaterial.SetFloat("_LightMaskStartSize", lightMaskStartSize);
            }
        }

        public void Play()
        {
            foreach (var ps in particleSystems)
            {
                if (ps != null) ps.Play();
            }

            isPlaying = true;
            StartCoroutine(EnableDecalDelayed());
        }

        public void ResetDecalState()
        {
            isPlaying = false;
            darkMaskStartSize = startSize;
            lightMaskStartSize = startSize;

            if (decalObject != null) decalObject.SetActive(false);

            if (decalMaterial != null)
            {
                decalMaterial.SetFloat("_DarkMaskStartSize", darkMaskStartSize);
                decalMaterial.SetFloat("_LightMaskStartSize", lightMaskStartSize);
            }
        }

        private IEnumerator EnableDecalDelayed()
        {
            yield return new WaitForSeconds(decalEnableDelay);
            if (decalObject != null) decalObject.SetActive(true);
        }
    }
}