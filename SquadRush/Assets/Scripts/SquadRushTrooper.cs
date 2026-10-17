using UnityEngine;

namespace SquadRush
{
    // Animation is driven by the simulation, so pause and deterministic checks also freeze visuals.
    public sealed class SquadRushTrooper : MonoBehaviour
    {
        public Transform Body, LeftLeg, RightLeg, Muzzle, WeaponModule;
        public void SetUpgradedWeapon(bool upgraded) { if (WeaponModule != null) WeaponModule.gameObject.SetActive(upgraded); }
        public MeshRenderer[] ColoredRenderers;
        float flashTime, hitTime;
        MaterialPropertyBlock hitProperties;
        Color uniform=new Color(1,1,1,0);
        public void SetUniform(Color color)
        {
            uniform=color;if(hitProperties==null)hitProperties=new MaterialPropertyBlock();
            hitProperties.SetColor("_UniformTint",uniform);foreach(var renderer in ColoredRenderers)renderer.SetPropertyBlock(hitProperties);
        }
        public void Fire() { flashTime = 0.075f; Muzzle.gameObject.SetActive(true); }
        public void Hit() { hitTime = 0.12f; }
        public void ResetPose()
        {
            if (hitProperties == null) hitProperties = new MaterialPropertyBlock();
            flashTime = hitTime = 0; Muzzle.gameObject.SetActive(false);
            Body.localPosition = Vector3.zero; Body.localRotation = Quaternion.identity;
            LeftLeg.localRotation = RightLeg.localRotation = Quaternion.identity;
            hitProperties.SetFloat("_Flash", 0);
            hitProperties.SetColor("_UniformTint",uniform);
            foreach (var renderer in ColoredRenderers) renderer.SetPropertyBlock(hitProperties);
        }
        public void Animate(float time, float dt, float phase, float lean = 0)
        {
            float step = time * 11 + phase;
            LeftLeg.localRotation = Quaternion.Euler(Mathf.Sin(step) * 20, 0, Mathf.Sin(step) * 2);
            RightLeg.localRotation = Quaternion.Euler(-Mathf.Sin(step) * 20, 0, -Mathf.Sin(step) * 2);
            Body.localPosition = new Vector3(0, Mathf.Abs(Mathf.Sin(step)) * 0.018f, 0);
            Body.localRotation = Quaternion.Euler(flashTime > 0 ? -3 : 0, 0, lean);
            if (flashTime > 0) { flashTime -= dt; if (flashTime <= 0) Muzzle.gameObject.SetActive(false); }
            if (hitTime > 0)
            {
                hitTime = Mathf.Max(0, hitTime - dt); hitProperties.SetFloat("_Flash", hitTime / 0.12f);
                foreach (var renderer in ColoredRenderers) renderer.SetPropertyBlock(hitProperties);
            }
        }
    }
}
