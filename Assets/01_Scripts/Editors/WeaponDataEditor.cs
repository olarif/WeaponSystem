#if UNITY_EDITOR
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CustomEditor(typeof(WeaponDataSO))]
public class WeaponDataEditor : Editor {
    private SerializedProperty weaponNameProp;
    private SerializedProperty weaponDescriptionProp;
    private SerializedProperty weaponPrefabProp;
    private SerializedProperty inputComponentsProp;
    private SerializedProperty executeComponentsProp;
    private SerializedProperty onHitComponentsProp;

    private Dictionary<ScriptableObject, Editor> componentEditors = new Dictionary<ScriptableObject, Editor>();
    private Dictionary<string, List<Type>> availableComponentTypes;

    private bool showInput = true;
    private bool showExecute = true;
    private bool showOnHit = true;

    private void OnEnable() {
        weaponNameProp        = serializedObject.FindProperty("weaponName");
        weaponDescriptionProp = serializedObject.FindProperty("weaponDescription");
        weaponPrefabProp      = serializedObject.FindProperty("weaponPrefab");
        inputComponentsProp   = serializedObject.FindProperty("inputComponents");
        executeComponentsProp = serializedObject.FindProperty("executeComponents");
        onHitComponentsProp   = serializedObject.FindProperty("onHitComponents");

        CacheAvailableComponentTypes();
        InitializeComponentEditors();
    }

    private void OnDisable() {
        foreach (var ed in componentEditors.Values)
            DestroyImmediate(ed);
        componentEditors.Clear();
    }

    private void CacheAvailableComponentTypes() {
        availableComponentTypes = new Dictionary<string, List<Type>> {
            { "Input", new List<Type>() },
            { "Execute", new List<Type>() },
            { "OnHit", new List<Type>() }
        };
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (var assembly in assemblies) {
            foreach (var type in assembly.GetTypes()
                         .Where(t => typeof(ScriptableObject).IsAssignableFrom(t) && !t.IsAbstract)) {
                if (typeof(InputComponent).IsAssignableFrom(type))
                    availableComponentTypes["Input"].Add(type);
                else if (typeof(ExecuteComponent).IsAssignableFrom(type))
                    availableComponentTypes["Execute"].Add(type);
                else if (typeof(OnHitComponent).IsAssignableFrom(type))
                    availableComponentTypes["OnHit"].Add(type);
            }
        }
    }

    public override void OnInspectorGUI() {
        serializedObject.Update();

        EditorGUILayout.PropertyField(weaponNameProp);
        EditorGUILayout.PropertyField(weaponDescriptionProp);
        EditorGUILayout.PropertyField(weaponPrefabProp);
        EditorGUILayout.Space();

        DrawComponentSection("Input Components", ref showInput, inputComponentsProp, availableComponentTypes["Input"]);
        DrawComponentSection("Execute Components", ref showExecute, executeComponentsProp, availableComponentTypes["Execute"]);
        DrawComponentSection("On-Hit Components", ref showOnHit, onHitComponentsProp, availableComponentTypes["OnHit"]);

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawComponentSection(string title, ref bool show, SerializedProperty listProp, List<Type> types) {
        show = EditorGUILayout.Foldout(show, title, true);
        if (!show) return;
        EditorGUI.indentLevel++;

        // Existing components
        for (int i = 0; i < listProp.arraySize; i++) {
            var element = listProp.GetArrayElementAtIndex(i);
            var so = element.objectReferenceValue as ScriptableObject;
            if (so == null) continue;

            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(so.GetType().Name, EditorStyles.boldLabel);
            if (GUILayout.Button("Remove", GUILayout.Width(70), GUILayout.Height(20))) {
                RemoveComponent(listProp, i, so);
                return; // exit to avoid enumeration issues
            }
            EditorGUILayout.EndHorizontal();

            // Draw sub-inspector
            if (!componentEditors.TryGetValue(so, out var ed) || ed == null)
                componentEditors[so] = CreateEditor(so);
            componentEditors[so].OnInspectorGUI();
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        // Add buttons
        EditorGUILayout.BeginHorizontal();
        foreach (var t in types) {
            if (GUILayout.Button($"Add {t.Name}", GUILayout.Height(30))) {
                AddComponent(listProp, t);
            }
        }
        EditorGUILayout.EndHorizontal();
        EditorGUI.indentLevel--;
        EditorGUILayout.Space();
    }

    private void AddComponent(SerializedProperty listProp, Type type) {
        var data = (WeaponDataSO)target;
        var comp = ScriptableObject.CreateInstance(type);
        comp.name = type.Name;
        comp.hideFlags = HideFlags.HideInHierarchy;
        AssetDatabase.AddObjectToAsset(comp, AssetDatabase.GetAssetPath(data));
        AssetDatabase.SaveAssets();

        serializedObject.Update();
        listProp.arraySize++;
        listProp.GetArrayElementAtIndex(listProp.arraySize - 1).objectReferenceValue = comp;
        serializedObject.ApplyModifiedProperties();

        componentEditors[comp] = CreateEditor(comp);
    }

    private void RemoveComponent(SerializedProperty listProp, int index, ScriptableObject comp) {
        serializedObject.Update();
        listProp.DeleteArrayElementAtIndex(index);
        AssetDatabase.RemoveObjectFromAsset(comp);
        serializedObject.ApplyModifiedProperties();
        componentEditors.Remove(comp);
    }

    private void InitializeComponentEditors() {
        // Initialize editors for existing components
        for (int i = 0; i < inputComponentsProp.arraySize; i++) {
            var so = inputComponentsProp.GetArrayElementAtIndex(i).objectReferenceValue as ScriptableObject;
            if (so != null) componentEditors[so] = CreateEditor(so);
        }
        for (int i = 0; i < executeComponentsProp.arraySize; i++) {
            var so = executeComponentsProp.GetArrayElementAtIndex(i).objectReferenceValue as ScriptableObject;
            if (so != null) componentEditors[so] = CreateEditor(so);
        }
        for (int i = 0; i < onHitComponentsProp.arraySize; i++) {
            var so = onHitComponentsProp.GetArrayElementAtIndex(i).objectReferenceValue as ScriptableObject;
            if (so != null) componentEditors[so] = CreateEditor(so);
        }
    }
}
#endif