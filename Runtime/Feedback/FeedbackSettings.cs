using System;

namespace Pancake.Feedback
{
    [DeclareBoxGroup("box")]
    public class FeedbackSettings : ScriptableSettings<FeedbackSettings>
    {
        public string token;
        public bool includeScreenshot = true;
        [Group("box"), InlineProperty, HideLabel] public FeedbackBoard board;
    }

    [Serializable]
    public class FeedbackBoard
    {
        public string id;

        public string[] listNames;
        public string[] listIds;

        public string[] categoryNames = new string[] {"Feedback", "Bug"};
        public string[] categoryIds = new string[] {null, null};

        public Label[] labels = new Label[] {new Label("1", null, "Low Priority"), new Label("2", null, "Medium Priority"), new Label("3", null, "High Priority")};
    }
}