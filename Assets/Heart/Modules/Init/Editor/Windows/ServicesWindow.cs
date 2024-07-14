using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sisus.Init.Internal;
using UnityEditor;
using UnityEngine;
using static Sisus.NullExtensions;
using Object = UnityEngine.Object;

namespace Sisus.Init.EditorOnly.Internal
{
	[InitializeOnLoad]
    internal sealed class ServicesWindow : EditorWindow
    {
		public const string MenuItemName = "Window/Analysis/Service Debugger";

		private static readonly List<ServiceInfo> services = new(64);
		private static readonly ServiceInfoOrderer serviceInfoOrderer = new();
		private static ServicesWindow instance;

		private Vector2 scrollPosition;
		private string filter = "";
		private readonly HashSet<object> shown = new HashSet<object>();
		private const double repaintInterval = 1d;
		private double lastRepaintTime;

		static ServicesWindow()
		{
			Service.ChangedEditorOnly -= OnServiceChanged;
			Service.ChangedEditorOnly += OnServiceChanged;
		}

		[MenuItem(MenuItemName, priority = 10000)]
        public static void Open()
        {
            var window = GetWindow<ServicesWindow>();
            window.titleContent = new GUIContent("Service Debugger");
        }

		private void OnEnable()
		{
			instance = this;
			titleContent = new GUIContent("Service Debugger");
		}

		private void Update()
		{
			if(EditorApplication.timeSinceStartup > lastRepaintTime + repaintInterval)
			{
				lastRepaintTime = EditorApplication.timeSinceStartup;
				Repaint();
			}
		}

		private void OnGUI()
		{
			filter = EditorGUILayout.TextField(filter, EditorStyles.toolbarSearchField);

			scrollPosition = GUILayout.BeginScrollView(scrollPosition);

			var iconSizeWas = EditorGUIUtility.GetIconSize();
			EditorGUIUtility.SetIconSize(new Vector2(16f, 16f));
			Texture scriptIcon = EditorGUIUtility.ObjectContent(null, typeof(MonoScript)).image;

			shown.Clear();

			GUIContent label = new();
			GUILayoutOption[] labelLayoutOptions = new[] { GUILayout.Width(Screen.width * 0.5f) };
            foreach(var serviceInfo in services)
			{
				object service = serviceInfo.Service;

				if(service == Null || !shown.Add(service))
				{
					continue;
				}

				if(filter.Length > 0 &&(serviceInfo.TypeName.IndexOf(filter, StringComparison.OrdinalIgnoreCase) == -1 && serviceInfo.ClientsText.IndexOf(filter, StringComparison.OrdinalIgnoreCase) == -1))
				{
					continue;
				}

				label.text = serviceInfo.TypeName;
				label.tooltip = "Click to find service";

				GUILayout.BeginHorizontal();

				if(TryGetUnityObject(service, false, out Object unityObject))
				{
					label.image = EditorGUIUtility.ObjectContent(unityObject, serviceInfo.ConcreteType).image;
					GUI.enabled = unityObject != null && (unityObject is not Behaviour behaviour || behaviour.isActiveAndEnabled);
				}
				else
				{
					label.image = scriptIcon;
				}

				if(GUILayout.Button(label, EditorStyles.label, labelLayoutOptions))
				{
					switch(Event.current.button)
					{
						case 0:
							PingService(service);
							SelectService(service);
							break;
						case 1:
							var menu = new GenericMenu();
							menu.AddItem(new GUIContent("Ping"), false, ()=>PingService(service));
							menu.AddItem(new GUIContent("Select"), false, ()=>SelectService(service));
							menu.ShowAsContext();
							break;
					}
				}

				GUI.enabled = true;

				GUILayout.FlexibleSpace();

				label.text = serviceInfo.ClientsText;
				label.tooltip = "Click to find all clients";

				if(GUILayout.Button(serviceInfo.ClientsText, EditorStyles.miniButton))
				{
					switch(Event.current.button)
					{
						case 0:
							ServiceTagEditorUtility.SelectAllReferencesInScene(service);
							break;
						case 1:
							var menu = new GenericMenu();
							menu.AddItem(new GUIContent("Find Clients In Scenes"), false, () => ServiceTagEditorUtility.SelectAllReferencesInScene(service));
							menu.ShowAsContext();
							break;
					}
				}

				GUILayout.EndHorizontal();
			}



			EditorGUIUtility.SetIconSize(iconSizeWas);

			GUILayout.EndScrollView();
        }

		private static void SelectService(object service)
		{
			if(TryGetUnityObject(service, true, out Object unityObject))
			{
				if(unityObject is Component component)
				{
					Selection.activeObject = component.gameObject;
				}
				else
				{
					Selection.activeObject = unityObject;
				}
			}
		}

		private static void PingService(object service)
		{
			if(TryGetUnityObject(service, true, out Object unityObject))
			{
				EditorGUIUtility.PingObject(unityObject);
			}
		}

		private static bool TryGetUnityObject(object obj, bool includingMonoScript, out Object unityObject)
		{ 
			unityObject = obj as Object;
			if(unityObject != null)
			{
				return true;
			}
			
			if(Find.WrapperOf(obj, out var wrapper))
			{
				unityObject = wrapper as Object;
				return unityObject != null;
			}

			if(includingMonoScript && Find.Script(obj.GetType(), out var script))
			{
				unityObject = script;
				return true;
			}

			return false;
		}

		private static void OnServiceChanged(Type definingType, Clients clients, object oldInstance, object newInstance)
		{
			if(oldInstance is not null)
			{
				services.Remove(new(definingType, clients, oldInstance));
			}

			if(newInstance is not null)
			{
				var serviceInfo = new ServiceInfo(definingType, clients, newInstance);
				int index = services.BinarySearch(serviceInfo, serviceInfoOrderer);
				if(index < 0) index = ~index;
				services.Insert(index, serviceInfo);
			}

			if(instance != null)
			{
				instance.lastRepaintTime = -1f;
			}
		}

		private sealed class ServiceInfoOrderer : IComparer<ServiceInfo>
		{
			public int Compare(ServiceInfo x, ServiceInfo y) => x.TypeName.CompareTo(y.TypeName);
		}

		private readonly struct ServiceInfo : IEquatable<ServiceInfo>
		{
			public readonly Type ConcreteType;
			private readonly Type DefiningType;
			private readonly Clients ToClients;

			public readonly object Service;
			public readonly string TypeName;
			public readonly string ClientsText;

			public ServiceInfo(Type definingType, Clients toClients, object service)
			{
				DefiningType = definingType;
				ToClients = toClients;
				Service = service;

				ConcreteType = service.GetType();
				if(ConcreteType.IsGenericType)
				{
					var typeDefinition = ConcreteType.GetGenericTypeDefinition();
					if(typeDefinition == typeof(ValueTask<>) || typeDefinition == typeof(Task<>) || typeDefinition == typeof(Lazy<>))
					{
						ConcreteType = ConcreteType.GetGenericArguments()[0];
					}
				}

				if(ConcreteType == definingType || ConcreteType == typeof(object))
				{
					TypeName = TypeUtility.ToString(definingType);
				}
				else
				{
					TypeName = TypeUtility.ToString(ConcreteType) + " (" + TypeUtility.ToString(definingType) + ")";
				}

				ClientsText = ToClients.ToString();
			}

			public bool Equals(ServiceInfo other) => DefiningType == other.DefiningType && ReferenceEquals(Service, other.Service);
			public override bool Equals(object obj) => obj is ServiceInfo serviceInfo && Equals(serviceInfo);
			public override int GetHashCode() => HashCode.Combine(DefiningType, Service);
		}
	}
}