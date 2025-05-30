using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditorInternal;

[CustomEditor(typeof(WeaponDataSO))]
public class WeaponDataEditor : Editor
{
    private SerializedProperty[] _basicProperties;
    private SerializedProperty[] _modelProperties;
    private SerializedProperty[] _transformProperties;
    private SerializedProperty _bindingsProperty;
    
    private ReorderableList _bindingsList;
    private bool _showPreview = true;
    private Vector2 _scrollPosition;
    
    private static readonly GUIContent[] _sectionHeaders = {
        new("Basic Information", "Core weapon identification and description"),
        new("Model Setup", "3D models and pickup prefab configuration"),
        new("Transform Settings", "Position and rotation offsets for weapon models"),
        new("Input Bindings", "Configure weapon input actions and behaviors")
    };

    void OnEnable()
    {
        CacheProperties();
        SetupBindingsList();
    }

    private void CacheProperties()
    {
        _basicProperties = new[] {
            serializedObject.FindProperty("weaponName"),
            serializedObject.FindProperty("weaponDescription")
        };

        _modelProperties = new[] {
            serializedObject.FindProperty("rightHandModel"),
            serializedObject.FindProperty("leftHandModel"),
            serializedObject.FindProperty("pickupPrefab")
        };

        _transformProperties = new[] {
            serializedObject.FindProperty("modelPositionOffset"),
            serializedObject.FindProperty("modelRotationOffset"),
            serializedObject.FindProperty("defaultHand")
        };

        _bindingsProperty = serializedObject.FindProperty("inputBindings");
    }

    private void SetupBindingsList()
    {
        if (_bindingsProperty == null)
        {
            Debug.LogError("Cannot find inputBindings property on WeaponDataSO!");
            return;
        }

        _bindingsList = new ReorderableList(serializedObject, _bindingsProperty, true, true, true, true);
        
        _bindingsList.drawHeaderCallback = rect => {
            var headerRect = new Rect(rect.x, rect.y, rect.width - 60f, rect.height);
            EditorGUI.LabelField(headerRect, "Input Bindings", EditorStyles.boldLabel);
            
            var previewRect = new Rect(rect.xMax - 60f, rect.y, 60f, rect.height);
            _showPreview = EditorGUI.Toggle(previewRect, new GUIContent("Preview"), _showPreview);
        };

        _bindingsList.drawElementCallback = (rect, index, active, focused) => {
            var element = _bindingsProperty.GetArrayElementAtIndex(index);
            var height = EditorGUI.GetPropertyHeight(element, true);
            var elementRect = new Rect(rect.x, rect.y, rect.width, height);
            
            EditorGUI.PropertyField(elementRect, element, true);
        };

        _bindingsList.elementHeightCallback = index => {
            var element = _bindingsProperty.GetArrayElementAtIndex(index);
            return EditorGUI.GetPropertyHeight(element, true) + 4f;
        };

        _bindingsList.onAddCallback = list =>
        {
            serializedObject.Update();

            // 1) Grow the array
            int newIndex = _bindingsProperty.arraySize;
            _bindingsProperty.arraySize++;
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();

            // 2) Grab the brand-new element
            var newElem = _bindingsProperty.GetArrayElementAtIndex(newIndex);

            // 3) Reset *every* field on it
            newElem.FindPropertyRelative(nameof(InputBindingData.bindingMode))
                .enumValueIndex = (int)BindingMode.Press;
            newElem.FindPropertyRelative(nameof(InputBindingData.actionRef))
                .objectReferenceValue = null;
            newElem.FindPropertyRelative(nameof(InputBindingData.hand))
                .enumValueIndex = (int)Hand.Right;
            newElem.FindPropertyRelative(nameof(InputBindingData.holdTime))
                .floatValue = 0f;
            newElem.FindPropertyRelative(nameof(InputBindingData.cooldown))
                .floatValue = 0f;

            // 4) Clear out its nested Actions list
            var actionsProp = newElem.FindPropertyRelative(nameof(InputBindingData.bindings));
            actionsProp.ClearArray();

            // 5) Make sure the foldout starts closed
            newElem.isExpanded = false;

            serializedObject.ApplyModifiedProperties();
        };
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Header with weapon name
        DrawHeader();
        
        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

        // Main sections
        DrawSection(_sectionHeaders[0], () => DrawBasicInfo());
        DrawSection(_sectionHeaders[1], () => DrawModelSetup());
        DrawSection(_sectionHeaders[2], () => DrawTransformSettings());
        DrawSection(_sectionHeaders[3], () => DrawInputBindings());

        // Preview section
        if (_showPreview)
        {
            DrawSection(new GUIContent("Preview", "Runtime preview and validation"), DrawPreview);
        }

        EditorGUILayout.EndScrollView();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawHeader()
    {
        var weaponData = target as WeaponDataSO;
        string weaponName = string.IsNullOrEmpty(weaponData.weaponName) ? "Unnamed Weapon" : weaponData.weaponName;
        
        EditorGUILayout.Space();
        var headerStyle = new GUIStyle(EditorStyles.largeLabel) {
            fontSize = 18,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        
        EditorGUILayout.LabelField($"⚔ {weaponName}", headerStyle);
        
        if (!string.IsNullOrEmpty(weaponData.weaponDescription))
        {
            var descStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel) {
                wordWrap = true
            };
            EditorGUILayout.LabelField(weaponData.weaponDescription, descStyle);
        }
        
        EditorGUILayout.Space();
    }

    private void DrawSection(GUIContent header, System.Action drawContent)
    {
        EditorGUILayout.Space(8f);
        
        // Section header with background
        var headerRect = GUILayoutUtility.GetRect(0, 24f, GUILayout.ExpandWidth(true));
        EditorGUI.DrawRect(headerRect, new Color(0.3f, 0.3f, 0.3f, 0.3f));
        
        var iconRect = new Rect(headerRect.x + 8f, headerRect.y + 4f, 16f, 16f);
        var textRect = new Rect(iconRect.xMax + 4f, headerRect.y, headerRect.width - 32f, headerRect.height);
        
        EditorGUI.LabelField(textRect, header, EditorStyles.boldLabel);
        
        // Content area
        var contentRect = EditorGUILayout.BeginVertical("box");
        drawContent?.Invoke();
        EditorGUILayout.EndVertical();
    }

    private void DrawBasicInfo()
    {
        foreach (var prop in _basicProperties)
        {
            EditorGUILayout.PropertyField(prop);
        }
    }

    private void DrawModelSetup()
    {
        foreach (var prop in _modelProperties)
        {
            EditorGUILayout.PropertyField(prop);
        }
        
        // Validation
        var weaponData = target as WeaponDataSO;
        if (weaponData.rightHandModel == null && weaponData.leftHandModel == null)
        {
            EditorGUILayout.HelpBox("At least one hand model should be assigned.", MessageType.Warning);
        }
    }

    private void DrawTransformSettings()
    {
        foreach (var prop in _transformProperties)
        {
            EditorGUILayout.PropertyField(prop);
        }
    }

    private void DrawInputBindings()
    {
        if (_bindingsList == null)
        {
            EditorGUILayout.HelpBox("Bindings list not initialized!", MessageType.Error);
            return;
        }

        _bindingsList.DoLayoutList();
        
        // Summary
        DrawBindingsSummary();
    }

    private void DrawBindingsSummary()
    {
        if (_bindingsProperty.arraySize == 0) return;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Bindings Summary", EditorStyles.boldLabel);
        
        var summaryStyle = new GUIStyle(EditorStyles.helpBox) {
            fontSize = 11
        };
        
        System.Text.StringBuilder summary = new();
        for (int i = 0; i < _bindingsProperty.arraySize; i++)
        {
            var binding = _bindingsProperty.GetArrayElementAtIndex(i);
            var mode = (BindingMode)binding.FindPropertyRelative("bindingMode").enumValueIndex;
            var hand = (Hand)binding.FindPropertyRelative("hand").enumValueIndex;
            var actionCount = binding.FindPropertyRelative("bindings").arraySize;
            
            summary.AppendLine($"• {mode} ({hand}): {actionCount} actions");
        }
        
        EditorGUILayout.LabelField(summary.ToString(), summaryStyle);
    }

    private void DrawPreview()
    {
        var weaponData = target as WeaponDataSO;
        
        // Validation status
        bool isValid = ValidateWeaponData(weaponData, out var issues);
        
        var statusColor = isValid ? Color.green : Color.yellow;
        var statusText = isValid ? "✓ Valid Configuration" : $"⚠ {issues.Count} Issues Found";
        
        var statusStyle = new GUIStyle(EditorStyles.label) {
            normal = { textColor = statusColor },
            fontStyle = FontStyle.Bold
        };
        
        EditorGUILayout.LabelField(statusText, statusStyle);
        
        if (!isValid)
        {
            foreach (var issue in issues)
            {
                EditorGUILayout.HelpBox(issue, MessageType.Warning);
            }
        }
        
        // Quick stats
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Quick Stats:", EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"Total Bindings: {weaponData.inputBindings?.Count ?? 0}");
        
        int totalActions = 0;
        if (weaponData.inputBindings != null)
        {
            foreach (var binding in weaponData.inputBindings)
            {
                totalActions += binding.bindings?.Count ?? 0;
            }
        }
        
        EditorGUILayout.LabelField($"Total Actions: {totalActions}");
    }

    private bool ValidateWeaponData(WeaponDataSO weaponData, out List<string> issues)
    {
        issues = new List<string>();
        
        if (string.IsNullOrEmpty(weaponData.weaponName))
            issues.Add("Weapon name is empty");
            
        if (weaponData.rightHandModel == null && weaponData.leftHandModel == null)
            issues.Add("No hand models assigned");
            
        if (weaponData.inputBindings == null || weaponData.inputBindings.Count == 0)
            issues.Add("No input bindings configured");
        else
        {
            for (int i = 0; i < weaponData.inputBindings.Count; i++)
            {
                var binding = weaponData.inputBindings[i];
                if (binding.actionRef == null)
                    issues.Add($"Binding {i + 1}: No input action assigned");
                    
                if (binding.bindings == null || binding.bindings.Count == 0)
                    issues.Add($"Binding {i + 1}: No weapon actions configured");
            }
        }
        
        return issues.Count == 0;
    }
}