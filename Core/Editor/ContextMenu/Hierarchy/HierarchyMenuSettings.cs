using System;
using System.Collections.Generic;
using Pancake;
using UnityEngine;

namespace PancakeEditor.ContextMenu
{
	[EditorIcon("scriptable_editor_setting")]
	[CreateAssetMenu( fileName = "HierarchyMenuSettings", menuName = "ContextMenu/HierarchyMenuSettings", order = 9000 )]
	internal sealed class HierarchyMenuSettings : ScriptableObject
	{
		[SerializeField] private Data[] lists;

		 public IList<Data> List => lists;

		public void Reset()
		{
			lists = Array.Empty<Data>();
		}

		public void ResetToDefault()
		{
			lists = new[]
			{
				new Data( "Copy", "Edit/Copy" ),
				new Data( "Paste", "Edit/Paste" ),

				new Data(),

				new Data( "Rename", "Edit/Rename" ),
				new Data( "Duplicate", "Edit/Duplicate" ),
				new Data( "Delete", "Edit/Delete" ),

				new Data(),

				new Data( "Select Children", "Edit/Select Children" ),
				new Data( "Set Default Parent", "..." ),
				new Data( "Clear Default Parent", "..." ),
				
				new Data("Prefab/Unpack", "GameObject/Prefab/Unpack"),
				new Data("Prefab/Unpack Completely", "GameObject/Prefab/Unpack Completely"),

				new Data(),

				new Data( "Create Empty", "GameObject/Create Empty" ),

				new Data( "3D Object/Cube", "GameObject/3D Object/Cube" ),
				new Data( "3D Object/Sphere", "GameObject/3D Object/Sphere" ),
				new Data( "3D Object/Capsule", "GameObject/3D Object/Capsule" ),
				new Data( "3D Object/Cylinder", "GameObject/3D Object/Cylinder" ),
				new Data( "3D Object/Plane", "GameObject/3D Object/Plane" ),
				new Data( "3D Object/Quad", "GameObject/3D Object/Quad" ),

				new Data( "Effects/Particle System", "GameObject/Effects/Particle System" ),
				new Data( "Effects/Particle System Force Field", "GameObject/Effects/Particle System Force Field" ),
				new Data( "Effects/Trail", "GameObject/Effects/Trail" ),
				new Data( "Effects/Line", "GameObject/Effects/Line" ),

				new Data( "Light/Directional Light", "GameObject/Light/Directional Light" ),
				new Data( "Light/Point Light", "GameObject/Light/Point Light" ),
				new Data( "Light/Spotlight", "GameObject/Light/Spotlight" ),
				new Data( "Light/Area Light", "GameObject/Light/Area Light" ),
				new Data( "Light/Reflection Probe", "GameObject/Light/Reflection Probe" ),
				new Data( "Light/Light Probe Group", "GameObject/Light/Light Probe Group" ),

				new Data( "UI/Image", "GameObject/UI/Image" ),
				new Data( "UI/Raw Image", "GameObject/UI/Raw Image" ),
				new Data( "UI/Panel", "GameObject/UI/Panel" ),
				new Data( "UI/"),
				new Data( "UI/Text - TMP", "GameObject/UI/Text - TextMeshPro" ),
				new Data( "UI/Button - TMP", "GameObject/UI/Button - TextMeshPro" ),
				new Data( "UI/Dropdown - TMP", "GameObject/UI/Dropdown - TextMeshPro" ),
				new Data( "UI/Input Field - TMP", "GameObject/UI/Input Field - TextMeshPro" ),
				new Data( "UI/Toggle", "GameObject/UI/Toggle" ),
				new Data( "UI/Slider", "GameObject/UI/Slider" ),
				new Data( "UI/Scrollbar", "GameObject/UI/Scrollbar" ),
				new Data( "UI/Scroll View", "GameObject/UI/Scroll View" ),
				new Data( "UI/"),
				new Data( "UI/Canvas", "GameObject/UI/Canvas" ),
				new Data( "UI/Event System", "GameObject/UI/Event System" ),

				new Data( "Camera", "GameObject/Camera" ),
				
				new Data(),
				new Data( "Pancake/UIButton", "GameObject/Pancake/UIButton" ),
				new Data( "Pancake/UIButton - TMP", "GameObject/Pancake/UIButton - TMP" ),
				new Data( "Pancake/UIPopup", "GameObject/Pancake/UIPopup" ),
				new Data( "Pancake/UI Set Native Size", "GameObject/Pancake/UI Set Native Size + Pivot" ),
				new Data( "Pancake/UI Set Native Size", "GameObject/Pancake/UI Set Native Size + Pivot" ),
				new Data(),
				new Data( "Properties...", "GameObject/Properties..." ),
			};
		}

		
		[Serializable]
		internal sealed class Data
		{
			[SerializeField] private string name         = string.Empty;
			[SerializeField] private string menuItemPath = string.Empty;

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