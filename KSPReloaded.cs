using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KSPReloaded
{

	struct KSPReloadedAssembly {
		public string addonName;
		public string url;
		public string filePath;
	}

	[KSPAddon(KSPAddon.Startup.EveryScene, false)]
	public class KSPReloaded : MonoBehaviour
	{
		private string pluginDirectory = "Plugins";

		private Rect windowPosition;
		private GUIStyle rightAlignedText;
		private bool keyWasDownLastUpdate;
		private bool windowVisible = false;
		private List<KSPReloadedAssembly> loadedAssemblies  = new List<KSPReloadedAssembly>();
		private List<string> loadableFilePaths = new List<string> ();
		private bool showingLoaded = true;
		private Vector3 previousMousePosition;
		void Awake () {
			this.enabled = true;
		}

		void Start () {
			Debug.LogWarning ("KSPReloaded started up");

			generateAssemblyList ();
			generateLoadableFilesList ();

			RenderingManager.AddToPostDrawQueue (1, DrawGUI);
			windowPosition = new Rect ((Screen.width / 2) - 150, 100, 50, 200);
			rightAlignedText = new GUIStyle (HighLogic.Skin.label);
			rightAlignedText.alignment = TextAnchor.UpperLeft;
		}

		void generateLoadableFilesList() {
			foreach (string loadableFilePath in System.IO.Directory.GetFiles(KSPUtil.ApplicationRootPath + this.pluginDirectory)) {
				if (loadableFilePath.EndsWith (".dll")) {
					this.loadableFilePaths.Add (loadableFilePath);
				}
			}
		}

		void generateAssemblyList() {
			AssemblyLoader.LoadedAssembyList loadedAssemblies = AssemblyLoader.loadedAssemblies;
			foreach (AssemblyLoader.LoadedAssembly loadedAssembly in loadedAssemblies) {
				if (loadedAssembly.dllName != "KSP") {
					KSPReloadedAssembly assemblyToReload = new KSPReloadedAssembly ();
					assemblyToReload.filePath = loadedAssembly.path;
					assemblyToReload.url = loadedAssembly.url;
					assemblyToReload.addonName = loadedAssembly.assembly.GetName ().Name;
					Debug.Log (assemblyToReload.addonName);
					this.loadedAssemblies.Add (assemblyToReload);
				}
			}
		}

		void Update () {
			//Because obviously bottom to top left to right was the right choice, right guys?
			Vector3 currentMousePosition = Input.mousePosition;
			currentMousePosition.y = Screen.height - currentMousePosition.y;
			Rect dragableArea = this.windowPosition;
			dragableArea.height = 0;
			if (Input.GetMouseButton (0) && this.windowPosition.Contains (currentMousePosition)) {
				if (this.previousMousePosition.x != 0) {
					this.windowPosition.x -= this.previousMousePosition.x - currentMousePosition.x;
					this.windowPosition.y -= this.previousMousePosition.y - currentMousePosition.y;
				}
			}
			this.previousMousePosition = currentMousePosition;

			bool keyIsDown = Input.GetKey (KeyCode.F10);
			if (keyIsDown != keyWasDownLastUpdate && !keyWasDownLastUpdate) {
				windowVisible = !windowVisible;
			}
			keyWasDownLastUpdate = keyIsDown;
		}

		GUILayoutOption[] windowOptions() {
			this.windowPosition.height = this.loadedAssemblies.Count () * 40;
			this.windowPosition.width = 200;
			return new GUILayoutOption[] { GUILayout.Width (this.windowPosition.width), GUILayout.Height (this.windowPosition.height) };
		}

		void DrawGUI () {
			GUI.skin = HighLogic.Skin;
			if (windowVisible) {
				GUILayout.Window (-554645, this.windowPosition, MainWindowGUI, "KSPModManager", windowOptions ());
			}
		}

		void MainWindowGUI (int windowId) {
			GUILayout.BeginVertical ();
			GUILayout.BeginHorizontal ();
			if (GUILayout.Button ("Loaded", rightAlignedText)) {
				this.showingLoaded = true;
			} 
			if (GUILayout.Button ("Manager", rightAlignedText)) {
				this.showingLoaded = false;
			} 
			GUILayout.EndHorizontal ();
			GUILayout.EndVertical ();
			if (this.showingLoaded) {
				LoadedAddonsGUI (windowId);
			} else {
				LoadableAddonsGUI (windowId);
			}
		}

		void LoadedAddonsGUI (int windowId) {
			bool reloadAssemblies = false;
			int assemblyCount = 0;
			KSPReloadedAssembly assemblyToReload = new KSPReloadedAssembly();
			foreach (KSPReloadedAssembly loadedAssembly in loadedAssemblies) {
				assemblyCount++;
				GUILayout.BeginVertical ();
				GUILayout.BeginHorizontal ();
				if (GUILayout.Button ("Reload", rightAlignedText)) {
					reloadAssemblies = true;
					assemblyToReload = loadedAssembly;
				}
				AddLabel (loadedAssembly.addonName);
				GUILayout.EndHorizontal ();
				GUILayout.EndVertical ();
			}

			if (reloadAssemblies) {
				reloadAllPlugins (assemblyToReload);
			}
		}

		void LoadableAddonsGUI (int windowId) {
			string filepathToLoad = null;
			foreach (string loadableAddon in this.loadableFilePaths) {
				System.IO.FileInfo addonFileInfo = new System.IO.FileInfo (loadableAddon);
				string addonName = addonFileInfo.Name;
				GUILayout.BeginVertical ();
				GUILayout.BeginHorizontal ();
				if (GUILayout.Button ("Load", rightAlignedText)) {
					filepathToLoad = loadableAddon;
				}
				AddLabel (addonName);
				GUILayout.EndHorizontal ();
				GUILayout.EndVertical ();
			}
		}

		string filePathForAssembly(KSPReloadedAssembly assembly) {
			string filePath = null;
			System.IO.FileInfo fileInfo = new System.IO.FileInfo (assembly.filePath);
			string pluginPath = fileInfo.DirectoryName;
			string extension = fileInfo.Extension;
			string currentFileName = fileInfo.Name;
			string originalPath = pluginPath + "/" + assembly.addonName + extension;

			if (assembly.filePath != originalPath && System.IO.File.Exists(assembly.filePath)) {
				filePath = assembly.filePath;
			}
			if (System.IO.File.Exists (originalPath)) {
				if (filePath != null) {
					System.IO.File.Delete (filePath);
				}
				filePath = originalPath;
			}

			return filePath;
		}

		void reloadAllPlugins(KSPReloadedAssembly assemblyToBeReloaded) {
			foreach (KSPReloadedAssembly loadedAssembly in loadedAssemblies) {
				if (loadedAssembly.addonName != this.name) {
					string typeName = loadedAssembly.addonName + "." + loadedAssembly.addonName;
					try {
						UnityEngine.Object[] assemblyObjects = UnityEngine.Object.FindObjectsOfType (Type.GetType (typeName, true));
						foreach (UnityEngine.Object assemblyObject in assemblyObjects) {
							DestroyImmediate (assemblyObject);
						}
					} catch {
					}
				}
			}

			AssemblyLoader.ClearPlugins ();

			//make a copy of the loaded assemblies
			List <KSPReloadedAssembly> modifiedAssemblies = new List <KSPReloadedAssembly> ();

			for (int i = 0; i < this.loadedAssemblies.Count(); i++) {
				KSPReloadedAssembly assemblyToReload = this.loadedAssemblies [i];
				if (assemblyToReload.addonName == assemblyToBeReloaded.addonName) {
					string filePath = filePathForAssembly (assemblyToReload);
					assemblyToReload.filePath = createNewFileName(filePath);
					if (filePath != null) {
						System.IO.File.Move (filePath, assemblyToReload.filePath);
					} else {
						Debug.Log ("cannot find file " + filePath);
					}
				}
				modifiedAssemblies.Add (assemblyToReload);
			}
			//overwrite the loaded assemblies with the new list (updated filepaths)
			this.loadedAssemblies = modifiedAssemblies;

			//this will force the removal of the now missing plugin.
			AssemblyLoader.LoadAssemblies ();

			foreach (KSPReloadedAssembly loadedAssembly in loadedAssemblies) {
				//we don't want to try to reload kerbal itself, or us!
				if (loadedAssembly.addonName != "KSP") {
					try {
						AssemblyLoader.LoadPlugin (new System.IO.FileInfo (loadedAssembly.filePath), loadedAssembly.url);
					} catch {
					}
				}
			}

			//this time, it will load the new, renamed plugin.
			AssemblyLoader.LoadAssemblies ();

			//and finally, restart the plugins (needs to be finished)
			AddonLoader.Instance.StartAddons (KSPAddon.Startup.Instantly);
			DestroyImmediate (this);
		}

		string createNewFileName(string filePath) {
			System.IO.FileInfo fileInfo = new System.IO.FileInfo (filePath);
			string newHash = (filePath + DateTime.Now.ToString ()).GetHashCode ().ToString ();
			return fileInfo.DirectoryName + "/" + newHash + ".dll"; 
		}

		void AddLabel(string text) {
			GUILayout.Label(text, rightAlignedText);
		}

		public void OnDestroy ()
		{
			windowVisible = false;
		}
	}
}

