using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomPropertyDrawer(typeof(InputBindingData), true)]
public class InputBindingDataDrawer : PropertyDrawer
{
    private static readonly Dictionary<string, ReorderableList> _listsCache = new();
    private static List<Type> _actionTypes;
    private static readonly Dictionary<BindingMode, Color> _modeColors = new()
    {
        { BindingMode.Press, new Color(0.3f, 0.8f, 0.3f, 0.2f) },
        { BindingMode.Charge, new Color(0.8f, 0.6f, 0.2f, 0.2f) },
        { BindingMode.Continuous, new Color(0.2f, 0.6f, 0.8f, 0.2f) },
        { BindingMode.Release, new Color(0.8f, 0.3f, 0.3f, 0.2f) }
    };

    private static readonly Dictionary<BindingMode, string> _modeDescriptions = new()
    {
        { BindingMode.Press, "Single trigger on button press" },
        { BindingMode.Charge, "Hold to charge, release to fire" },
        { BindingMode.Continuous, "Continuous action while held" },
        { BindingMode.Release, "Trigger on button release" }
    };

    private void EnsureActionTypes()
    {
        if (_actionTypes != null) return;
        _actionTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => typeof(IWeaponAction).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
            .OrderBy(t => t.Name)
            .ToList();
    }

    public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
    {
        float height = 0f;

        // Header with mode indicator
        height += EditorGUIUtility.singleLineHeight + 8f;

        if (!prop.isExpanded) return height;

        // Binding mode section
        height += EditorGUIUtility.singleLineHeight + 4f; // Mode selector
        height += 20f; // Description box

        // Input settings section
        height += EditorGUIUtility.singleLineHeight + 2f; // Section header
        height += EditorGUIUtility.singleLineHeight + 2f; // Input Action
        height += EditorGUIUtility.singleLineHeight + 2f; // Hand
        
        // Conditional fields based on mode
        var modeProp = prop.FindPropertyRelative("bindingMode");
        var mode = (BindingMode)modeProp.enumValueIndex;
        
        if (mode == BindingMode.Charge)
        {
            height += EditorGUIUtility.singleLineHeight + 2f; // Hold Time
        }
        
        height += EditorGUIUtility.singleLineHeight + 2f; // Cooldown

        // Actions section
        height += EditorGUIUtility.singleLineHeight + 4f; // Section header
        var actionsList = GetActionsList(prop);
        height += actionsList.GetHeight() + 8f;

        return height;
    }

    public override void OnGUI(Rect rect, SerializedProperty prop, GUIContent label)
    {
        EnsureActionTypes();
        
        var originalRect = rect;
        rect.height = EditorGUIUtility.singleLineHeight;
        
        // Get binding mode for styling
        var modeProp = prop.FindPropertyRelative("bindingMode");
        var mode = (BindingMode)modeProp.enumValueIndex;
        var modeColor = _modeColors.GetValueOrDefault(mode, Color.white);

        // Draw background
        var bgRect = new Rect(originalRect.x - 4, originalRect.y, originalRect.width + 8, GetPropertyHeight(prop, label));
        EditorGUI.DrawRect(bgRect, modeColor);
        
        // Draw border
        var borderRect = new Rect(bgRect.x, bgRect.y, bgRect.width, 2f);
        EditorGUI.DrawRect(borderRect, GetModeAccentColor(mode));

        float y = rect.y + 4f;

        // Enhanced header with icon and status
        DrawEnhancedHeader(ref y, rect, prop, mode);

        if (!prop.isExpanded) return;

        EditorGUI.indentLevel++;
        
        // Binding Mode Section
        DrawModeSection(ref y, rect, prop, mode);
        
        // Input Settings Section
        DrawInputSection(ref y, rect, prop, mode);
        
        // Actions Section
        DrawActionsSection(ref y, rect, prop);

        EditorGUI.indentLevel--;
    }

    private void DrawEnhancedHeader(ref float y, Rect rect, SerializedProperty prop, BindingMode mode)
    {
        var headerRect = new Rect(rect.x, y, rect.width, EditorGUIUtility.singleLineHeight);
        
        // Mode icon
        var iconRect = new Rect(headerRect.x, headerRect.y, 20f, headerRect.height);
        var icon = GetModeIcon(mode);
        if (icon != null)
        {
            GUI.Label(iconRect, new GUIContent(icon), EditorStyles.centeredGreyMiniLabel);
        }

        // Foldout with enhanced label
        var foldoutRect = new Rect(iconRect.xMax + 4f, headerRect.y, headerRect.width - 24f - 60f, headerRect.height);
        string headerText = $"{ObjectNames.NicifyVariableName(mode.ToString())} Binding";
        
        // Add validation indicator
        var inputActionProp = prop.FindPropertyRelative("actionRef");
        bool isValid = inputActionProp != null && inputActionProp.objectReferenceValue != null;
        string validationIcon = isValid ? "✓" : "⚠";
        headerText = $"{validationIcon} {headerText}";
        
        prop.isExpanded = EditorGUI.Foldout(foldoutRect, prop.isExpanded, headerText, true, EditorStyles.foldoutHeader);

        // Action count indicator
        var countRect = new Rect(headerRect.xMax - 60f, headerRect.y, 60f, headerRect.height);
        var bindingsProp = prop.FindPropertyRelative("bindings");
        int actionCount = bindingsProp?.arraySize ?? 0;
        GUI.Label(countRect, $"({actionCount})", EditorStyles.centeredGreyMiniLabel);

        y += EditorGUIUtility.singleLineHeight + 4f;
    }

    private void DrawModeSection(ref float y, Rect rect, SerializedProperty prop, BindingMode mode)
    {
        // Section header
        var sectionRect = new Rect(rect.x, y, rect.width, EditorGUIUtility.singleLineHeight);
        EditorGUI.LabelField(sectionRect, "Binding Mode", EditorStyles.boldLabel);
        y += EditorGUIUtility.singleLineHeight + 2f;

        // Mode selector
        var modeProp = prop.FindPropertyRelative("bindingMode");
        var fieldRect = new Rect(rect.x, y, rect.width, EditorGUIUtility.singleLineHeight);
        EditorGUI.PropertyField(fieldRect, modeProp, new GUIContent("Mode"));
        y += EditorGUIUtility.singleLineHeight + 2f;

        // Description box
        var descRect = new Rect(rect.x, y, rect.width, 16f);
        var description = _modeDescriptions.GetValueOrDefault(mode, "Unknown mode");
        EditorGUI.HelpBox(descRect, description, MessageType.Info);
        y += 20f;
    }

    private void DrawInputSection(ref float y, Rect rect, SerializedProperty prop, BindingMode mode)
    {
        // Section header
        var sectionRect = new Rect(rect.x, y, rect.width, EditorGUIUtility.singleLineHeight);
        EditorGUI.LabelField(sectionRect, "Input Settings", EditorStyles.boldLabel);
        y += EditorGUIUtility.singleLineHeight + 2f;

        // Input Action
        DrawPropertyField(ref y, rect, prop, "actionRef", "Input Action");

        // Hand selection
        DrawPropertyField(ref y, rect, prop, "hand", "Hand");

        // Conditional fields
        if (mode == BindingMode.Charge)
        {
            DrawPropertyField(ref y, rect, prop, "holdTime", "Hold Time (s)");
        }

        // Cooldown
        DrawPropertyField(ref y, rect, prop, "cooldown", "Cooldown (s)");
    }

    private void DrawActionsSection(ref float y, Rect rect, SerializedProperty prop)
    {
        // Section header
        var sectionRect = new Rect(rect.x, y, rect.width, EditorGUIUtility.singleLineHeight);
        EditorGUI.LabelField(sectionRect, "Weapon Actions", EditorStyles.boldLabel);
        y += EditorGUIUtility.singleLineHeight + 4f;

        // Actions list
        var actionsList = GetActionsList(prop);
        var listRect = new Rect(rect.x, y, rect.width, actionsList.GetHeight());
        actionsList.DoList(listRect);
    }

    private void DrawPropertyField(ref float y, Rect rect, SerializedProperty root, string propertyName, string label)
    {
        var prop = root.FindPropertyRelative(propertyName);
        var fieldRect = new Rect(rect.x, y, rect.width, EditorGUIUtility.singleLineHeight);
        EditorGUI.PropertyField(fieldRect, prop, new GUIContent(label));
        y += EditorGUIUtility.singleLineHeight + 2f;
    }

    private ReorderableList GetActionsList(SerializedProperty bindingProp)
    {
        string key = $"{bindingProp.serializedObject.targetObject.GetInstanceID()}_{bindingProp.propertyPath}";
        
        if (_listsCache.TryGetValue(key, out var existingList) && 
            existingList.serializedProperty == bindingProp.FindPropertyRelative("bindings"))
        {
            return existingList;
        }

        var actionsProp = bindingProp.FindPropertyRelative("bindings");
        var list = new ReorderableList(actionsProp.serializedObject, actionsProp, true, false, true, true);

        // Enhanced drawing
        list.drawElementCallback = (rect, idx, active, focused) => DrawActionElement(rect, actionsProp, idx, active);
        list.elementHeightCallback = idx => GetActionElementHeight(actionsProp, idx);
        list.onAddDropdownCallback = (buttonRect, roList) => ShowActionTypeMenu(buttonRect, actionsProp);
        list.onRemoveCallback = roList => RemoveActionElement(actionsProp, roList.index);

        _listsCache[key] = list;
        return list;
    }

    private void DrawActionElement(Rect rect, SerializedProperty actionsProp, int index, bool isActive)
    {
        if (index >= actionsProp.arraySize) return;

        var element = actionsProp.GetArrayElementAtIndex(index);
        var actionProp = element.FindPropertyRelative("action");
        var phaseProp = element.FindPropertyRelative("triggerPhase");
        
        float y = rect.y + 2f;
        
        // Background for active element
        if (isActive)
        {
            var bgRect = new Rect(rect.x - 2, rect.y, rect.width + 4, rect.height);
            EditorGUI.DrawRect(bgRect, new Color(0.3f, 0.5f, 0.8f, 0.1f));
        }

        // Action type header with phase indicator
        var phase = (TriggerPhase)phaseProp.enumValueIndex;
        string actionType = GetNiceManagedTypeName(actionProp);
        string phaseColor = GetPhaseColor(phase);
        string headerText = $"<color={phaseColor}>●</color> {actionType} ({ObjectNames.NicifyVariableName(phase.ToString())})";
        
        var headerStyle = new GUIStyle(EditorStyles.foldout) { richText = true };
        var headerRect = new Rect(rect.x, y, rect.width, EditorGUIUtility.singleLineHeight);
        element.isExpanded = EditorGUI.Foldout(headerRect, element.isExpanded, headerText, true, headerStyle);
        y += EditorGUIUtility.singleLineHeight + 2f;

        if (!element.isExpanded) return;

        EditorGUI.indentLevel++;

        // Trigger Phase
        var phaseRect = new Rect(rect.x, y, rect.width, EditorGUIUtility.singleLineHeight);
        EditorGUI.PropertyField(phaseRect, phaseProp, new GUIContent("Trigger Phase"));
        y += EditorGUIUtility.singleLineHeight + 2f;

        // Tick Rate (conditional)
        if (phase == TriggerPhase.OnTick)
        {
            var tickRateProp = element.FindPropertyRelative("tickRate");
            var tickRect = new Rect(rect.x, y, rect.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(tickRect, tickRateProp, new GUIContent("Tick Rate (s)"));
            y += EditorGUIUtility.singleLineHeight + 2f;
        }

        // Action properties
        if (actionProp.managedReferenceValue != null)
        {
            var actionRect = new Rect(rect.x, y, rect.width, EditorGUI.GetPropertyHeight(actionProp, true));
            EditorGUI.PropertyField(actionRect, actionProp, new GUIContent("Action Settings"), true);
        }

        EditorGUI.indentLevel--;
    }

    private float GetActionElementHeight(SerializedProperty actionsProp, int index)
    {
        if (index >= actionsProp.arraySize) return EditorGUIUtility.singleLineHeight;

        var element = actionsProp.GetArrayElementAtIndex(index);
        float height = EditorGUIUtility.singleLineHeight + 4f; // Header

        if (!element.isExpanded) return height;

        height += EditorGUIUtility.singleLineHeight + 2f; // Trigger Phase

        var phaseProp = element.FindPropertyRelative("triggerPhase");
        if ((TriggerPhase)phaseProp.enumValueIndex == TriggerPhase.OnTick)
        {
            height += EditorGUIUtility.singleLineHeight + 2f; // Tick Rate
        }

        // Action properties
        var actionProp = element.FindPropertyRelative("action");
        if (actionProp.managedReferenceValue != null)
        {
            height += EditorGUI.GetPropertyHeight(actionProp, true) + 4f;
        }

        return height;
    }

    private void ShowActionTypeMenu(Rect buttonRect, SerializedProperty actionsProp)
    {
        var menu = new GenericMenu();
        
        foreach (var type in _actionTypes)
        {
            string categoryName = GetActionCategory(type);
            string displayName = $"{categoryName}/{ObjectNames.NicifyVariableName(type.Name)}";
            
            menu.AddItem(new GUIContent(displayName), false, () => AddNewAction(actionsProp, type));
        }
        
        menu.DropDown(buttonRect);
    }

    private void AddNewAction(SerializedProperty actionsProp, Type actionType)
    {
        int newIndex = actionsProp.arraySize;
        actionsProp.arraySize++;
    
        // Force immediate serialization to create the array slot
        actionsProp.serializedObject.ApplyModifiedProperties();
        actionsProp.serializedObject.Update();

        var newElement = actionsProp.GetArrayElementAtIndex(newIndex);
        var actionProp = newElement.FindPropertyRelative("action");
        var phaseProp = newElement.FindPropertyRelative("triggerPhase");
        var tickRateProp = newElement.FindPropertyRelative("tickRate");

        // Force Unity to treat this as a completely new managed reference
        // by clearing first and using a unique reference ID
        actionProp.managedReferenceValue = null;
        actionsProp.serializedObject.ApplyModifiedProperties();
        actionsProp.serializedObject.Update();
    
        // Create new instance and force immediate serialization
        var newInstance = Activator.CreateInstance(actionType);
        actionProp.managedReferenceValue = newInstance;
    
        // Apply immediately after setting the instance
        actionsProp.serializedObject.ApplyModifiedProperties();
        actionsProp.serializedObject.Update();
    
        // Now set other properties
        phaseProp.enumValueIndex = (int)TriggerPhase.OnPerform;
        tickRateProp.floatValue = 0.1f;
        newElement.isExpanded = true;
    
        // Final application to ensure all changes are persisted
        actionsProp.serializedObject.ApplyModifiedProperties();
    
        // Force a refresh of the entire property to break any lingering references
        var rootProp = actionsProp.serializedObject.FindProperty(actionsProp.propertyPath.Split('.')[0]);
        rootProp.serializedObject.Update();
    }

    private void RemoveActionElement(SerializedProperty actionsProp, int index)
    {
        if (index >= 0 && index < actionsProp.arraySize)
        {
            actionsProp.DeleteArrayElementAtIndex(index);
            actionsProp.serializedObject.ApplyModifiedProperties();
        }
    }

    #region Utility Methods
    
    private static string GetNiceManagedTypeName(SerializedProperty prop)
    {
        if (prop?.managedReferenceValue == null) return "None";
        return ObjectNames.NicifyVariableName(prop.managedReferenceValue.GetType().Name);
    }

    private static Texture2D GetModeIcon(BindingMode mode)
    {
        return mode switch
        {
            BindingMode.Press => EditorGUIUtility.IconContent("Animation.Play").image as Texture2D,
            BindingMode.Charge => EditorGUIUtility.IconContent("Animation.Record").image as Texture2D,
            BindingMode.Continuous => EditorGUIUtility.IconContent("Animation.Record").image as Texture2D,
            BindingMode.Release => EditorGUIUtility.IconContent("Animation.NextKey").image as Texture2D,
            _ => null
        };
    }

    private static Color GetModeAccentColor(BindingMode mode)
    {
        return mode switch
        {
            BindingMode.Press => Color.green,
            BindingMode.Charge => Color.yellow,
            BindingMode.Continuous => Color.cyan,
            BindingMode.Release => Color.red,
            _ => Color.gray
        };
    }

    private static string GetPhaseColor(TriggerPhase phase)
    {
        return phase switch
        {
            TriggerPhase.OnStart => "#4CAF50",
            TriggerPhase.OnPerform => "#2196F3",
            TriggerPhase.OnCancel => "#FF9800",
            TriggerPhase.OnTick => "#9C27B0",
            _ => "#757575"
        };
    }

    private static string GetActionCategory(Type actionType)
    {
        // Simple categorization based on type name patterns
        string name = actionType.Name.ToLower();
        if (name.Contains("fire") || name.Contains("shoot") || name.Contains("projectile"))
            return "Combat";
        if (name.Contains("heal") || name.Contains("buff") || name.Contains("effect"))
            return "Support";
        if (name.Contains("move") || name.Contains("teleport") || name.Contains("dash"))
            return "Movement";
        if (name.Contains("ui") || name.Contains("hud") || name.Contains("display"))
            return "UI";
        return "Misc";
    }

    #endregion
}