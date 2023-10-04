using FearIndigo.Utility;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace FearIndigo.Managers.Folder
{
    [CustomEditor(typeof(TrackManager), true)]
    public class TrackManagerEditor : Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var container = UIHelper.Create();

            InspectorElement.FillDefaultInspector(container, serializedObject, this);

            var trackManager = serializedObject.targetObject as TrackManager;
            if (trackManager)
            {
                container.Add(
                    UIHelper.Create<Button>("Randomize Seed", trackManager.RandomizeSeed),
                    UIHelper.Create<Button>("Generate Track", trackManager.GenerateTrack)
                );
            }

            return container;
        }
    }
}