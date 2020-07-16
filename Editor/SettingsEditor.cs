//#define ENABLE_LEGACY_PREFERENCE

using Hananoki.UnityReflection;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Hananoki.Extensions;
using Hananoki.SharedModule;

using E = Hananoki.SearchFilter.SettingsEditor;

namespace Hananoki.SearchFilter {
	[System.Serializable]
	public class SettingsEditor {

		[System.Serializable]
		public class Hierarchy {
			public USceneHierarchyWindow.SearchMode searchMode;
			public string filter;

			public Hierarchy( USceneHierarchyWindow.SearchMode searchMode, string filter ) {
				this.searchMode = searchMode;
				this.filter = filter;
			}
			public Hierarchy( Hierarchy self ) {
				this.searchMode = self.searchMode;
				this.filter = self.filter;
			}
		}

		[System.Serializable]
		public class Project {
			public string filter;

			public Project( string filter ) {
				this.filter = filter;
			}
			public Project( Project self ) {
				this.filter = self.filter;
			}
		}


		public List<Hierarchy> searchFilterHierarchy;

		public List<Project> searchFilterProject;

		public int toolbarSelect;

		public bool Enable = true;

		public static E i;

		public static void Load() {
			if( i != null ) return;
			i = EditorPrefJson<E>.Get( Package.editorPrefName );
		}



		public static void Save() {
			EditorPrefJson<E>.Set( Package.editorPrefName, i );
		}
	}



	public class SettingsEditorWindow : HSettingsEditorWindow {

		static string[] toolbarNames = { "Hierarchy", "Project" };

		static ReorderableList s_rl_searchFilterHierarchy;
		static ReorderableList s_rl_searchFilterProject;

		static bool s_changed;
		static Vector2 s_scorll;

		public static void Open() {
			var w = GetWindow<SettingsEditorWindow>();
			w.SetTitle( new GUIContent( Package.name, EditorIcon.settings ) );
			w.headerMame = Package.name;
			w.headerVersion = Package.version;
			w.gui = DrawGUI;
		}


		static void DrawCommandTable( ReorderableList r ) {
			EditorGUI.BeginChangeCheck();
			r.DoLayoutList();
			if( EditorGUI.EndChangeCheck() ) {
				s_changed = true;
			}
		}

		static ReorderableList MakeRLFromHierarchy() {
			var r = new ReorderableList( E.i.searchFilterHierarchy, typeof( SettingsEditor.Hierarchy ) );

			r.drawHeaderCallback = ( rect ) => {
				EditorGUI.LabelField( rect, "SceneHierarchyWindow - SearchFilter" );
			};

			r.onAddCallback = ( rect ) => {
				if( E.i.searchFilterHierarchy.Count == 0 ) {
					E.i.searchFilterHierarchy.Add( new E.Hierarchy( USceneHierarchyWindow.SearchMode.All, "" ) );
				}
				else {
					E.i.searchFilterHierarchy.Add( new E.Hierarchy( E.i.searchFilterHierarchy[ r.count - 1 ] ) );
				}
			};

			r.drawElementCallback = ( rect, index, isActive, isFocused ) => {
				EditorGUI.BeginChangeCheck();
				var p = E.i.searchFilterHierarchy[ index ];
				var w = rect.width;
				var x = rect.x;
				rect.y += 1;
				rect.height = EditorGUIUtility.singleLineHeight;
				rect.width = w * 0.20f;
				p.searchMode = (USceneHierarchyWindow.SearchMode) EditorGUI.EnumPopup( rect, p.searchMode, "MiniPopup" );

				rect.x = x + w * 0.20f;
				rect.width = w * 0.80f;
				p.filter = EditorGUI.TextField( rect, p.filter );

			};

			return r;
		}


		static ReorderableList MakeRLFromProjectBrowser() {
			var r = new ReorderableList( E.i.searchFilterProject, typeof( E.Project ) );

			r.drawHeaderCallback = ( rect ) => {
				EditorGUI.LabelField( rect, "ProjectBrowser - SearchFilter" );
			};

			r.onAddCallback = ( rect ) => {
				if( E.i.searchFilterProject.Count == 0 ) {
					E.i.searchFilterProject.Add( new E.Project( "" ) );
				}
				else {
					E.i.searchFilterProject.Add( new E.Project( E.i.searchFilterProject[ r.count - 1 ] ) );
				}
			};

			r.drawElementCallback = ( rect, index, isActive, isFocused ) => {
				EditorGUI.BeginChangeCheck();
				var p = E.i.searchFilterProject[ index ];
				rect.y += 1;
				rect.height = EditorGUIUtility.singleLineHeight;
				p.filter = EditorGUI.TextField( rect, p.filter );
			};

			return r;
		}


		public static void DrawGUI() {
			E.Load();

			if( E.i.searchFilterHierarchy == null ) {
				E.i.searchFilterHierarchy = new List<E.Hierarchy>();
			}
			if( E.i.searchFilterProject == null ) {
				E.i.searchFilterProject = new List<E.Project>();
			}

			if( s_rl_searchFilterHierarchy == null ) {
				s_rl_searchFilterHierarchy = MakeRLFromHierarchy();
			}
			if( s_rl_searchFilterProject == null ) {
				s_rl_searchFilterProject = MakeRLFromProjectBrowser();
			}



			EditorGUI.BeginChangeCheck();
			E.i.toolbarSelect = GUILayout.Toolbar( E.i.toolbarSelect, toolbarNames );
			if( EditorGUI.EndChangeCheck() ) {
				s_changed = true;
			}

			if( E.i.toolbarSelect == 0 ) {
				DrawCommandTable( s_rl_searchFilterHierarchy );
			}
			else {
				DrawCommandTable( s_rl_searchFilterProject );
			}


			if( s_changed ) {
				E.Save();
			}

			GUILayout.Space( 8f );
		}


#if !ENABLE_HANANOKI_SETTINGS
#if UNITY_2018_3_OR_NEWER && !ENABLE_LEGACY_PREFERENCE
		[SettingsProvider]
		public static SettingsProvider PreferenceView() {
			var provider = new SettingsProvider( $"Preferences/Hananoki/{Package.name}", SettingsScope.User ) {
				label = $"{Package.name}",
				guiHandler = PreferencesGUI,
				titleBarGuiHandler = () => GUILayout.Label( $"{Package.version}", EditorStyles.miniLabel ),
			};
			return provider;
		}
		public static void PreferencesGUI( string searchText ) {
#else
		[PreferenceItem( Package.name )]
		public static void PreferencesGUI() {
#endif
			using( new LayoutScope() ) DrawGUI();
		}
#endif
	}



#if ENABLE_HANANOKI_SETTINGS
	[SettingsClass]
	public class SettingsEvent {
		[SettingsMethod]
		public static SettingsItem RegisterSettings() {
			return new SettingsItem() {
				//mode = 1,
				displayName = Package.name,
				version = Package.version,
				gui = SettingsEditorWindow.DrawGUI,
			};
		}
	}
#endif
}
