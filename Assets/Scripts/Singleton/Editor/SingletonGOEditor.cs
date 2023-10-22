using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace FearIndigo.Singleton.Editor
{
	[CustomEditor(typeof(SingletonGO<>), true)]
	public class SingletonGOEditor : UnityEditor.Editor
	{
		private string[] _propertiesInBaseClass;
		private bool _foldout;

		private void OnEnable()
		{
			_propertiesInBaseClass = typeof(SingletonGO<>)
				.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
				.Select(f => f.Name)
				.ToArray();
		}

		public override void OnInspectorGUI()
		{
			serializedObject.UpdateIfRequiredOrScript();
			
			// Draw script name label
			using (new EditorGUI.DisabledScope(true))
				EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"), true);
			
			// Draw foldout group for base class fields
			_foldout = EditorGUILayout.BeginFoldoutHeaderGroup(_foldout, "Singleton Settings");
			if (_foldout)
			{
				foreach (var propertyName in _propertiesInBaseClass)
				{
					var serializedProperty = serializedObject.FindProperty(propertyName);
					if(serializedProperty != null)
						EditorGUILayout.PropertyField(serializedProperty, true);
				}
			}
			EditorGUILayout.EndFoldoutHeaderGroup();
			
			EditorGUILayout.Space();
			
			// Draw fields for derived classes
			SerializedProperty iterator = serializedObject.GetIterator();
			bool enterChildren = true;
			while (iterator.NextVisible(enterChildren))
			{
				enterChildren = false;
				if (!_propertiesInBaseClass.Contains(iterator.name) && iterator.propertyPath != "m_Script")
					EditorGUILayout.PropertyField(iterator, true);
			}
			
			serializedObject.ApplyModifiedProperties();
		}

		public override VisualElement CreateInspectorGUI()
		{
			// Create editor container element.
			var container = new VisualElement();
			
			// Draw script name label
			var scriptField = new PropertyField(serializedObject.FindProperty("m_Script"));
			scriptField.SetEnabled(false);
			container.Add(scriptField);
			
			// Draw foldout group for base class fields
			var settingsFoldout = new Foldout();
			settingsFoldout.text = "Singleton Settings";
			_foldout = settingsFoldout.value;
			foreach (var propertyName in _propertiesInBaseClass)
			{
				var serializedProperty = serializedObject.FindProperty(propertyName);
				if(serializedProperty != null)
					settingsFoldout.Add(new PropertyField(serializedProperty));
			}
			container.Add(settingsFoldout);

			// Draw fields for derived classes
			SerializedProperty iterator = serializedObject.GetIterator();
			bool enterChildren = true;
			while (iterator.NextVisible(enterChildren))
			{
				enterChildren = false;
				if (!_propertiesInBaseClass.Contains(iterator.name) && iterator.propertyPath != "m_Script")
					container.Add(new PropertyField(iterator));
			}
			
			// Return the finished inspector UI
			return container;
		}
	}
}