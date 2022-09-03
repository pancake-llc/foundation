#if UNITY_EDITOR
using UnityEngine;

namespace Pancake.Editor
{
    public class EditorResources : ScriptableAssetSingleton<EditorResources>
    {
        [Foldout("SAVE DATA", styled: true)] public Texture2D circleCheckmark;
        public TextAsset classTypeTemplate;
        public TextAsset valueTypeTemplate;

        [Foldout("FINDER", styled: true)] public Texture2D chevronUp;
        public Texture2D chevronDown;
        public Texture2D eraserIcon;
        public Texture2D pinIcon;
        public Texture2D extrudeIcon;
        public Texture2D prefabIcon;
        public Texture2D alignLeft;
        public Texture2D alignCenter;
        public Texture2D alignRight;
        public Texture2D alignBottom;
        public Texture2D alignMiddle;
        public Texture2D alignTop;
        public Texture2D distributeHorizontal;
        public Texture2D distributeVertical;
        public Texture2D snapAllPic;
        public Texture2D snapVerticalPic;
        public Texture2D snapHorizontalPic;
        public Texture2D freeParentModeOnPic;
        public Texture2D freeParentModeOffPic;
        public Texture2D allBorderPic;
        public Texture2D pointPic;
        public Texture2D verticalPointPic;
        public Texture2D horizontalPointPic;
        public Texture2D verticalBorderPic;
        public Texture2D horizontalBorderPic;
        public Texture2D aboutIcon;
        public Texture2D helpOutlineIcon;
        public Texture2D arrowLeftIcon;
        public Texture2D arrowRightIcon;
        public Texture2D autoFixIcon;
        public Texture2D cleanIcon;
        public Texture2D clearIcon;
        public Texture2D collapseIcon;
        public Texture2D copyIcon;
        public Texture2D deleteIcon;
        public Texture2D doubleArrowLeftIcon;
        public Texture2D doubleArrowRightIcon;
        public Texture2D expandIcon;
        public Texture2D exportIcon;
        public Texture2D findIcon;
        public Texture2D filterIcon;
        public Texture2D gearIcon;
        public Texture2D hideIcon;
        public Texture2D homeIcon;
        public Texture2D issueIcon;
        public Texture2D logIcon;
        public Texture2D minusIcon;
        public Texture2D plusIcon;
        public Texture2D moreIcon;
        public Texture2D restoreIcon;
        public Texture2D revealIcon;
        public Texture2D revealBigIcon;
        public Texture2D selectAllIcon;
        public Texture2D selectNoneIcon;
        public Texture2D showIcon;
        public Texture2D starIcon;
        public Texture2D supportIcon;
        public Texture2D repeatIcon;

        [Foldout("SOA", styled: true)] 
        public TextAsset collectionTemlate;
        public TextAsset gameEventListenerTemplate;
        public TextAsset gameEventTemplate;
        public TextAsset referenceTemplate;
        public TextAsset unityEventTemplate;
        public TextAsset variableTemplate;
        
        [Foldout("SKIN", styled: true)] public GUISkin skin;
    }
}
#endif