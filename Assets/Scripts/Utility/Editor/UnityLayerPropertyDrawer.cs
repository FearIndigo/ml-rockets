using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace FearIndigo.Utility.Editor
{
    [CustomPropertyDrawer(typeof(UnityLayer))]
    public class UnityLayerPropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var container = UIHelper.Create();

            var layerIndexProperty = property.FindPropertyRelative(nameof(UnityLayer.layerIndex));
            
            var layerField = UIHelper.Create<LayerField>();
            layerField.label = property.displayName;
            layerField.BindProperty(layerIndexProperty);
            container.Add(layerField);
            
            return container;
        }
    }
}