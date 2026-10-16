using UnityEngine;

namespace SquadRush
{
    // Render the game's own soldier only when the permanent loadout changes.
    public sealed class SquadRushPortrait : MonoBehaviour
    {
        public RenderTexture Texture { get; private set; }
        GameObject root;
        Camera portraitCamera;
        SquadRushTrooper rig;
        int lastWeapon = -2;
        public void Initialize(SquadRushGame game)
        {
            const int layer = 30;
            game.View.cullingMask &= ~(1 << layer);
            root = new GameObject("Menu soldier portrait"); root.transform.position = new Vector3(1000, 0, 0);
            Transform model = game.GetComponent<SquadRushArt>().Trooper("Loadout model", root.transform, false);
            model.localRotation = Quaternion.Euler(0, 165, 0); rig = model.GetComponent<SquadRushTrooper>();
            foreach (Transform part in model.GetComponentsInChildren<Transform>(true)) part.gameObject.layer = layer;
            var cameraObject = new GameObject("Portrait camera"); cameraObject.transform.SetParent(root.transform, false);
            portraitCamera = cameraObject.AddComponent<Camera>(); portraitCamera.enabled = false;
            portraitCamera.transform.localPosition = new Vector3(0, 1.12f, -3.4f);
            portraitCamera.transform.LookAt(root.transform.position + Vector3.up * 0.66f);
            portraitCamera.fieldOfView = 27; portraitCamera.nearClipPlane = 0.1f; portraitCamera.farClipPlane = 8;
            portraitCamera.cullingMask = 1 << layer; portraitCamera.clearFlags = CameraClearFlags.SolidColor;
            portraitCamera.backgroundColor = Color.clear; portraitCamera.allowHDR = false;
            Texture = new RenderTexture(384, 512, 24) { name = "Loadout soldier portrait", antiAliasing = 2 };
            Texture.Create(); portraitCamera.targetTexture = Texture;
        }
        public void Refresh(SquadRushEquipment equipment)
        {
            if (lastWeapon == equipment.WeaponPower) return;
            lastWeapon = equipment.WeaponPower; rig.ResetPose(); rig.SetUpgradedWeapon(lastWeapon > 0);
            portraitCamera.Render();
        }
        void OnDestroy()
        {
            if (portraitCamera != null) portraitCamera.targetTexture = null;
            if (Texture != null) { Texture.Release(); Release(Texture); }
            if (root != null) Release(root);
        }
        static void Release(Object item) { if (Application.isPlaying) Destroy(item); else DestroyImmediate(item); }
    }
}
