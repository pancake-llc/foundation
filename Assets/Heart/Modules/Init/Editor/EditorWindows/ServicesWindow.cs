using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Sisus.Init.Internal;
using UnityEditor;
using UnityEngine;
using static Sisus.NullExtensions;
using Object = UnityEngine.Object;

namespace Sisus.Init.EditorOnly.Internal
{
	internal sealed class ServicesWindow : EditorWindow
	{
		public const string MenuItemName = "Tools/Pancake/Init/Service Debugger";
		private const double clientsListAutoRebuildInterval = 0.5d;
		private static readonly GUIContent titleLabel = new("Service Debugger");

		private ClientInfo[] clients = Array.Empty<ClientInfo>();

		private readonly GUIContent inactiveLabel = new("Inactive", "Inactive services are listed as well.");
		private readonly GUIContent activeLabel = new("Active", "Inactive services not listed.");
		private bool showClients = false;
		private bool autoRebuildEnabled = false;
		private readonly GUIContent clientsLabel = new("Clients", "Displaying clients that can receive services.");
		private readonly GUIContent servicesLabel = new("Services", "Displaying services that can be received by clients.");
		private readonly GUILayoutOption[] expandWidth = new[] { GUILayout.ExpandWidth(true) };

		private Vector2 scrollPosition;
		private string filter = "";
		private const double repaintInterval = 1d;
		private double lastRepaintTime;
		private bool showInactive;
		private double lastClientsListRebuildTime;

		[MenuItem(MenuItemName, priority = 10000)]
		public static void Open()
		{
			var window = GetWindow<ServicesWindow>();
			window.titleContent = titleLabel;
		}

		private void OnEnable()
		{
			titleContent = titleLabel;

			Service.AnyChangedEditorOnly -= Repaint;
			Service.AnyChangedEditorOnly += Repaint;
			ObjectChangeEvents.changesPublished -= OnAnyObjectChanged;
			ObjectChangeEvents.changesPublished += OnAnyObjectChanged;
		}

		private void OnAnyObjectChanged(ref ObjectChangeEventStream stream) => Repaint();

		private void OnDisable()
		{
			Service.AnyChangedEditorOnly -= Repaint;
			ObjectChangeEvents.changesPublished -= OnAnyObjectChanged;
		}

		private void Update()
		{
			if(EditorApplication.timeSinceStartup > lastRepaintTime + repaintInterval)
			{
				lastRepaintTime = EditorApplication.timeSinceStartup;
				Repaint();
			}

			HandleUpdateClientsList();
		}

		private void OnGUI()
		{
			DrawTabBar();

			scrollPosition = GUILayout.BeginScrollView(scrollPosition);

			var iconSizeWas = EditorGUIUtility.GetIconSize();
			EditorGUIUtility.SetIconSize(new Vector2(16f, 16f));

			if(!showClients)
			{
				DrawServicesList();
			}
			else
			{
				DrawClientsList();
			}

			EditorGUIUtility.SetIconSize(iconSizeWas);

			GUILayout.EndScrollView();

			GUILayout.Space(10f);

			if(showClients)
			{
				DrawRefreshClientsControls();
			}

			GUILayout.Space(10f);
		}

		private void DrawTabBar()
		{
			GUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandHeight(true));

			if(GUILayout.Button(showClients ? clientsLabel : servicesLabel, EditorStyles.toolbarButton, GUILayout.Width(60f)))
			{
				showClients = !showClients;
				RebuildClientsList();
				Repaint();
				GUIUtility.ExitGUI();
			}

			bool setShowInactive = GUILayout.Toggle(showInactive, showInactive ? inactiveLabel : activeLabel, EditorStyles.toolbarButton, GUILayout.Width(55f));
			if(setShowInactive != showInactive)
			{
				showInactive = setShowInactive;
				if(showClients)
				{
					RebuildClientsList();
				}

				Repaint();
				GUIUtility.ExitGUI();
			}

			filter = EditorGUILayout.TextField(filter, EditorStyles.toolbarSearchField);

			GUILayout.EndHorizontal();
		}

		private void DrawServicesList() => Service.UsingActiveInstancesEditorOnly(DrawServicesList);

		private void DrawServicesList(List<Service.ServiceInfo> services)
		{
			if(services.Count == 0)
			{
				GUILayout.Label("No services to list.", EditorStyles.largeLabel);
			}

			GUIContent label = new();
			foreach(var serviceInfo in services)
			{
				object service = serviceInfo.Service;

				if(service == Null || serviceInfo.IsAsset)
				{
					continue;
				}

				if(filter.Length > 0 && serviceInfo.Label.text.IndexOf(filter, StringComparison.OrdinalIgnoreCase) == -1 && serviceInfo.ClientsText.IndexOf(filter, StringComparison.OrdinalIgnoreCase) == -1)
				{
					continue;
				}

				bool isUnityObject = TryGetUnityObject(service, false, out Object unityObject);
				bool isActive = !isUnityObject || (unityObject && (unityObject is not Behaviour behaviour || behaviour.isActiveAndEnabled));
				if(!isActive && !showInactive)
				{
					continue;
				}

				GUILayout.BeginHorizontal();

				if(!serviceInfo.IsSetupDone)
				{
					serviceInfo.Setup(EditorServiceTagUtility.GetServiceDefiningTypes(service));
				}

				label.text = serviceInfo.Label.text;
				label.tooltip = serviceInfo.Label.tooltip ?? "Click to locate service";
				if(isUnityObject)
				{
					label.image = EditorGUIUtility.ObjectContent(unityObject, serviceInfo.ConcreteType).image;
					GUI.enabled = isActive;
				}
				else
				{
					Texture scriptIcon = EditorGUIUtility.ObjectContent(null, typeof(MonoScript)).image;
					label.image = scriptIcon;
				}

				if(GUILayout.Button(label, Styles.RichTextLabel, expandWidth))
				{
					switch(Event.current.button)
					{
						case 0:
							PingObject(service);
							SelectObject(service);
							break;
						case 1:
							var menu = new GenericMenu();
							menu.AddItem(new("Ping"), false, () => PingObject(service));
							menu.AddItem(new("Select"), false, () => SelectObject(service));
							menu.ShowAsContext();
							break;
					}
				}

				GUI.enabled = true;

				GUILayout.FlexibleSpace();

				label.text = serviceInfo.ClientsText;
				label.tooltip = "Click to find all clients";
				label.image = null;

				if(GUILayout.Button(label, EditorStyles.miniButton))
				{
					switch(Event.current.button)
					{
						case 0:
							EditorServiceTagUtility.SelectAllReferencesInScene(service);
							break;
						case 1:
							var menu = new GenericMenu();
							menu.AddItem(new("Find Clients In Scenes"), false, () => EditorServiceTagUtility.SelectAllReferencesInScene(service));
							menu.ShowAsContext();
							break;
					}
				}

				GUILayout.EndHorizontal();
			}
		}

		private void DrawClientsList()
		{
			if(clients.Length == 0)
			{
				GUILayout.Label("No clients to list.", EditorStyles.largeLabel);
			}

			foreach(var clientInfo in clients)
			{
				object client = clientInfo.client;

				if(clientInfo.IsAsset)
				{
					continue;
				}

				if(filter.Length > 0 && clientInfo.Label.text.IndexOf(filter, StringComparison.OrdinalIgnoreCase) == -1 && clientInfo.InitState.IndexOf(filter, StringComparison.OrdinalIgnoreCase) == -1)
				{
					continue;
				}

				Object unityObject = clientInfo.unityObject;
				bool isUnityObject = unityObject;
				if(unityObject is not null && !isUnityObject)
				{
					continue;
				}

				bool isActive = !isUnityObject || (unityObject && (unityObject is not Behaviour behaviour || behaviour.isActiveAndEnabled));
				if(!isActive && !showInactive)
				{
					continue;
				}

				var label = new GUIContent
				(
				clientInfo.Label.text,
				clientInfo.Label.image,
				"Click to locate client"
				);

				GUILayout.BeginHorizontal();

				GUI.enabled = isActive;

				if(GUILayout.Button(label, Styles.RichTextLabel, expandWidth))
				{
					switch(Event.current.button)
					{
						case 0:
							PingObject(client);
							SelectObject(client);
							break;
						case 1:
							var menu = new GenericMenu();
							menu.AddItem(new("Ping"), false, ()=>PingObject(client));
							menu.AddItem(new("Select"), false, ()=>SelectObject(client));
							menu.ShowAsContext();
							break;
					}
				}

				GUI.enabled = true;

				GUILayout.FlexibleSpace();

				label.text = clientInfo.InitState;
				label.tooltip = "The current initialization state of the client.\n\nUninitialized: no services have been injected to the client.\n\nInitializing: client is currently in the process of being initialized, or an exception occured during initialization.\n\n.Initialized: Client has successfully received all services.";
				label.image = null;

				GUI.color = clientInfo.InitStateColor;

				if(GUILayout.Button(label, EditorStyles.miniButton, GUILayout.Width(85f)))
				{
					switch(Event.current.button)
					{
						case 0:
							EditorServiceTagUtility.SelectAllReferencesInScene(client);
							break;
						case 1:
							var menu = new GenericMenu();
							menu.AddItem(new("Find Clients In Scenes"), false, () => EditorServiceTagUtility.SelectAllReferencesInScene(client));
							menu.ShowAsContext();
							break;
					}
				}

				GUI.color = Color.white;

				GUILayout.EndHorizontal();
			}
		}

		private void DrawRefreshClientsControls()
		{
			GUILayout.BeginHorizontal(expandWidth);
			GUILayout.Space(10f);

			if(autoRebuildEnabled)
			{
				GUI.enabled = false;
			}

			if(GUILayout.Button("Refresh List", GUILayout.Height(30f)))
			{
				GUI.enabled = true;
				RebuildClientsList();
				Repaint();
				GUIUtility.ExitGUI();
			}

			GUI.enabled = true;

			GUILayout.Space(10f);
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal(expandWidth);
			GUILayout.Space(10f);
			autoRebuildEnabled = EditorGUILayout.ToggleLeft("Refresh Continuously", autoRebuildEnabled);
			GUILayout.Space(10f);
			GUILayout.EndHorizontal();
		}

		private void HandleUpdateClientsList()
		{
			if(!showClients || !autoRebuildEnabled)
			{
				return;
			}

			double timeSinceLastUpdate = EditorApplication.timeSinceStartup - lastClientsListRebuildTime;
			if(timeSinceLastUpdate < clientsListAutoRebuildInterval)
			{
				return;
			}

			RebuildClientsList();
			Repaint();
		}

		private void RebuildClientsList()
		{
			var instances = Find.All<IInitializableEditorOnly>(showInactive);
			int count = instances.Length;
			clients = new ClientInfo[count];
			for(int i = 0; i < count; i++)
			{
				clients[i] = new(instances[i]);
			}

			lastClientsListRebuildTime = EditorApplication.timeSinceStartup;
		}

		private static void SelectObject(object service)
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

		private static void PingObject(object service)
		{
			if(TryGetUnityObject(service, true, out Object unityObject))
			{
				EditorGUIUtility.PingObject(unityObject);
			}
		}

		private static bool TryGetUnityObject(object obj, bool includingMonoScript, out Object unityObject)
		{ 
			unityObject = obj as Object;
			if(unityObject is not null)
			{
				return true;
			}
			
			if(Find.WrapperOf(obj, out var wrapper))
			{
				unityObject = wrapper as Object;
				return unityObject is not null;
			}

			if(includingMonoScript && Find.Script(obj.GetType(), out var script))
			{
				unityObject = script;
				return true;
			}

			return false;
		}

		private static void OnServiceInitializationFailed(GlobalServiceInfo serviceInfo)
		{
			#if DEV_MODE
			Debug.Log($"ServicesWindow.OnServiceInitializationFailed({serviceInfo.ConcreteOrDefiningType.Name})");
			#endif
		}

		private static bool IsPrefabAsset([AllowNull] object obj) => obj != Null && Find.In(obj, out Transform transform) && transform.gameObject.IsPartOfPrefabAsset();

		private struct ClientInfo : IEquatable<ClientInfo>
		{
			private static readonly Dictionary<InitState, string> initStateNames = new(3) { { Init.Internal.InitState.Uninitialized, "Uninitialized"  }, { Init.Internal.InitState.Initializing, "Initializing"  }, { Init.Internal.InitState.Initialized, "Initialized"  } };
			private static readonly Dictionary<InitState, Color> initStateColors = new(3) { { Init.Internal.InitState.Uninitialized, Color.grey  }, { Init.Internal.InitState.Initializing, Color.grey  }, { Init.Internal.InitState.Initialized, Color.white  } };

			public readonly IInitializableEditorOnly client;
			public readonly Type[] initParameterTypes;
			public readonly bool[] initArgsAreServices;
			public readonly bool hasInitializer;
			public readonly Object unityObject;
			public readonly IInitializer initializer;
			public readonly NullArgumentGuard nullArgumentGuard;
			private bool? isAsset;
			private readonly Lazy<GUIContent> label;

			public readonly GUIContent Label => label.Value;
			public readonly string InitState => initStateNames[client.InitState];
			public readonly Color InitStateColor => initStateColors[client.InitState];
			public bool IsAsset => isAsset ??= IsPrefabAsset(client);

			public ClientInfo(IInitializableEditorOnly client) : this()
			{
				this.client = client;
				hasInitializer = client.HasInitializer;
				initializer = client.Initializer;
				TryGetUnityObject(client, false, out unityObject);
				nullArgumentGuard = InitializerUtility.requestNullArgumentGuardFlags(unityObject);
				initParameterTypes = InitializerEditorUtility.GetInitParameterTypes(unityObject);
				initArgsAreServices = new bool[initParameterTypes.Length];
				for(int i = 0; i < initParameterTypes.Length; i++)
				{
					initArgsAreServices[i] = ServiceUtility.IsServiceDefiningType(initParameterTypes[i]);
				}

				Object obj = unityObject;
				label = new(()=> new GUIContent(TypeUtility.ToString(client.GetType()), (obj ? EditorGUIUtility.ObjectContent(obj, obj.GetType()) : EditorGUIUtility.ObjectContent(null, typeof(MonoScript))).image));
			}

			public bool Equals(ClientInfo other) => ReferenceEquals(client, other.client);
			public override bool Equals(object obj) => obj is ClientInfo clientInfo && Equals(clientInfo);
			public override int GetHashCode() => client.GetHashCode();
		}
	}
}