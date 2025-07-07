using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(EnemyCycleStep))]
public class EnemyCycleStepDrawer : PropertyDrawer
{
    private bool foldout = true;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing; // foldout

        if (!foldout) return height;

        // Header + ActionType + WaitTime 
        height += (EditorGUIUtility.singleLineHeight + 2) * 3;

        var actionType = (EnemyCycleActionType)property.FindPropertyRelative("actionType").enumValueIndex;

        switch (actionType)
        {
            case EnemyCycleActionType.Shoot:
                height += (EditorGUIUtility.singleLineHeight + 2) * 6; // header + 5 properties
                break;
            case EnemyCycleActionType.Move:
            case EnemyCycleActionType.PathToTile:
                height += (EditorGUIUtility.singleLineHeight + 2) * 3; // header + 1 property
                break;
        }

        return height + 6; // final padding
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty actionTypeProp = property.FindPropertyRelative("actionType");
        SerializedProperty waitTimeProp = property.FindPropertyRelative("waitTime");
        SerializedProperty projectilePrefabProp = property.FindPropertyRelative("projectilePrefab");
        SerializedProperty damageProp = property.FindPropertyRelative("damage");
        SerializedProperty speedProp = property.FindPropertyRelative("projectileSpeed");
        SerializedProperty rangeProp = property.FindPropertyRelative("range");
        SerializedProperty patternProp = property.FindPropertyRelative("pattern");
        SerializedProperty moveDirProp = property.FindPropertyRelative("moveDirection");
        SerializedProperty targetTileProp = property.FindPropertyRelative("targetTile");

        Rect rect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        foldout = EditorGUI.Foldout(rect, foldout, label, true);

        if (!foldout) return;

        EditorGUI.indentLevel++;
        rect.y += EditorGUIUtility.singleLineHeight + 2;

        // Always show ActionType
        EditorGUI.LabelField(rect, "Generic", EditorStyles.boldLabel);
        rect.y += EditorGUIUtility.singleLineHeight;
        EditorGUI.PropertyField(rect, actionTypeProp);
        rect.y += EditorGUIUtility.singleLineHeight + 2;
        EditorGUI.PropertyField(rect, waitTimeProp);
        rect.y += EditorGUIUtility.singleLineHeight + 4;

        EnemyCycleActionType type = (EnemyCycleActionType)actionTypeProp.enumValueIndex;

        switch (type)
        {
            case EnemyCycleActionType.Shoot:
                EditorGUI.LabelField(rect, "Shooting", EditorStyles.boldLabel);
                rect.y += EditorGUIUtility.singleLineHeight;
                EditorGUI.PropertyField(rect, projectilePrefabProp);
                rect.y += EditorGUIUtility.singleLineHeight + 2;
                EditorGUI.PropertyField(rect, damageProp);
                rect.y += EditorGUIUtility.singleLineHeight + 2;
                EditorGUI.PropertyField(rect, speedProp);
                rect.y += EditorGUIUtility.singleLineHeight + 2;
                EditorGUI.PropertyField(rect, rangeProp);
                rect.y += EditorGUIUtility.singleLineHeight + 2;
                EditorGUI.PropertyField(rect, patternProp);
                break;

            case EnemyCycleActionType.Move:
                EditorGUI.LabelField(rect, "Movement", EditorStyles.boldLabel);
                rect.y += EditorGUIUtility.singleLineHeight;
                EditorGUI.PropertyField(rect, moveDirProp);
                break;

            case EnemyCycleActionType.PathToTile:
                EditorGUI.LabelField(rect, "Pathfinding", EditorStyles.boldLabel);
                rect.y += EditorGUIUtility.singleLineHeight;
                EditorGUI.PropertyField(rect, targetTileProp);
                break;
        }

        EditorGUI.indentLevel--;

        EditorUtility.SetDirty(property.serializedObject.targetObject);
        SceneView.RepaintAll();
    }
}