using Pancake.Feedback;
using UnityEditor;
using UnityEngine;

namespace Pancake.Editor
{
    internal class FeedbackNewBoardWindow : EditorWindow
    {
        private const string WINDOW_TITLE = "New Feedback Board";
        private const int WIDTH = 312;
        private const int HEIGHT = 46;


        private Trello _trello;
        private string _boardName = "Name New Board";

        public static FeedbackNewBoardWindow GetWindow()
        {
            FeedbackNewBoardWindow window = GetWindow<FeedbackNewBoardWindow>(true, WINDOW_TITLE);

            // set window size
            window.minSize = new Vector2(WIDTH, HEIGHT);
            window.maxSize = window.minSize;

            return window;
        }

        private void OnEnable()
        {
            if (_trello == null) _trello = new Trello(FeedbackSettings.Instance.token);
        }

        private void OnGUI()
        {
            if (_trello == null) return;

            _boardName = EditorGUILayout.TextField("Board Name", _boardName);

            if (GUILayout.Button("Create Board"))
            {
                // add board
                SetupBoard(_boardName);

                if (EditorUtility.DisplayDialog("Board created!", "The board " + _boardName + " has been successfully created!", "Ok"))
                {
                    // refresh boards in configuration
                    FeedbackSettingProvider.ScheduleRefresh();

                    // close self
                    Close();
                }
            }
        }

        /// <summary>
        /// Clones the default feedback board
        /// </summary>
        /// <param name="boardName"></param>
        /// <returns></returns>
        public void SetupBoard(string boardName)
        {
            _trello.AddBoard(boardName,
                true,
                true,
                null,
                null,
                Trello.TEMPLATE_BOARD_ID);
        }
    }
}