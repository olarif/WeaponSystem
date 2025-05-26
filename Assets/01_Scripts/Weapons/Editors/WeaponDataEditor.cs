using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(WeaponDataSO))]
public class WeaponDataEditor : Editor
{
    private SerializedProperty
        pName, pDesc,
        pRightModel, pLeftModel, pPickupPrefab,
        pModelPos, pModelRot, pDefaultHand,
        pBindings;
    private ReorderableList _bindingsList;

    void OnEnable()
    {
        // grab all top‐level SO props
        pName         = serializedObject.FindProperty(nameof(WeaponDataSO.weaponName));
        pDesc         = serializedObject.FindProperty(nameof(WeaponDataSO.weaponDescription));
        pRightModel   = serializedObject.FindProperty(nameof(WeaponDataSO.rightHandModel));
        pLeftModel    = serializedObject.FindProperty(nameof(WeaponDataSO.leftHandModel));
        pPickupPrefab = serializedObject.FindProperty(nameof(WeaponDataSO.pickupPrefab));
        pModelPos     = serializedObject.FindProperty(nameof(WeaponDataSO.modelPositionOffset));
        pModelRot     = serializedObject.FindProperty(nameof(WeaponDataSO.modelRotationOffset));
        pDefaultHand  = serializedObject.FindProperty(nameof(WeaponDataSO.defaultHand));

        // grab the bindings list
        pBindings = serializedObject.FindProperty(nameof(WeaponDataSO.inputBindings));
        if (pBindings == null)
            Debug.LogError("Cannot find inputBindings on WeaponDataSO!");

        // create a simple ReorderableList that defers drawing each element
        _bindingsList = new ReorderableList(
            serializedObject, pBindings,
            draggable: true, displayHeader: true,
            displayAddButton: true, displayRemoveButton: true
        );

        _bindingsList.drawHeaderCallback = rect =>
            EditorGUI.LabelField(rect, "Input Bindings");

        // each element’s UI is entirely handled by our PropertyDrawer
        _bindingsList.drawElementCallback = (rect, idx, a, f) =>
        {
            EditorGUI.PropertyField(
                rect,
                pBindings.GetArrayElementAtIndex(idx),
                includeChildren: true);
        };

        // dynamic height via the drawer’s GetPropertyHeight
        _bindingsList.elementHeightCallback = idx =>
            EditorGUI.GetPropertyHeight(
                pBindings.GetArrayElementAtIndex(idx),
                includeChildren: true)
          + 4;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // draw top‐level
        EditorGUILayout.PropertyField(pName, new GUIContent("Name"));
        EditorGUILayout.PropertyField(pDesc, new GUIContent("Description"));
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(pRightModel, new GUIContent("Right Model"));
        EditorGUILayout.PropertyField(pLeftModel,  new GUIContent("Left Model"));
        EditorGUILayout.PropertyField(pPickupPrefab, new GUIContent("Pickup Prefab"));
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(pModelPos, new GUIContent("Model Position"));
        EditorGUILayout.PropertyField(pModelRot, new GUIContent("Model Rotation"));
        EditorGUILayout.PropertyField(pDefaultHand, new GUIContent("Default Hand"));
        EditorGUILayout.Space();

        // draw the ReorderableList of InputBindingData
        if (pBindings != null)
            _bindingsList.DoLayoutList();
        else
            EditorGUILayout.HelpBox("No inputBindings found!", MessageType.Error);

        serializedObject.ApplyModifiedProperties();
    }
}
