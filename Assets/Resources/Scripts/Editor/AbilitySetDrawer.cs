// #if UNITY_EDITOR
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEditor;
// using ComponentUtils;
//
//
// namespace Tcp4
// {
//     [CustomPropertyDrawer(typeof(AbilitySet))]
//     public class AbilitySetDrawer : PropertyDrawer
//     {
//         public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
//         {
//             EditorGUI.BeginProperty(position, label, property);
//
//             SerializedProperty abilities = property.FindPropertyRelative("abilities");
//
//             EditorGUILayout.BeginVertical(EditorStyles.helpBox);
//             EditorGUILayout.LabelField("Abilities", EditorStyles.boldLabel);
//
//             if (abilities != null && abilities.isArray)
//             {
//                 for (int i = 0; i < abilities.arraySize; i++)
//                 {
//                     var ability = abilities.GetArrayElementAtIndex(i);
//                     var abilityType = ability.FindPropertyRelative("abilityType");
//                     var isActive = ability.FindPropertyRelative("isActive");
//
//                     EditorGUILayout.BeginHorizontal();
//                     EditorGUILayout.PropertyField(abilityType, GUIContent.none);
//                     EditorGUILayout.PropertyField(isActive, GUIContent.none, GUILayout.Width(50));
//                     EditorGUILayout.EndHorizontal();
//                 }
//             }
//
//             if (GUILayout.Button("Add Ability"))
//             {
//                 abilities.InsertArrayElementAtIndex(abilities.arraySize);
//             }
//
//             EditorGUILayout.EndVertical();
//
//             EditorGUI.EndProperty();
//         }
//
//         public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
//         {
//             SerializedProperty abilities = property.FindPropertyRelative("abilities");
//             return abilities.arraySize * EditorGUIUtility.singleLineHeight * 1.5f + 50f;
//         }
//     }
//
// }
//
// #endif