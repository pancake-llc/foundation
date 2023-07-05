using System;
using System.Collections.Generic;
using Pancake;
using UnityEngine;

namespace PancakeEditor.ContextMenu
{
	[EditorIcon("scriptable_editor_setting")]
	[CreateAssetMenu( fileName = "ProjectWindowMenuSettings", menuName = "ContextMenu/ProjectWindowMenuSettings", order = 9001 )]
	internal sealed class ProjectWindowMenuSettings : ScriptableObject
	{
		[SerializeField] private Data[] lists = null;

		public IList<Data> List => lists;

		public void Reset()
		{
			lists = Array.Empty<Data>();
		}

		public void ResetToDefault()
		{
			lists = new[]
			{
				new Data( "Create/Pancake/Scriptable/Variables/bool", "Assets/Create/Pancake/Scriptable/Variables/bool" ),
				new Data( "Create/Pancake/Scriptable/Variables/int", "Assets/Create/Pancake/Scriptable/Variables/int" ),
				new Data( "Create/Pancake/Scriptable/Variables/float", "Assets/Create/Pancake/Scriptable/Variables/float" ),
				new Data( "Create/Pancake/Scriptable/Variables/string", "Assets/Create/Pancake/Scriptable/Variables/string" ),
				new Data( "Create/Pancake/Scriptable/Variables/string pair", "Assets/Create/Pancake/Scriptable/Variables/string pair" ),
				new Data( "Create/Pancake/Scriptable/Variables/color", "Assets/Create/Pancake/Scriptable/Variables/color" ),
				new Data( "Create/Pancake/Scriptable/Variables/vector2", "Assets/Create/Pancake/Scriptable/Variables/vector2" ),
				new Data( "Create/Pancake/Scriptable/Variables/vector3", "Assets/Create/Pancake/Scriptable/Variables/vector3" ),
				new Data( "Create/Pancake/Scriptable/Variables/gameobject", "Assets/Create/Pancake/Scriptable/Variables/gameobject" ),
				
				new Data( "Create/Pancake/Scriptable/Constants/int", "Assets/Create/Pancake/Scriptable/Constants/int" ),
				new Data( "Create/Pancake/Scriptable/Constants/float", "Assets/Create/Pancake/Scriptable/Constants/float" ),
				new Data( "Create/Pancake/Scriptable/Constants/string", "Assets/Create/Pancake/Scriptable/Constants/string" ),
				
				new Data( "Create/Pancake/Scriptable/Lists/action", "Assets/Create/Pancake/Scriptable/Lists/action" ),
				new Data( "Create/Pancake/Scriptable/Lists/vector2", "Assets/Create/Pancake/Scriptable/Lists/vector2" ),
				new Data( "Create/Pancake/Scriptable/Lists/vector3", "Assets/Create/Pancake/Scriptable/Lists/vector3" ),
				new Data( "Create/Pancake/Scriptable/Lists/gameobject", "Assets/Create/Pancake/Scriptable/Lists/gameobject" ),
				new Data( "Create/Pancake/Scriptable/Lists/level callback", "Assets/Create/Pancake/Scriptable/Lists/level callback" ),
				
				new Data( "Create/Pancake/Scriptable/Events/no param", "Assets/Create/Pancake/Scriptable/Events/no params" ),
				new Data( "Create/Pancake/Scriptable/Events/bool", "Assets/Create/Pancake/Scriptable/Events/bool" ),
				new Data( "Create/Pancake/Scriptable/Events/int", "Assets/Create/Pancake/Scriptable/Events/int" ),
				new Data( "Create/Pancake/Scriptable/Events/float", "Assets/Create/Pancake/Scriptable/Events/float" ),
				new Data( "Create/Pancake/Scriptable/Events/string", "Assets/Create/Pancake/Scriptable/Events/string" ),
				new Data( "Create/Pancake/Scriptable/Events/color", "Assets/Create/Pancake/Scriptable/Events/color" ),
				new Data( "Create/Pancake/Scriptable/Events/vector2", "Assets/Create/Pancake/Scriptable/Events/vector2" ),
				new Data( "Create/Pancake/Scriptable/Events/vector3", "Assets/Create/Pancake/Scriptable/Events/vector3" ),
				new Data( "Create/Pancake/Scriptable/Events/gameobject", "Assets/Create/Pancake/Scriptable/Events/gameobject" ),
#if PANCAKE_IAP
				new Data( "Create/Pancake/IAP/Scriptable IAP Data", "Assets/Create/Pancake/IAP/Scriptable IAPData"),
				new Data( "Create/Pancake/IAP/Func Product Event", "Assets/Create/Pancake/IAP/Func Product Event"),
				new Data( "Create/Pancake/IAP/Product Event", "Assets/Create/Pancake/IAP/Product Event"),
				new Data( "Create/Pancake/IAP/No Parameters Event", "Assets/Create/Pancake/IAP/No Parameters Event"),
#endif
				new Data( "Create/Pancake/Sound/Audio", "Assets/Create/Pancake/Sound/Audio" ),
				new Data( "Create/Pancake/Sound/Audio Config", "Assets/Create/Pancake/Sound/Audio Config" ),
				new Data( "Create/Pancake/Sound/Audio Structure", "Assets/Create/Pancake/Sound/Audio Structure" ),
				new Data( "Create/Pancake/Sound/Audio Handle Event", "Assets/Create/Pancake/Sound/Audio Handle Event" ),
				new Data( "Create/Pancake/Sound/Audio Play Event", "Assets/Create/Pancake/Sound/Audio Play Event" ),
				new Data( "Create/Pancake/Sound/" ),
				new Data( "Create/Pancake/Sound/Emitter Factory", "Assets/Create/Pancake/Sound/Emitter Factory" ),
				new Data( "Create/Pancake/Sound/Emitter Pool", "Assets/Create/Pancake/Sound/Emitter Pool" ),
				new Data( "Create/Pancake/Tracking/Firebase No Param", "Assets/Create/Pancake/Tracking/Firebase No Param" ),
				new Data( "Create/Pancake/Tracking/Firebase One Param", "Assets/Create/Pancake/Tracking/Firebase One Param" ),
				new Data( "Create/Pancake/Tracking/Firebase Two Param", "Assets/Create/Pancake/Tracking/Firebase Two Param" ),
				new Data( "Create/Pancake/Tracking/Firebase Three Param", "Assets/Create/Pancake/Tracking/Firebase Three Param" ),
				new Data( "Create/Pancake/Tracking/Firebase Four Param", "Assets/Create/Pancake/Tracking/Firebase Four Param" ),
				new Data( "Create/Pancake/Tracking/Firebase Five Param", "Assets/Create/Pancake/Tracking/Firebase Five Param" ),
				new Data( "Create/Pancake/Tracking/Firebase Six Param", "Assets/Create/Pancake/Tracking/Firebase Six Param" ),
				new Data( "Create/Pancake/Tracking/" ),
				new Data( "Create/Pancake/Tracking/Adjust", "Assets/Create/Pancake/Tracking/Adjust" ),
				new Data( "Create/Pancake/Misc/Level System Setting", "Assets/Create/Pancake/Misc/Level System Setting" ),
				new Data( "Create/Pancake/Misc/" ),
				new Data( "Create/Pancake/Misc/Game Object Factory", "Assets/Create/Pancake/Misc/Game Object Factory" ),
				new Data( "Create/Pancake/Misc/Game Object Pool", "Assets/Create/Pancake/Misc/Game Object Pool" ),
				new Data( "Create/Pancake/Misc/" ),
				new Data( "Create/Pancake/Misc/Notification Channel", "Assets/Create/Pancake/Misc/Notification Channel" ),
				new Data( "Create/Pancake/Misc/Popup Show Event", "Assets/Create/Pancake/Misc/Popup Show Event" ),

#if PANCAKE_ADDRESSABLE
				new Data( "Create/" ),
				new Data( "Create/Addressables/Content Builders/Use Asset Database (fastest)", "Assets/Create/Addressables/Content Builders/Use Asset Database (fastest)"),
				new Data( "Create/Addressables/Content Builders/Use Existing Build (requires built groups)",  "Assets/Create/Addressables/Content Builders/Use Existing Build (requires built groups)"),
				new Data( "Create/Addressables/Content Builders/Simulate Groups (advanced)", "Assets/Create/Addressables/Content Builders/Simulate Groups (advanced)" ),
				new Data( "Create/Addressables/Content Builders/Default Build Script",  "Assets/Create/Addressables/Content Builders/Default Build Script"),
				new Data( "Create/Addressables/Initialization/Cache Initialization Settings", "Assets/Create/Addressables/Initialization/Cache Initialization Settings"),
#if PANCAKE_PLAY_ASSET_DELIVERY
				new Data( "Create/Addressables/Initialization/Play Asset Delivery Initialization Settings", "Assets/Create/Addressables/Initialization/Play Asset Delivery Initialization Settings"),
#endif
				new Data( "Create/Addressables/Group Templates/Blank Group Template", "Assets/Create/Addressables/Group Templates/Blank Group Template"),
#if PANCAKE_PLAY_ASSET_DELIVERY
				new Data( "Create/Addressables/Custom Build/Play Asset Delivery", "Assets/Create/Addressables/Custom Build/Play Asset Delivery"),
#endif
#endif
				
				new Data( "Create/" ),
				
				new Data( "Create/Folder", "Assets/Create/Folder" ),

				new Data( "Create/" ),

				new Data( "Create/C# Script", "Assets/Create/C# Script" ),
				new Data( "Create/2D/Sprite Atlas", "Assets/Create/2D/Sprite Atlas" ),
				new Data( "Create/Shader/Standard Surface Shader", "Assets/Create/Shader/Standard Surface Shader" ),
				new Data( "Create/Shader/Unlit Shader", "Assets/Create/Shader/Unlit Shader" ),
				new Data( "Create/Shader/Image Effect Shader", "Assets/Create/Shader/Image Effect Shader" ),
				new Data( "Create/Shader/Compute Shader", "Assets/Create/Shader/Compute Shader" ),
				new Data( "Create/Shader/Ray Tracing Shader", "Assets/Create/Shader/Ray Tracing Shader" ),

				new Data( "Create/Assembly Definition", "Assets/Create/Assembly Definition" ),
				new Data( "Create/Assembly Definition Reference", "Assets/Create/Assembly Definition Reference" ),

				new Data( "Create/TextMeshPro/Font Asset", "Assets/Create/TextMeshPro/Font Asset" ),
				new Data( "Create/TextMeshPro/Font Asset Variant", "Assets/Create/TextMeshPro/Font Asset Variant" ),
				new Data( "Create/TextMeshPro/Sprite Asset", "Assets/Create/TextMeshPro/Sprite Asset" ),
				new Data( "Create/TextMeshPro/Color Gradient", "Assets/Create/TextMeshPro/Color Gradient" ),
				new Data( "Create/TextMeshPro/Style Sheet", "Assets/Create/TextMeshPro/Style Sheet" ),
				
				new Data( "Create/" ),

				new Data( "Create/Audio Mixer", "Assets/Create/Audio Mixer" ),

				new Data( "Create/" ),

				
				new Data( "Create/Render Texture", "Assets/Create/Render Texture" ),
				new Data( "Create/Custom Render Texture", "Assets/Create/Custom Render Texture" ),

				new Data( "Create/" ),

				new Data( "Create/Animator Controller", "Assets/Create/Animator Controller" ),
				new Data( "Create/Animator Override Controller", "Assets/Create/Animator Override Controller" ),
				new Data( "Create/Animation", "Assets/Create/Animation" ),
				new Data( "Create/Avatar Mask", "Assets/Create/Avatar Mask" ),

				new Data( "Create/" ),
				
				new Data( "Create/Material", "Assets/Create/Material" ),
				new Data( "Create/Physic Material", "Assets/Create/Physic Material" ),
				new Data( "Create/Physics Material 2D", "Assets/Create/2D/Physics Material 2D" ),

				new Data( "Create/" ),

				new Data( "Create/GUI Skin", "Assets/Create/GUI Skin" ),
				
				new Data(),
				
				new Data( "Show in Explorer", "Assets/Show in Explorer" ),
				new Data( "Open", "Assets/Open" ),
				new Data( "Delete", "Assets/Delete" ),
				new Data( "Rename", "Assets/Rename" ),
				new Data( "Copy Path &%C", "Assets/Copy Path" ),

				new Data(),
				
				new Data( "Export Package", "Assets/Export Package..." ),
				new Data( "Find References In Scene", "Assets/Find References In Scene" ),

				new Data(),

				new Data( "Refresh %R", "Assets/Refresh" ),
				new Data( "Reimport", "Assets/Reimport" ),

				new Data(),

				new Data( "Reimport All", "Assets/Reimport All" ),

				new Data(),

				new Data( "View in Import Activity Window", "Assets/View in Import Activity Window" ),

				new Data(),

				new Data( "Properties... &P", "Assets/Properties..." ),
			};
		}

		[Serializable]
		internal sealed class Data
		{
			[SerializeField] private string name         = string.Empty;
			[SerializeField] private string menuItemPath = string.Empty;
			[SerializeField] private string shortcut = string.Empty;

			public string Name         => name;
			public string MenuItemPath => menuItemPath;
			public bool   IsSeparator  => string.IsNullOrWhiteSpace( menuItemPath );

			public Data()
			{
			}

			public Data( string name )
			{
				this.name = name;
			}

			public Data( string name, string menuItemPath )
			{
				this.name         = name;
				this.menuItemPath = menuItemPath;
				
			}
		}
	}
}