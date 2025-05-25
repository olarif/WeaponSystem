using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Custom inspector for WeaponDataSO that allows editing
/// weapon metadata and input-action bindings with a context menu.
/// </summary>
[CustomEditor(typeof(WeaponDataSO))]
public class WeaponDataSOEditor : Editor
{
    // Top-level properties
    private SerializedProperty pName;
    private SerializedProperty pDesc;
    private SerializedProperty pRightModel;
    private SerializedProperty pLeftModel;
    private SerializedProperty pPickupPrefab;
    private SerializedProperty pModelPos;
    private SerializedProperty pModelRot;
    private SerializedProperty pDefaultHand;

    // Bindings array
    private SerializedProperty pBindings;
    private string _bindingsFieldName;

    // Cached available IWeaponAction types
    private List<Type> _actionTypes;

    /// <summary>
    /// Initialize property references and cache action types.
    /// </summary>
    private void OnEnable()
    {
        // Top-level fields
        pName         = serializedObject.FindProperty("weaponName");
        pDesc         = serializedObject.FindProperty("weaponDescription");
        pRightModel   = serializedObject.FindProperty("rightHandModel");
        pLeftModel    = serializedObject.FindProperty("leftHandModel");
        pPickupPrefab = serializedObject.FindProperty("pickupPrefab");
        pModelPos     = serializedObject.FindProperty("modelPositionOffset");
        pModelRot     = serializedObject.FindProperty("modelRotationOffset");
        pDefaultHand  = serializedObject.FindProperty("defaultHand");

        // Find bindings list (supports both names)
        pBindings = serializedObject.FindProperty("inputBindings");
        _bindingsFieldName = "inputBindings";
        if (pBindings == null)
        {
            pBindings = serializedObject.FindProperty("bindings");
            _bindingsFieldName = "bindings";
        }

        // Gather all concrete IWeaponAction types for Add Action menu
        _actionTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => typeof(IWeaponAction).IsAssignableFrom(t)
                        && !t.IsInterface && !t.IsAbstract)
            .OrderBy(t => t.Name)
            .ToList();
    }

    /// <summary>
    /// Draw the inspector GUI for WeaponDataSO.
    /// </summary>
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Draw basic weapon info
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
        EditorGUILayout.LabelField("Input Bindings", EditorStyles.boldLabel);

        if (pBindings == null)
        {
            // Show error if bindings list not found
            EditorGUILayout.HelpBox(
                $"Could not find a List<InputBindingData> named '{_bindingsFieldName}'",
                MessageType.Error);
        }
        else
        {
            DrawBindings();
        }

        serializedObject.ApplyModifiedProperties();
    }

    /// <summary>
    /// Renders the list of input bindings and their actions.
    /// </summary>
    private void DrawBindings()
    {
        // Button to add a new input binding
        if (GUILayout.Button("Add Binding", GUILayout.MaxWidth(120)))
        {
            Undo.RecordObject(target, "Add Input Binding");
            pBindings.arraySize++;
            serializedObject.ApplyModifiedProperties();
        }

        // Iterate each binding
        for (int i = 0; i < pBindings.arraySize; i++)
        {
            var bindProp = pBindings.GetArrayElementAtIndex(i);
            bindProp.isExpanded = EditorGUILayout.Foldout(
                bindProp.isExpanded, $"Binding #{i + 1}", true);

            if (!bindProp.isExpanded) continue;
            EditorGUI.indentLevel++;

            // Basic binding fields
            EditorGUILayout.PropertyField(
                bindProp.FindPropertyRelative("bindingMode"),
                new GUIContent("Binding Mode"));
            EditorGUILayout.PropertyField(
                bindProp.FindPropertyRelative("actionRef"),
                new GUIContent("Input Action"));
            EditorGUILayout.PropertyField(
                bindProp.FindPropertyRelative("hand"),
                new GUIContent("Hand"));
            EditorGUILayout.PropertyField(
                bindProp.FindPropertyRelative("holdTime"),
                new GUIContent("Hold Time"));
            EditorGUILayout.PropertyField(
                bindProp.FindPropertyRelative("cooldown"),
                new GUIContent("Input Cooldown"));

            // Remove binding button
            if (GUILayout.Button("Remove Binding", GUILayout.MaxWidth(140)))
            {
                Undo.RecordObject(target, "Remove Input Binding");
                pBindings.DeleteArrayElementAtIndex(i);
                serializedObject.ApplyModifiedProperties();
                EditorGUI.indentLevel--;
                break; // Prevent index issues
            }

            // Actions sub-list
            var actionsProp = bindProp.FindPropertyRelative("bindings");
            if (actionsProp == null)
            {
                EditorGUILayout.HelpBox(
                    "Missing 'bindings' list!", MessageType.Warning);
            }
            else
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);

                // Button to add a new weapon action
                if (GUILayout.Button("Add Action", GUILayout.MaxWidth(120)))
                    ShowAddActionMenu(i);

                // Iterate each action in this binding
                for (int j = 0; j < actionsProp.arraySize; j++)
                {
                    var actProp = actionsProp.GetArrayElementAtIndex(j);
                    string title = GetNiceManagedTypeName(
                        actProp.FindPropertyRelative("action"));

                    actProp.isExpanded = EditorGUILayout.Foldout(
                        actProp.isExpanded, title, true);
                    if (!actProp.isExpanded) continue;
                    EditorGUI.indentLevel++;

                    // Trigger phase field
                    EditorGUILayout.PropertyField(
                        actProp.FindPropertyRelative("triggerPhase"),
                        new GUIContent("Trigger Phase"));

                    // Tick rate only for OnTick phase
                    var phaseProp = actProp.FindPropertyRelative("triggerPhase");
                    if ((TriggerPhase)phaseProp.enumValueIndex == TriggerPhase.OnTick)
                    {
                        EditorGUILayout.PropertyField(
                            actProp.FindPropertyRelative("tickRate"),
                            new GUIContent("Tick Rate"));
                    }

                    // Draw managed-reference action data
                    var typeProp = actProp.FindPropertyRelative("action");
                    if (typeProp != null && typeProp.managedReferenceValue != null)
                    {
                        EditorGUILayout.PropertyField(
                            typeProp,
                            new GUIContent("Action Data"),
                            includeChildren: true);
                    }
                    else
                    {
                        EditorGUILayout.HelpBox(
                            "Action data missing or improperly configured.",
                            MessageType.Error);
                    }

                    // Remove action button
                    if (GUILayout.Button("Remove Action", GUILayout.MaxWidth(120)))
                    {
                        Undo.RecordObject(target, "Remove Weapon Action");
                        actionsProp.DeleteArrayElementAtIndex(j);
                        serializedObject.ApplyModifiedProperties();
                        EditorGUI.indentLevel--;
                        break;
                    }

                    EditorGUI.indentLevel--;
                }
            }

            EditorGUI.indentLevel--;
            EditorGUILayout.Space();
        }
    }

    /// <summary>
    /// Creates a context menu for adding a new IWeaponAction instance.
    /// </summary>
    private void ShowAddActionMenu(int bindingIndex)
    {
        var menu = new GenericMenu();
        foreach (var type in _actionTypes)
        {
            string displayName = ObjectNames.NicifyVariableName(type.Name);
            menu.AddItem(new GUIContent(displayName), false, () =>
            {
                serializedObject.Update();

                var bindProp = pBindings.GetArrayElementAtIndex(bindingIndex);
                var actionsProp = bindProp.FindPropertyRelative("bindings");

                Undo.RecordObject(target, "Add Weapon Action");

                // Expand array and create new element
                int newIndex = actionsProp.arraySize;
                actionsProp.arraySize++;
                serializedObject.ApplyModifiedProperties();
                serializedObject.Update();

                // Instantiate the managed reference
                bindProp = pBindings.GetArrayElementAtIndex(bindingIndex);
                actionsProp = bindProp.FindPropertyRelative("bindings");
                var newActionProp = actionsProp.GetArrayElementAtIndex(newIndex);
                var actionProp = newActionProp.FindPropertyRelative("action");
                actionProp.managedReferenceValue = Activator.CreateInstance(type);

                // Default trigger phase to OnPerform
                var phaseProp = newActionProp.FindPropertyRelative("triggerPhase");
                phaseProp.enumValueIndex = (int)TriggerPhase.OnPerform;

                serializedObject.ApplyModifiedProperties();
                serializedObject.Update();
            });
        }
        menu.ShowAsContext();
    }

    /// <summary>
    /// Extracts a clean type name from a managedReferenceFullTypename.
    /// </summary>
    private static string GetNiceManagedTypeName(SerializedProperty prop)
    {
        if (prop == null || string.IsNullOrEmpty(prop.managedReferenceFullTypename))
            return "None";

        string fullname = prop.managedReferenceFullTypename;
        int spaceIdx = fullname.IndexOf(' ');
        string after = (spaceIdx >= 0 && spaceIdx < fullname.Length - 1)
            ? fullname.Substring(spaceIdx + 1)
            : fullname;

        int dotIdx = after.LastIndexOf('.');
        return (dotIdx >= 0)
            ? after.Substring(dotIdx + 1)
            : after;
    }
}
