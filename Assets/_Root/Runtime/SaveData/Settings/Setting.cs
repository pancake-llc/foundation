/*
 * 	A serializable version of the data we can use as a field in the Editor, which doesn't automatically
 * 	assign defaults to itself, so we get no serialization errors.
 */


namespace Pancake.SaveData
{
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    [System.Serializable]
    public class Setting : MetaData
    {
        public Setting()
            : base(false)
        {
        }

        public Setting(bool applyDefaults)
            : base(applyDefaults)
        {
        }

        public Setting(string path)
            : base(false)
        {
            this.path = path;
        }

        public Setting(string path, ELocation location)
            : base(false)
        {
            Location = location;
        }

#if UNITY_EDITOR
        public bool showAdvancedSettings;
#endif
    }
}