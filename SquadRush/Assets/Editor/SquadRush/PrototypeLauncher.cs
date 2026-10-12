using UnityEditor;
using UnityEditor.SceneManagement;

namespace SquadRush.Editor
{
    public static class PrototypeLauncher
    {
        [MenuItem("Squad Rush/Play Prototype", false, 0)]
        public static void PlayPrototype()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EditorSceneManager.OpenScene("Assets/Scenes/SquadRush.unity");
            EditorApplication.isPlaying = true;
        }
    }
}
