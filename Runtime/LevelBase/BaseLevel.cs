namespace Pancake.LevelBase
{
    public abstract class BaseLevel : BaseMono, ILevel
    {
#if UNITY_EDITOR

        [Button(ButtonSize.Large, Name = "PLAY"), ShowIf(nameof(CountSelectionObject))]
        private void PlayImpl()
        {
            LevelDebug.IsTest = true;
            LevelDebug.PathLevelAsset = UnityEditor.AssetDatabase.GetAssetPath(UnityEditor.Selection.activeGameObject);
            if (UnityEditor.SceneManagement.EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                // ReSharper disable once AccessToStaticMemberViaDerivedType
                string path = UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(0);
                UnityEditor.SceneManagement.EditorSceneManager.OpenScene(path, UnityEditor.SceneManagement.OpenSceneMode.Single);
                UnityEditor.EditorApplication.isPlaying = true;
            }
        }

        private bool CountSelectionObject => UnityEditor.Selection.count == 1 && UnityEditor.EditorUtility.IsPersistent(UnityEditor.Selection.activeGameObject);
#endif


        public abstract void Active();

        public abstract void Deactive();
    }
}