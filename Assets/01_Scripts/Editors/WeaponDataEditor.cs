#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WeaponDataSO))]
public class WeaponDataEditor : Editor
{
    private Dictionary<ScriptableObject, Editor> componentEditors = new Dictionary<ScriptableObject, Editor>();
    private bool showInputComponents = true;
    private bool showExecuteComponents = true;
    private bool showOnHitComponents = true;

    // Get all available component types using reflection
    private Dictionary<string, List<Type>> availableComponentTypes;

    private void OnEnable()
    {
        InitializeComponentEditors();
        CacheAvailableComponentTypes();
    }

    private void OnDisable()
    {
        foreach (var ed in componentEditors.Values)
            DestroyImmediate(ed);
        componentEditors.Clear();
    }

    private void CacheAvailableComponentTypes()
    {
        availableComponentTypes = new Dictionary<string, List<Type>>
        {
            { "inputComponents", new List<Type>() },
            { "executeComponents", new List<Type>() },
            { "onHitComponents", new List<Type>() }
        };

        // Search in all loaded assemblies, not just the ScriptableObject assembly
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        var scriptableObjectType = typeof(ScriptableObject);
        
        // Debug output - display what we found
        //Debug.Log("Searching for weapon component types...");
        int totalTypesFound = 0;

        foreach (var assembly in assemblies)
        {
            if (assembly.GetName().Name.StartsWith("Unity.") || 
                assembly.GetName().Name.StartsWith("System.") ||
                assembly.GetName().Name.StartsWith("UnityEditor") ||
                assembly.GetName().Name.StartsWith("mscorlib"))
                continue; // Skip Unity/System assemblies to improve performance
            
            try
            {
                var types = assembly.GetTypes()
                    .Where(t => scriptableObjectType.IsAssignableFrom(t) && !t.IsAbstract && t != typeof(WeaponDataSO))
                    .ToList();
                
                totalTypesFound += types.Count;
                
                foreach (var type in types)
                {
                    try {
                        // Manual component we know exists (fallback to ensure we have at least one button)
                        if (type.Name == "ActionInput")
                        {
                            availableComponentTypes["inputComponents"].Add(type);
                            //Debug.Log($"Found Input component: {type.Name}");
                            continue;
                        }

                        if (type.Name == "ProjectileExecute")
                        {
                            availableComponentTypes["executeComponents"].Add(type);
                            //Debug.Log($"Found Execute component: {type.Name}");
                            continue;
                        }
                        
                        // Find by name patterns
                        if (type.Name.EndsWith("Input") || 
                            type.Name.Contains("Input") || 
                            HasInterface(type, "IActionInput"))
                        {
                            availableComponentTypes["inputComponents"].Add(type);
                            //Debug.Log($"Found Input component: {type.Name}");
                        }
                        else if (type.Name.EndsWith("Execute") || 
                                type.Name.Contains("Execute") || 
                                HasInterface(type, "IExecuteComponent"))
                        {
                            availableComponentTypes["executeComponents"].Add(type);
                            //Debug.Log($"Found Execute component: {type.Name}");
                        }
                        else if (type.Name.EndsWith("Hit") || 
                                type.Name.Contains("OnHit") || 
                                type.Name.Contains("HitComponent") || 
                                HasInterface(type, "IOnHitComponent"))
                        {
                            availableComponentTypes["onHitComponents"].Add(type);
                            //Debug.Log($"Found OnHit component: {type.Name}");
                        }
                    }
                    catch (Exception ex) {
                        //Debug.LogWarning($"Error checking type {type.Name}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Error loading types from assembly {assembly.GetName().Name}: {ex.Message}");
            }
        }
        
        // If no components were found at all, manually add the specific types we saw in the original code
        if (availableComponentTypes["inputComponents"].Count == 0 && 
            availableComponentTypes["executeComponents"].Count == 0 && 
            availableComponentTypes["onHitComponents"].Count == 0)
        {
            Debug.LogWarning("No component types found through reflection. Adding fallback components.");
            
            // Manually add common component types
            TryAddTypeByName("ActionInput", "inputComponents");
            TryAddTypeByName("ProjectileExecute", "executeComponents");
        }
        
        // Log summary
        //Debug.Log($"Component discovery complete: {totalTypesFound} ScriptableObject types found");
        Debug.Log($"Input Components: {availableComponentTypes["inputComponents"].Count}");
        Debug.Log($"Execute Components: {availableComponentTypes["executeComponents"].Count}");
        Debug.Log($"OnHit Components: {availableComponentTypes["onHitComponents"].Count}");
    }
    
    private bool HasInterface(Type type, string interfaceName)
    {
        return type.GetInterfaces().Any(i => i.Name == interfaceName);
    }
    
    private void TryAddTypeByName(string typeName, string category)
    {
        // Try to find the type by name in all assemblies
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            try
            {
                var type = assembly.GetType(typeName);
                if (type == null)
                {
                    // Try with namespace
                    var types = assembly.GetTypes()
                        .Where(t => t.Name == typeName && typeof(ScriptableObject).IsAssignableFrom(t))
                        .ToList();
                        
                    if (types.Count > 0)
                    {
                        type = types[0];
                    }
                }
                
                if (type != null && typeof(ScriptableObject).IsAssignableFrom(type) && !type.IsAbstract)
                {
                    availableComponentTypes[category].Add(type);
                    Debug.Log($"Manually added {type.Name} to {category}");
                    return;
                }
            }
            catch { }
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        var data = (WeaponDataSO)target;

        // Draw default properties
        DrawPropertiesExcluding(serializedObject, "inputComponents", "executeComponents", "onHitComponents");
        EditorGUILayout.Space();

        // Input Components
        showInputComponents = EditorGUILayout.Foldout(showInputComponents, "Input Components", true);
        if (showInputComponents)
        {
            EditorGUI.indentLevel++;
            DrawComponentList("inputComponents", data.inputComponents);
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();
        }

        // Execute Components
        showExecuteComponents = EditorGUILayout.Foldout(showExecuteComponents, "Execute Components", true);
        if (showExecuteComponents)
        {
            EditorGUI.indentLevel++;
            DrawComponentList("executeComponents", data.executeComponents);
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();
        }

        // On-Hit Components
        showOnHitComponents = EditorGUILayout.Foldout(showOnHitComponents, "On-Hit Components", true);
        if (showOnHitComponents)
        {
            EditorGUI.indentLevel++;
            DrawComponentList("onHitComponents", data.onHitComponents);
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();
        }

        // Add Component Buttons
        DrawComponentButtons();

        // Clear All
        EditorGUILayout.Space();
        if (GUILayout.Button("Clear All Components"))
        {
            if (EditorUtility.DisplayDialog("Clear All Components", 
                "Are you sure you want to remove all components? This action cannot be undone.", "Yes", "Cancel"))
            {
                ClearAllComponents(data.inputComponents, "inputComponents");
                ClearAllComponents(data.executeComponents, "executeComponents");
                ClearAllComponents(data.onHitComponents, "onHitComponents");
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawComponentButtons()
    {
        EditorGUILayout.Space();

        // If no components were found, draw a fallback manual button section
        if ((availableComponentTypes["inputComponents"].Count == 0) &&
            (availableComponentTypes["executeComponents"].Count == 0) &&
            (availableComponentTypes["onHitComponents"].Count == 0))
        {
            // Add manual buttons for Input Components
            EditorGUILayout.LabelField("Add Input Components", EditorStyles.boldLabel);
            if (GUILayout.Button("Action Input", GUILayout.Height(25)))
            {
                AddComponentByName("ActionInput", "inputComponents");
            }
            EditorGUILayout.Space();

            // Add manual buttons for Execute Components
            EditorGUILayout.LabelField("Add Execute Components", EditorStyles.boldLabel);
            if (GUILayout.Button("Projectile Execute", GUILayout.Height(25)))
            {
                AddComponentByName("ProjectileExecute", "executeComponents");
            }
            EditorGUILayout.Space();

            // Add manual buttons for OnHit Components
            EditorGUILayout.LabelField("Add On-Hit Components", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("No OnHit components found in project", MessageType.Info);
            EditorGUILayout.Space();
            
            return;
        }

        // Draw input component buttons
        if (availableComponentTypes["inputComponents"].Count > 0)
        {
            EditorGUILayout.LabelField("Add Input Components", EditorStyles.boldLabel);
            DrawComponentTypeButtons(availableComponentTypes["inputComponents"], "inputComponents");
            EditorGUILayout.Space();
        }

        // Draw execute component buttons
        if (availableComponentTypes["executeComponents"].Count > 0)
        {
            EditorGUILayout.LabelField("Add Execute Components", EditorStyles.boldLabel);
            DrawComponentTypeButtons(availableComponentTypes["executeComponents"], "executeComponents");
            EditorGUILayout.Space();
        }

        // Draw on-hit component buttons
        if (availableComponentTypes["onHitComponents"].Count > 0)
        {
            EditorGUILayout.LabelField("Add On-Hit Components", EditorStyles.boldLabel);
            DrawComponentTypeButtons(availableComponentTypes["onHitComponents"], "onHitComponents");
            EditorGUILayout.Space();
        }
    }

    private void DrawComponentTypeButtons(List<Type> types, string propertyName)
    {
        EditorGUILayout.BeginVertical();
        int buttonsPerRow = 2;
        int buttonCount = 0;

        EditorGUILayout.BeginHorizontal();
        foreach (var type in types)
        {
            string buttonName = type.Name;
            // Remove common suffixes for cleaner button names
            if (buttonName.EndsWith("Input") || buttonName.EndsWith("Execute") || buttonName.EndsWith("Hit"))
                buttonName = buttonName.Substring(0, buttonName.Length - 5);
            
            if (GUILayout.Button(buttonName, GUILayout.Height(25)))
            {
                AddComponentOfType(type, propertyName);
            }

            buttonCount++;
            if (buttonCount % buttonsPerRow == 0)
            {
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
            }
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
    }
    
    //

    private void DrawComponentList(string propertyName, List<ScriptableObject> list)
    {
        var prop = serializedObject.FindProperty(propertyName);
        
        for (int i = 0; i < list.Count; i++)
        {
            var comp = list[i];
            if (comp == null) continue;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            
            // Component name with nicer formatting
            string typeName = comp.GetType().Name;
            if (typeName.EndsWith("Input") || typeName.EndsWith("Execute") || typeName.EndsWith("Hit"))
                typeName = typeName.Substring(0, typeName.Length - 5);
            
            EditorGUILayout.LabelField(typeName, EditorStyles.boldLabel);
            
            if (GUILayout.Button("Remove", GUILayout.Width(70)))
            {
                // We need to delay the actual removal to avoid modifying the collection during iteration
                EditorApplication.delayCall += () => {
                    // Get the latest property since we're in a delayed call
                    serializedObject.Update();
                    var currentProp = serializedObject.FindProperty(propertyName);
                    
                    // Find and remove the component
                    for (int j = 0; j < currentProp.arraySize; j++)
                    {
                        if (currentProp.GetArrayElementAtIndex(j).objectReferenceValue == comp)
                        {
                            // Remove from the editor dictionary first
                            if (componentEditors.ContainsKey(comp))
                            {
                                DestroyImmediate(componentEditors[comp]);
                                componentEditors.Remove(comp);
                            }
                            
                            // Remove from the serialized array
                            currentProp.DeleteArrayElementAtIndex(j);
                            
                            // Remove as sub-asset
                            AssetDatabase.RemoveObjectFromAsset(comp);
                            EditorUtility.SetDirty(target);
                            AssetDatabase.SaveAssets();
                            break;
                        }
                    }
                    
                    serializedObject.ApplyModifiedProperties();
                };
                
                GUIUtility.ExitGUI();
                return;
            }
            
            EditorGUILayout.EndHorizontal();

            DrawDivider();
            
            // Display the component's inspector
            GetOrCreateEditor(comp).OnInspectorGUI();
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }
    }

    private void AddComponentOfType(Type componentType, string propertyName)
    {
        var data = (WeaponDataSO)target;
        var prop = serializedObject.FindProperty(propertyName);
        
        // Check if the component already exists and if we should allow multiple instances
        bool allowMultiple = true; // Change this if you want to restrict certain components
        
        if (!allowMultiple)
        {
            for (int i = 0; i < prop.arraySize; i++)
            {
                var existingComp = prop.GetArrayElementAtIndex(i).objectReferenceValue;
                if (existingComp != null && existingComp.GetType() == componentType)
                {
                    EditorUtility.DisplayDialog("Component Already Exists", 
                        $"This weapon already has a {componentType.Name} component.", "OK");
                    return;
                }
            }
        }
        
        // Create the ScriptableObject sub-asset
        var comp = CreateInstance(componentType);
        comp.name = componentType.Name;
        comp.hideFlags = HideFlags.HideInHierarchy;
        
        // Add to asset file
        AssetDatabase.AddObjectToAsset(comp, AssetDatabase.GetAssetPath(data));
        
        // Add to serialized list
        serializedObject.Update();
        prop.arraySize++;
        prop.GetArrayElementAtIndex(prop.arraySize - 1).objectReferenceValue = comp;
        serializedObject.ApplyModifiedProperties();
        
        EditorUtility.SetDirty(target);
        AssetDatabase.SaveAssets();
        
        // Create editor for the new component
        componentEditors[comp] = CreateEditor(comp);
    }

    private void ClearAllComponents(List<ScriptableObject> list, string propertyName)
    {
        var prop = serializedObject.FindProperty(propertyName);
        serializedObject.Update();
        
        // Remove all components in the list
        for (int i = prop.arraySize - 1; i >= 0; i--)
        {
            var element = prop.GetArrayElementAtIndex(i).objectReferenceValue as ScriptableObject;
            if (element != null)
            {
                if (componentEditors.ContainsKey(element))
                {
                    DestroyImmediate(componentEditors[element]);
                    componentEditors.Remove(element);
                }
                AssetDatabase.RemoveObjectFromAsset(element);
            }
            prop.DeleteArrayElementAtIndex(i);
        }
        
        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(target);
        AssetDatabase.SaveAssets();
    }

    private void InitializeComponentEditors()
    {
        foreach (var ed in componentEditors.Values)
            DestroyImmediate(ed);
        componentEditors.Clear();

        var data = (WeaponDataSO)target;
        
        // Create editors for all component lists
        AddEditorsForList(data.inputComponents);
        AddEditorsForList(data.executeComponents);
        AddEditorsForList(data.onHitComponents);
    }

    private void AddEditorsForList(List<ScriptableObject> list)
    {
        if (list == null) return;
        
        foreach (var comp in list)
        {
            if (comp != null && !componentEditors.ContainsKey(comp))
                componentEditors[comp] = CreateEditor(comp);
        }
    }
    
    private void AddComponentByName(string typeName, string propertyName)
    {
        Type componentType = null;
        
        // Search for the type by name in all assemblies
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            try
            {
                // Try to find the type with exact name first
                componentType = assembly.GetType(typeName);
                
                // If not found, try searching for classes that match just the name part
                if (componentType == null)
                {
                    var types = assembly.GetTypes()
                        .Where(t => t.Name == typeName && typeof(ScriptableObject).IsAssignableFrom(t) && !t.IsAbstract)
                        .ToList();
                        
                    if (types.Count > 0)
                    {
                        componentType = types[0];
                        break;
                    }
                }
                else if (typeof(ScriptableObject).IsAssignableFrom(componentType) && !componentType.IsAbstract)
                {
                    break;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Error searching for type {typeName} in assembly {assembly.GetName().Name}: {ex.Message}");
            }
        }
        
        if (componentType == null)
        {
            // If type is still not found, let's try creating a generic component
            Debug.LogWarning($"Could not find component type: {typeName}");
            EditorUtility.DisplayDialog("Component Not Found", 
                $"Could not find component type: {typeName}. Please ensure this class exists in your project.", "OK");
            return;
        }
        
        // Now that we have the type, add it
        AddComponentOfType(componentType, propertyName);
    }

    private Editor GetOrCreateEditor(ScriptableObject comp)
    {
        if (!componentEditors.TryGetValue(comp, out var ed) || ed == null)
            componentEditors[comp] = ed = CreateEditor(comp);
        return ed;
    }

    private void DrawDivider()
    {
        EditorGUILayout.Space();
        EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, 1), new Color(0.5f, 0.5f, 0.5f, 1));
        EditorGUILayout.Space();
    }
}
#endif