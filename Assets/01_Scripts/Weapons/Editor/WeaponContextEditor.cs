using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(WeaponContext))]
public class WeaponContextEditor : Editor
{
    private SerializedProperty _playerProp;
    private SerializedProperty _leftHandProp;
    private SerializedProperty _rightHandProp;
    private SerializedProperty _cameraProp;
    private SerializedProperty _leftChargeUIProp;
    private SerializedProperty _rightChargeUIProp;
    private SerializedProperty _animatorProp;
    
    private bool _showRuntimeInfo = true;
    private Vector2 _scrollPosition;

    void OnEnable()
    {
        _playerProp = serializedObject.FindProperty("Player");
        _leftHandProp = serializedObject.FindProperty("leftHand");
        _rightHandProp = serializedObject.FindProperty("rightHand");
        _cameraProp = serializedObject.FindProperty("PlayerCamera");
        _leftChargeUIProp = serializedObject.FindProperty("leftChargeUI");
        _rightChargeUIProp = serializedObject.FindProperty("rightChargeUI");
        _animatorProp = serializedObject.FindProperty("Animator");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawHeader();
        
        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
        
        DrawSection("Core References", DrawCoreReferences);
        DrawSection("UI References", DrawUIReferences);
        
        // Runtime information
        if (Application.isPlaying && _showRuntimeInfo)
        {
            DrawSection("Runtime Information", DrawRuntimeInfo);
        }

        EditorGUILayout.EndScrollView();

        serializedObject.ApplyModifiedProperties();
    }

    private new void DrawHeader()
    {
        EditorGUILayout.Space();
        var headerStyle = new GUIStyle(EditorStyles.largeLabel) {
            fontSize = 16,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        
        EditorGUILayout.LabelField("🎯 Weapon Context", headerStyle);
        
        // Toggle for runtime info
        if (Application.isPlaying)
        {
            _showRuntimeInfo = EditorGUILayout.Toggle("Show Runtime Info", _showRuntimeInfo);
        }
        
        EditorGUILayout.Space();
    }

    private void DrawSection(string title, System.Action drawContent)
    {
        EditorGUILayout.Space(4f);
        
        var headerRect = GUILayoutUtility.GetRect(0, 20f, GUILayout.ExpandWidth(true));
        EditorGUI.DrawRect(headerRect, new Color(0.3f, 0.3f, 0.3f, 0.2f));
        
        var textRect = new Rect(headerRect.x + 8f, headerRect.y, headerRect.width - 16f, headerRect.height);
        EditorGUI.LabelField(textRect, title, EditorStyles.boldLabel);
        
        EditorGUILayout.BeginVertical("box");
        drawContent?.Invoke();
        EditorGUILayout.EndVertical();
    }

    private void DrawCoreReferences()
    {
        EditorGUILayout.PropertyField(_playerProp, new GUIContent("Player Controller"));
        
        EditorGUILayout.Space(4f);
        EditorGUILayout.PropertyField(_leftHandProp, new GUIContent("Left Hand Transform"));
        EditorGUILayout.PropertyField(_rightHandProp, new GUIContent("Right Hand Transform"));
        
        EditorGUILayout.Space(4f);
        EditorGUILayout.PropertyField(_cameraProp, new GUIContent("Player Camera"));
        EditorGUILayout.PropertyField(_animatorProp, new GUIContent("Animator"));

        // Validation
        var context = target as WeaponContext;
        List<string> issues = new();
        
        if (context.Player == null) issues.Add("Player Controller not assigned");
        if (context.leftHand == null) issues.Add("Left Hand Transform not assigned");
        if (context.rightHand == null) issues.Add("Right Hand Transform not assigned");
        if (context.PlayerCamera == null) issues.Add("Player Camera not assigned");
        
        if (issues.Count > 0)
        {
            EditorGUILayout.Space();
            foreach (var issue in issues)
            {
                EditorGUILayout.HelpBox(issue, MessageType.Warning);
            }
        }
    }

    private void DrawUIReferences()
    {
        EditorGUILayout.PropertyField(_leftChargeUIProp, new GUIContent("Left Hand Charge UI"));
        EditorGUILayout.PropertyField(_rightChargeUIProp, new GUIContent("Right Hand Charge UI"));
        
        // UI validation
        var context = target as WeaponContext;
        if (context.leftChargeUI == null || context.rightChargeUI == null)
        {
            EditorGUILayout.HelpBox("Charge UI elements are optional but recommended for charge-type weapons", MessageType.Info);
        }
    }

    private void DrawRuntimeInfo()
    {
        var context = target as WeaponContext;
        if (context == null) return;

        EditorGUI.BeginDisabledGroup(true);
        
        // Fire points
        EditorGUILayout.LabelField($"Active Fire Points: {context.FirePoints.Count}");
        if (context.FirePoints.Count > 0)
        {
            EditorGUI.indentLevel++;
            for (int i = 0; i < context.FirePoints.Count; i++)
            {
                var fp = context.FirePoints[i];
                EditorGUILayout.ObjectField($"Fire Point {i + 1}", fp, typeof(Transform), true);
            }
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space();
        
        // Active weapon controller
        EditorGUILayout.ObjectField("Active Weapon Controller", context.WeaponController, typeof(WeaponController), true);
        EditorGUILayout.ObjectField("Weapon Manager", context.WeaponManager, typeof(WeaponManager), true);
        
        EditorGUI.EndDisabledGroup();

        // Quick actions
        if (context.WeaponController != null)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Quick Actions:", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Log Fire Points"))
            {
                Debug.Log($"Fire Points ({context.FirePoints.Count}):");
                for (int i = 0; i < context.FirePoints.Count; i++)
                {
                    var fp = context.FirePoints[i];
                    Debug.Log($"  {i + 1}. {fp?.name} at {fp?.position}");
                }
            }
        }
    }
}