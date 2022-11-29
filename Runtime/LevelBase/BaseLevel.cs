namespace Pancake.LevelBase
{
    public abstract class BaseLevel : BaseBehaviour, ILevel
    {
#if UNITY_EDITOR

        [Button(ButtonSize.Large, Name = "PLAY"), ShowIf(nameof(CountSelectionObject))]
        private void PlayImpl()
        {
            LevelDebug.isTest = true;
            LevelDebug.levelTest = UnityEditor.Selection.activeGameObject.GetComponent<BaseLevel>();
            if (UnityEditor.SceneManagement.EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                // ReSharper disable once AccessToStaticMemberViaDerivedType
                //var scene = UnityEditor.SceneManagement.EditorSceneManager.GetSceneAt(0);
                //UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scene.path, UnityEditor.SceneManagement.OpenSceneMode.Single);
                //UnityEditor.EditorApplication.isPlaying = true;
            }
        }

        private bool CountSelectionObject => UnityEditor.Selection.count == 1 && UnityEditor.EditorUtility.IsPersistent(UnityEditor.Selection.activeGameObject);
#endif


        public abstract void Active();

        public abstract void Deactive();
    }
}