// InputBindingDataDrawer.cs
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomPropertyDrawer(typeof(InputBindingData), true)]
public class InputBindingDataDrawer : PropertyDrawer
{
    // cached list of all concrete IWeaponAction types
    static List<Type> _actionTypes;

    private ReorderableList _actionsList;

    private void EnsureActionTypes()
    {
        if (_actionTypes != null) return;
        _actionTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => typeof(IWeaponAction).IsAssignableFrom(t)
                     && !t.IsInterface && !t.IsAbstract)
            .OrderBy(t => t.Name)
            .ToList();
    }

    public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
    {
        // one line for the foldout
        float h = EditorGUIUtility.singleLineHeight + 4;
        if (!prop.isExpanded) return h;

        // five fields: bindingMode, actionRef, hand, holdTime, cooldown
        h += 5 * (EditorGUIUtility.singleLineHeight + 2) + 4;
        // plus the nested actions list height
        h += GetActionsList(prop).GetHeight() + 4;
        return h;
    }

    public override void OnGUI(Rect rect, SerializedProperty prop, GUIContent label)
    {
        EnsureActionTypes();

        float y = rect.y;
        // --- Foldout using bindingMode name ---
        var modeProp = prop.FindPropertyRelative(nameof(InputBindingData.bindingMode));
        string header = modeProp.enumDisplayNames[modeProp.enumValueIndex];
        prop.isExpanded = EditorGUI.Foldout(
            new Rect(rect.x, y, rect.width, EditorGUIUtility.singleLineHeight),
            prop.isExpanded, header, true);
        y += EditorGUIUtility.singleLineHeight + 2;
        if (!prop.isExpanded) return;

        EditorGUI.indentLevel++;
        var lineH = EditorGUIUtility.singleLineHeight;

        // Draw the five binding fields
        DrawField(ref y, rect, prop, nameof(InputBindingData.bindingMode),   "Mode");
        DrawField(ref y, rect, prop, nameof(InputBindingData.actionRef),     "Input Action");
        DrawField(ref y, rect, prop, nameof(InputBindingData.hand),          null);
        DrawField(ref y, rect, prop, nameof(InputBindingData.holdTime),      "Hold Time");
        DrawField(ref y, rect, prop, nameof(InputBindingData.cooldown),      "Cooldown");

        // --- Nested ReorderableList of ActionBindingData ---
        var list = GetActionsList(prop);
        list.DoList(new Rect(rect.x, y, rect.width, list.GetHeight()));

        EditorGUI.indentLevel--;
    }

    private void DrawField(ref float y, Rect r, SerializedProperty root, string name, string label)
    {
        var p = root.FindPropertyRelative(name);
        var gui = string.IsNullOrEmpty(label) ? new GUIContent(p.displayName)
                                              : new GUIContent(label);
        EditorGUI.PropertyField(
            new Rect(r.x, y, r.width, EditorGUIUtility.singleLineHeight),
            p, gui);
        y += EditorGUIUtility.singleLineHeight + 2;
    }

    private ReorderableList GetActionsList(SerializedProperty bindProp)
    {
        // keep one list per drawer instance
        if (_actionsList != null
            && _actionsList.serializedProperty == bindProp.FindPropertyRelative("bindings"))
            return _actionsList;

        var actionsProp = bindProp.FindPropertyRelative(nameof(InputBindingData.bindings));
        _actionsList = new ReorderableList(
            actionsProp.serializedObject, actionsProp,
            draggable: true, displayHeader: true,
            displayAddButton: true, displayRemoveButton: true
        );

        // header
        _actionsList.drawHeaderCallback = r =>
            EditorGUI.LabelField(r, "Actions");

        // calculate height properly when collapsed vs expanded
        _actionsList.elementHeightCallback = idx =>
        {
            var el = actionsProp.GetArrayElementAtIndex(idx);
            if (!el.isExpanded)
                return EditorGUIUtility.singleLineHeight + 4;

            float h = EditorGUIUtility.singleLineHeight + 4; // foldout
            // triggerPhase
            h += EditorGUIUtility.singleLineHeight + 2;
            // tickRate if needed
            if ((TriggerPhase)el.FindPropertyRelative(nameof(ActionBindingData.triggerPhase))
                   .enumValueIndex == TriggerPhase.OnTick)
            {
                h += EditorGUIUtility.singleLineHeight + 2;
            }
            // full managedReference data
            h += EditorGUI.GetPropertyHeight(
                     el.FindPropertyRelative(nameof(ActionBindingData.action)),
                     includeChildren: true)
                 + 4;
            return h;
        };

        // draw each action
        _actionsList.drawElementCallback = (rect, idx, active, focused) =>
        {
            var el = actionsProp.GetArrayElementAtIndex(idx);
            float y = rect.y + 2;

            // foldout with action type name
            var typeProp = el.FindPropertyRelative(nameof(ActionBindingData.action));
            string title = GetNiceManagedTypeName(typeProp);
            el.isExpanded = EditorGUI.Foldout(
                new Rect(rect.x, y, rect.width, EditorGUIUtility.singleLineHeight),
                el.isExpanded, title, true);
            y += EditorGUIUtility.singleLineHeight + 2;
            if (!el.isExpanded) return;

            EditorGUI.indentLevel++;
            var lineH = EditorGUIUtility.singleLineHeight;

            // triggerPhase
            EditorGUI.PropertyField(
                new Rect(rect.x, y, rect.width, lineH),
                el.FindPropertyRelative(nameof(ActionBindingData.triggerPhase)));
            y += lineH + 2;

            // tickRate if OnTick
            var phase = el.FindPropertyRelative(nameof(ActionBindingData.triggerPhase)).enumValueIndex;
            if ((TriggerPhase)phase == TriggerPhase.OnTick)
            {
                EditorGUI.PropertyField(
                    new Rect(rect.x, y, rect.width, lineH),
                    el.FindPropertyRelative(nameof(ActionBindingData.tickRate)));
                y += lineH + 2;
            }

            // managedReference action data
            EditorGUI.PropertyField(
                new Rect(rect.x, y, rect.width,
                         EditorGUI.GetPropertyHeight(typeProp, true)),
                typeProp, includeChildren: true);

            EditorGUI.indentLevel--;
        };

        // **OVERRIDE** the add‐button so we create a fresh instance each time
        _actionsList.onAddDropdownCallback = (buttonRect, roList) =>
        {
            var menu = new GenericMenu();
            foreach (var t in _actionTypes)
            {
                string nice = ObjectNames.NicifyVariableName(t.Name);
                menu.AddItem(new GUIContent(nice), false, () =>
                {
                    int idx = actionsProp.arraySize;
                    actionsProp.arraySize++;
                    actionsProp.serializedObject.ApplyModifiedProperties();
                    actionsProp.serializedObject.Update();
                    
                    var newEl = actionsProp.GetArrayElementAtIndex(idx);
                    
                    newEl.FindPropertyRelative("action")
                        .managedReferenceValue = Activator.CreateInstance(t);
                    
                    newEl.FindPropertyRelative("triggerPhase").enumValueIndex 
                        = (int)TriggerPhase.OnPerform;

                    actionsProp.serializedObject.ApplyModifiedProperties();
                });
            }
            menu.DropDown(buttonRect);
        };

        // override remove so Unity cleans up correctly
        _actionsList.onRemoveCallback = list =>
        {
            int idx = list.index;
            actionsProp.DeleteArrayElementAtIndex(idx);
            actionsProp.DeleteArrayElementAtIndex(idx);
            actionsProp.serializedObject.ApplyModifiedProperties();
        };

        return _actionsList;
    }

    private static string GetNiceManagedTypeName(SerializedProperty prop)
    {
        if (prop == null) return "None";
        var full = prop.managedReferenceFullTypename;
        if (string.IsNullOrEmpty(full)) return "None";
        int sp = full.IndexOf(' ');
        var after = sp >= 0 ? full.Substring(sp + 1) : full;
        int dot = after.LastIndexOf('.');
        return dot >= 0 ? after.Substring(dot + 1) : after;
    }
}
