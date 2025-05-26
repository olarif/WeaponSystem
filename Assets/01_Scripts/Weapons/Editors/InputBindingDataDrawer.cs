using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomPropertyDrawer(typeof(InputBindingData), true)]
public class InputBindingDataDrawer : PropertyDrawer
{
    // cached action types
    static List<Type> _actionTypes;

    // one ReorderableList per drawer instance
    private ReorderableList _actionsList;

    // gather all IWeaponAction types once
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
        float h = EditorGUIUtility.singleLineHeight + 4;      // foldout
        if (!prop.isExpanded) return h;

        // 5 fields: bindingMode, actionRef, hand, holdTime, cooldown
        h += 5 * (EditorGUIUtility.singleLineHeight + 2) + 4;

        // nested list height
        h += GetActionsList(prop).GetHeight() + 4;
        return h;
    }

    public override void OnGUI(Rect rect, SerializedProperty prop, GUIContent label)
    {
        EnsureActionTypes();

        float y = rect.y;
        var modeProp = prop.FindPropertyRelative(nameof(InputBindingData.bindingMode));
        string header = modeProp.enumDisplayNames[modeProp.enumValueIndex];

        // foldout
        prop.isExpanded = EditorGUI.Foldout(
            new Rect(rect.x, y, rect.width, EditorGUIUtility.singleLineHeight),
            prop.isExpanded, header, true);
        y += EditorGUIUtility.singleLineHeight + 2;
        if (!prop.isExpanded) return;

        EditorGUI.indentLevel++;
        // draw the five fields
        DrawField(ref y, rect, prop, nameof(InputBindingData.bindingMode),   "Mode");
        DrawField(ref y, rect, prop, nameof(InputBindingData.actionRef),     "Input Action");
        DrawField(ref y, rect, prop, nameof(InputBindingData.hand),          null);
        DrawField(ref y, rect, prop, nameof(InputBindingData.holdTime),      "Hold Time");
        DrawField(ref y, rect, prop, nameof(InputBindingData.cooldown),      "Cooldown");

        // nested actions list
        var list = GetActionsList(prop);
        list.DoList(new Rect(rect.x, y, rect.width, list.GetHeight()));

        EditorGUI.indentLevel--;
    }

    private void DrawField(
        ref float y, Rect rect,
        SerializedProperty root, string propName, string label)
    {
        var sp = root.FindPropertyRelative(propName);
        var guiLabel = string.IsNullOrEmpty(label)
            ? new GUIContent(sp.displayName)
            : new GUIContent(label);
        EditorGUI.PropertyField(
            new Rect(rect.x, y, rect.width, EditorGUIUtility.singleLineHeight),
            sp, guiLabel);
        y += EditorGUIUtility.singleLineHeight + 2;
    }

    private ReorderableList GetActionsList(SerializedProperty bindProp)
    {
        var actionsProp = bindProp.FindPropertyRelative(nameof(InputBindingData.bindings));
        if (_actionsList != null &&
            _actionsList.serializedProperty == actionsProp)
            return _actionsList;

        _actionsList = new ReorderableList(
            actionsProp.serializedObject, actionsProp,
            draggable: true, displayHeader: true,
            displayAddButton: true, displayRemoveButton: true
        );

        _actionsList.drawHeaderCallback = r =>
            EditorGUI.LabelField(r, "Actions");

        _actionsList.elementHeightCallback = idx =>
        {
            var el = actionsProp.GetArrayElementAtIndex(idx);
            // if the element is *not* expanded, only use one line
            if (!el.isExpanded)
                return EditorGUIUtility.singleLineHeight + 4;

            // otherwise calculate the full expanded height:
            float h = EditorGUIUtility.singleLineHeight + 4; // foldout line
            // triggerPhase line
            h += EditorGUIUtility.singleLineHeight + 2;
            // tickRate, if OnTick
            if ((TriggerPhase)el.FindPropertyRelative("triggerPhase").enumValueIndex
                == TriggerPhase.OnTick)
                h += EditorGUIUtility.singleLineHeight + 2;
            // then the managed‐reference “action” data
            h += EditorGUI.GetPropertyHeight(
                     el.FindPropertyRelative("action"),
                     includeChildren: true)
                 + 4;
            return h;
        };

        _actionsList.drawElementCallback = (r, i, a, f) =>
        {
            var el = actionsProp.GetArrayElementAtIndex(i);
            float yy = r.y + 2;

            // foldout showing the action type
            var typeProp = el.FindPropertyRelative(nameof(ActionBindingData.action));
            string typeName = GetNiceName(typeProp);
            el.isExpanded = EditorGUI.Foldout(
                new Rect(r.x, yy, r.width, EditorGUIUtility.singleLineHeight),
                el.isExpanded, typeName, true);
            yy += EditorGUIUtility.singleLineHeight + 2;
            if (!el.isExpanded) return;

            EditorGUI.indentLevel++;
            // draw the entire action object
            EditorGUI.PropertyField(
                new Rect(r.x, yy, r.width,
                    EditorGUI.GetPropertyHeight(typeProp, true)),
                typeProp, true);
            EditorGUI.indentLevel--;
        };

        // pop up a GenericMenu at the + button
        _actionsList.onAddDropdownCallback = (buttonRect, list) =>
            ShowAddActionMenu(bindProp, buttonRect);

        return _actionsList;
    }

    private void ShowAddActionMenu(
        SerializedProperty bindProp, Rect buttonRect)
    {
        var menu = new GenericMenu();
        foreach (var t in _actionTypes)
        {
            string nice = ObjectNames.NicifyVariableName(t.Name);
            menu.AddItem(new GUIContent(nice), false, () =>
            {
                AddAction(bindProp, t);
            });
        }
        menu.DropDown(buttonRect);
    }

    private void AddAction(SerializedProperty bindProp, Type type)
    {
        var actionsProp = bindProp
            .FindPropertyRelative(nameof(InputBindingData.bindings));
        actionsProp.arraySize++;
        actionsProp.serializedObject.ApplyModifiedProperties();
        actionsProp.serializedObject.Update();

        var newEl = actionsProp.GetArrayElementAtIndex(actionsProp.arraySize - 1);
        newEl.FindPropertyRelative(nameof(ActionBindingData.action))
             .managedReferenceValue = Activator.CreateInstance(type);
        // default to OnPerform
        newEl.FindPropertyRelative(nameof(ActionBindingData.triggerPhase))
             .enumValueIndex = (int)TriggerPhase.OnPerform;

        actionsProp.serializedObject.ApplyModifiedProperties();
    }

    private string GetNiceName(SerializedProperty prop)
    {
        var full = prop.managedReferenceFullTypename;
        if (string.IsNullOrEmpty(full)) return "None";
        var after = full.Substring(full.IndexOf(' ') + 1);
        return after.Substring(after.LastIndexOf('.') + 1);
    }
}
