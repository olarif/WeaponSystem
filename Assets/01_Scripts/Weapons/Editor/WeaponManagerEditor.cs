/*using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;


[CustomEditor(typeof(WeaponManager))]
public class WeaponManagerEditor : Editor
{
    private SerializedProperty _weaponsProperty;
    private SerializedProperty _currentWeaponProperty;
    private SerializedProperty _contextProperty;
    
    private ReorderableList _weaponsList;
    private bool _showDebugInfo = false;
    private Vector2 _scrollPosition;

    void OnEnable()
    {
        _weaponsProperty = serializedObject.FindProperty("availableWeapons");
        _currentWeaponProperty = serializedObject.FindProperty("currentWeaponIndex");
        _contextProperty = serializedObject.FindProperty("weaponContext");
        
        SetupWeaponsList();
    }

    private void SetupWeaponsList()
    {
        if (_weaponsProperty == null) return;

        _weaponsList = new ReorderableList(serializedObject, _weaponsProperty, true, true, true, true);
        
        _weaponsList.drawHeaderCallback = rect => {
            var headerRect = new Rect(rect.x, rect.y, rect.width - 80f, rect.height);
            EditorGUI.LabelField(headerRect, "Available Weapons", EditorStyles.boldLabel);
            
            var debugRect = new Rect(rect.xMax - 80f, rect.y, 80f, rect.height);
            _showDebugInfo = EditorGUI.Toggle(debugRect, "Debug", _showDebugInfo);
        };

        _weaponsList.drawElementCallback = (rect, index, active, focused) => {
            var element = _weaponsProperty.GetArrayElementAtIndex(index);
            var weapon = element.objectReferenceValue as WeaponDataSO;
            
            // Background for current weapon
            if (Application.isPlaying && index == _currentWeaponProperty.intValue)
            {
                EditorGUI.DrawRect(rect, new Color(0.2f, 0.8f, 0.2f, 0.2f));
            }
            
            // Weapon field
            var fieldRect = new Rect(rect.x, rect.y + 2f, rect.width - 60f, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(fieldRect, element, GUIContent.none);
            
            // Quick info
            if (weapon != null)
            {
                var infoRect = new Rect(rect.xMax - 55f, rect.y + 2f, 55f, EditorGUIUtility.singleLineHeight);
                int bindingCount = weapon.inputBindings?.Count ?? 0;
                EditorGUI.LabelField(infoRect, $"({bindingCount})", EditorStyles.centeredGreyMiniLabel);
            }
        };

        _weaponsList.elementHeight = EditorGUIUtility.singleLineHeight + 4f;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawHeader();
        
        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

        // Context reference
        EditorGUILayout.PropertyField(_contextProperty, new GUIContent("Weapon Context"));
        EditorGUILayout.Space();

        // Weapons list
        if (_weaponsList != null)
        {
            _weaponsList.DoLayoutList();
        }

        EditorGUILayout.Space();

        // Runtime controls
        if (Application.isPlaying)
        {
            DrawRuntimeControls();
        }

        // Debug information
        if (_showDebugInfo)
        {
            DrawDebugInfo();
        }

        EditorGUILayout.EndScrollView();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawHeader()
    {
        EditorGUILayout.Space();
        var headerStyle = new GUIStyle(EditorStyles.largeLabel) {
            fontSize = 16,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        
        EditorGUILayout.LabelField("🔫 Weapon Manager", headerStyle);
        EditorGUILayout.Space();
    }

    private void DrawRuntimeControls()
    {
        var manager = target as WeaponManager;
        if (manager == null) return;

        EditorGUILayout.LabelField("Runtime Controls", EditorStyles.boldLabel);
        
        EditorGUI.BeginDisabledGroup(!Application.isPlaying);
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("◀ Previous Weapon"))
        {
            manager.SwitchToPreviousWeapon();
        }
        
        if (GUILayout.Button("▶ Next Weapon"))
        {
            manager.SwitchToNextWeapon();
        }
        
        EditorGUILayout.EndHorizontal();

        // Direct weapon selection
        if (manager.availableWeapons != null && manager.availableWeapons.Count > 0)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Direct Selection:", EditorStyles.boldLabel);
            
            for (int i = 0; i < manager.availableWeapons.Count; i++)
            {
                var weapon = manager.availableWeapons[i];
                if (weapon == null) continue;
                
                EditorGUILayout.BeginHorizontal();
                
                bool isCurrent = i == manager.currentWeaponIndex;
                var buttonStyle = isCurrent ? EditorStyles.miniButtonMid : EditorStyles.miniButton;
                var buttonColor = isCurrent ? Color.green : Color.white;
                
                GUI.backgroundColor = buttonColor;
                if (GUILayout.Button($"{i + 1}. {weapon.weaponName}", buttonStyle))
                {
                    manager.SwitchToWeapon(i);
                }
                GUI.backgroundColor = Color.white;
                
                EditorGUILayout.EndHorizontal();
            }
        }
        
        EditorGUI.EndDisabledGroup();
    }

    private void DrawDebugInfo()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Debug Information", EditorStyles.boldLabel);
        
        var manager = target as WeaponManager;
        if (manager == null) return;

        EditorGUI.BeginDisabledGroup(true);
        
        EditorGUILayout.IntField("Current Weapon Index", manager.currentWeaponIndex);
        
        if (Application.isPlaying && manager.currentWeaponIndex >= 0 && 
            manager.availableWeapons != null && manager.currentWeaponIndex < manager.availableWeapons.Count)
        {
            var currentWeapon = manager.availableWeapons[manager.currentWeaponIndex];
            EditorGUILayout.ObjectField("Current Weapon Data", currentWeapon, typeof(WeaponDataSO), false);
        }

        EditorGUILayout.IntField("Total Weapons", manager.availableWeapons?.Count ?? 0);
        
        EditorGUI.EndDisabledGroup();

        // Validation
        List<string> issues = new();
        ValidateManager(manager, issues);
        
        if (issues.Count > 0)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Issues Found:", EditorStyles.boldLabel);
            foreach (var issue in issues)
            {
                EditorGUILayout.HelpBox(issue, MessageType.Warning);
            }
        }
    }

    private void ValidateManager(WeaponManager manager, List<string> issues)
    {
        if (manager.weaponContext == null)
            issues.Add("Weapon Context is not assigned");
            
        if (manager.availableWeapons == null || manager.availableWeapons.Count == 0)
            issues.Add("No weapons available in the manager");
        else
        {
            for (int i = 0; i < manager.availableWeapons.Count; i++)
            {
                if (manager.availableWeapons[i] == null)
                    issues.Add($"Weapon slot {i + 1} is empty");
            }
        }
    }
}*/