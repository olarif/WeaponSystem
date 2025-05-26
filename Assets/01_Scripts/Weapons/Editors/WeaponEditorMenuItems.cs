using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
 
public static class WeaponEditorUtilities
{
    // Color palette for consistent theming
    public static class Colors
    {
        public static readonly Color Primary = new(0.2f, 0.6f, 0.9f, 1f);
        public static readonly Color Success = new(0.3f, 0.8f, 0.3f, 1f);
        public static readonly Color Warning = new(0.9f, 0.7f, 0.2f, 1f);
        public static readonly Color Error = new(0.9f, 0.3f, 0.3f, 1f);
        public static readonly Color Info = new(0.5f, 0.7f, 0.9f, 1f);
        
        public static readonly Color BackgroundLight = new(1f, 1f, 1f, 0.1f);
        public static readonly Color BackgroundDark = new(0f, 0f, 0f, 0.1f);
        public static readonly Color Accent = new(0.8f, 0.4f, 0.8f, 0.3f);
    }

    // Consistent spacing values
    public static class Spacing
    {
        public const float Small = 4f;
        public const float Medium = 8f;
        public const float Large = 12f;
        public const float Section = 16f;
    }

    // Common GUIStyle cache
    private static Dictionary<string, GUIStyle> _styleCache = new();

    public static GUIStyle GetStyle(string styleName)
    {
        if (_styleCache.TryGetValue(styleName, out var cachedStyle))
            return cachedStyle;

        GUIStyle style = styleName switch
        {
            "SectionHeader" => new GUIStyle(EditorStyles.boldLabel) {
                fontSize = 14,
                padding = new RectOffset(8, 8, 4, 4)
            },
            "SubHeader" => new GUIStyle(EditorStyles.label) {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                padding = new RectOffset(4, 4, 2, 2)
            },
            "InfoBox" => new GUIStyle(EditorStyles.helpBox) {
                fontSize = 11,
                padding = new RectOffset(8, 8, 6, 6),
                margin = new RectOffset(4, 4, 4, 4)
            },
            "CenteredBold" => new GUIStyle(EditorStyles.boldLabel) {
                alignment = TextAnchor.MiddleCenter
            },
            "RichText" => new GUIStyle(EditorStyles.label) {
                richText = true
            },
            _ => new GUIStyle(EditorStyles.label)
        };

        _styleCache[styleName] = style;
        return style;
    }

    // Drawing helpers
    public static void DrawColoredRect(Rect rect, Color color)
    {
        var oldColor = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(rect, EditorGUIUtility.whiteTexture);
        GUI.color = oldColor;
    }

    public static void DrawSectionBackground(Rect rect, Color color)
    {
        DrawColoredRect(rect, color);
        
        // Add subtle border
        var borderRect = new Rect(rect.x, rect.y, rect.width, 1f);
        DrawColoredRect(borderRect, new Color(color.r, color.g, color.b, color.a * 2f));
    }

    public static bool DrawFoldoutHeader(Rect rect, bool expanded, string title, GUIStyle style = null)
    {
        style ??= EditorStyles.foldoutHeader;
        return EditorGUI.Foldout(rect, expanded, title, true, style);
    }

    // Validation helpers
    public static void DrawValidationResults(List<string> issues, List<string> warnings = null)
    {
        if (issues?.Count > 0)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Issues Found:", GetStyle("SubHeader"));
            
            foreach (var issue in issues)
            {
                EditorGUILayout.HelpBox(issue, MessageType.Error);
            }
        }

        if (warnings?.Count > 0)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Warnings:", GetStyle("SubHeader"));
            
            foreach (var warning in warnings)
            {
                EditorGUILayout.HelpBox(warning, MessageType.Warning);
            }
        }
    }

    // Property field with enhanced styling
    public static void DrawEnhancedPropertyField(SerializedProperty property, GUIContent label = null, bool includeChildren = true)
    {
        label ??= new GUIContent(property.displayName, property.tooltip);
        
        // Add validation indicator if needed
        if (IsPropertyRequired(property) && IsPropertyEmpty(property))
        {
            var originalColor = GUI.contentColor;
            GUI.contentColor = Colors.Warning;
            EditorGUILayout.PropertyField(property, label, includeChildren);
            GUI.contentColor = originalColor;
        }
        else
        {
            EditorGUILayout.PropertyField(property, label, includeChildren);
        }
    }

    private static bool IsPropertyRequired(SerializedProperty property)
    {
        // Check for common required property names
        string[] requiredProperties = { "weaponName", "actionRef", "action" };
        return requiredProperties.Contains(property.name);
    }

    private static bool IsPropertyEmpty(SerializedProperty property)
    {
        return property.propertyType switch
        {
            SerializedPropertyType.String => string.IsNullOrEmpty(property.stringValue),
            SerializedPropertyType.ObjectReference => property.objectReferenceValue == null,
            SerializedPropertyType.ManagedReference => property.managedReferenceValue == null,
            _ => false
        };
    }

    // Icon helpers
    public static Texture2D GetIcon(string iconName)
    {
        return EditorGUIUtility.IconContent(iconName).image as Texture2D;
    }

    // Layout helpers
    public static void BeginBoxGroup(string title = null)
    {
        if (!string.IsNullOrEmpty(title))
        {
            EditorGUILayout.LabelField(title, GetStyle("SubHeader"));
        }
        EditorGUILayout.BeginVertical("box");
    }

    public static void EndBoxGroup()
    {
        EditorGUILayout.EndVertical();
    }

    // Debug helpers for development
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void DebugDrawRect(Rect rect, Color color, string label = "")
    {
        DrawColoredRect(rect, color);
        if (!string.IsNullOrEmpty(label))
        {
            var labelRect = new Rect(rect.x + 2, rect.y + 2, rect.width - 4, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(labelRect, label, EditorStyles.miniLabel);
        }
    }
}