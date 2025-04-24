// #if UNITY_EDITOR
// using System.Collections.Generic;
// using Tcp4.Resources.Scripts.Core;
// using UnityEngine;
// using UnityEditor;
//
// namespace Tcp4
// {
//     [CustomEditor(typeof(BaseEntitySO))]
//     public class BaseEntitySOEditor : Editor
//     {
//         private SerializedProperty nameProperty;
//         private SerializedProperty idProperty;
//         private SerializedProperty baseStatsProperty;
//         private SerializedProperty abilitySetProperty;
//
//         private int selectedStatIndex = -1;
//         private bool isAddingNewStat = false;
//         private StatusType newStatType;
//
//         private void OnEnable()
//         {
//             if (target == null)
//                 return;
//
//             nameProperty = serializedObject.FindProperty("Name");
//             idProperty = serializedObject.FindProperty("Id");
//             baseStatsProperty = serializedObject.FindProperty("baseStats");
//             abilitySetProperty = serializedObject.FindProperty("abilitySet");
//         }
//
//         public override void OnInspectorGUI()
//         {
//             if (target == null)
//             {
//                 EditorGUILayout.HelpBox("O objeto alvo � nulo. Por favor, selecione um objeto v�lido.", MessageType.Error);
//                 return;
//             }
//
//             serializedObject.Update();
//
//             EditorGUILayout.LabelField("Script: " + target.GetType().Name, EditorStyles.miniLabel);
//             EditorGUILayout.Space();
//
//             DrawNameField();
//             DrawIdField();
//             DrawBaseStatsTable();
//             DrawAbilitySet();
//
//             serializedObject.ApplyModifiedProperties();
//         }
//
//         private void DrawNameField()
//         {
//             EditorGUILayout.BeginVertical(EditorStyles.helpBox);
//             EditorGUILayout.LabelField("Nome da Entidade", EditorStyles.boldLabel);
//             EditorGUILayout.PropertyField(nameProperty, GUIContent.none);
//             EditorGUILayout.EndVertical();
//         }
//
//         private void DrawIdField()
//         {
//             EditorGUILayout.BeginVertical(EditorStyles.helpBox);
//             EditorGUILayout.LabelField("ID da Entidade", EditorStyles.boldLabel);
//             EditorGUILayout.PropertyField(idProperty);
//             EditorGUILayout.EndVertical();
//         }
//
//         private void DrawBaseStatsTable()
//         {
//             EditorGUILayout.Space();
//             EditorGUILayout.LabelField("Base Stats", EditorStyles.boldLabel);
//             EditorGUILayout.BeginVertical(EditorStyles.helpBox);
//
//             EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
//             GUILayout.Label("Status Type", EditorStyles.miniButtonLeft, GUILayout.Width(150));
//             GUILayout.Label("Valor", EditorStyles.miniButtonMid, GUILayout.Width(80));
//             GUILayout.Label("Gr�fico", EditorStyles.miniButtonRight);
//             EditorGUILayout.EndHorizontal();
//
//             if (baseStatsProperty != null && baseStatsProperty.isArray)
//             {
//                 for (int i = 0; i < baseStatsProperty.arraySize; i++)
//                 {
//                     DrawBaseStatRow(baseStatsProperty.GetArrayElementAtIndex(i), i);
//                 }
//             }
//
//             EditorGUILayout.BeginHorizontal();
//             if (GUILayout.Button("Adicionar Stat"))
//             {
//                 isAddingNewStat = true;
//                 newStatType = StatusType.Health;
//             }
//             if (GUILayout.Button("Remover Stat Selecionado") && selectedStatIndex >= 0 && selectedStatIndex < baseStatsProperty.arraySize)
//             {
//                 baseStatsProperty.DeleteArrayElementAtIndex(selectedStatIndex);
//                 selectedStatIndex = -1;
//             }
//             EditorGUILayout.EndHorizontal();
//
//             if (isAddingNewStat)
//             {
//                 DrawNewStatSelector();
//             }
//
//             EditorGUILayout.EndVertical();
//         }
//
//         private void DrawNewStatSelector()
//         {
//             EditorGUILayout.BeginHorizontal();
//             newStatType = (StatusType)EditorGUILayout.EnumPopup("Novo Status Type", newStatType);
//             if (GUILayout.Button("Confirmar", GUILayout.Width(100)))
//             {
//                 AddNewStat(newStatType);
//                 isAddingNewStat = false;
//             }
//             if (GUILayout.Button("Cancelar", GUILayout.Width(100)))
//             {
//                 isAddingNewStat = false;
//             }
//             EditorGUILayout.EndHorizontal();
//         }
//
//         private void AddNewStat(StatusType type)
//         {
//             baseStatsProperty.InsertArrayElementAtIndex(baseStatsProperty.arraySize);
//             var newElement = baseStatsProperty.GetArrayElementAtIndex(baseStatsProperty.arraySize - 1);
//             newElement.FindPropertyRelative("statusType").enumValueIndex = (int)type;
//             newElement.FindPropertyRelative("value").floatValue = 0f;
//         }
//
//         private void DrawBaseStatRow(SerializedProperty element, int index)
//         {
//             EditorGUILayout.BeginHorizontal();
//
//             SerializedProperty statusType = element.FindPropertyRelative("statusType");
//             SerializedProperty value = element.FindPropertyRelative("value");
//
//             bool isSelected = index == selectedStatIndex;
//             Color originalColor = GUI.color;
//             if (isSelected)
//             {
//                 GUI.color = Color.cyan;
//             }
//
//             if (GUILayout.Button(statusType.enumDisplayNames[statusType.enumValueIndex], GUILayout.Width(150)))
//             {
//                 selectedStatIndex = isSelected ? -1 : index;
//             }
//
//             GUI.color = originalColor;
//
//             EditorGUILayout.PropertyField(value, GUIContent.none, GUILayout.Width(80));
//
//             Rect progressRect = GUILayoutUtility.GetRect(0, EditorGUIUtility.singleLineHeight, GUILayout.ExpandWidth(true));
//             DrawStylizedProgressBar(progressRect, value);
//
//             EditorGUILayout.EndHorizontal();
//         }
//
//         private void DrawStylizedProgressBar(Rect rect, SerializedProperty valueProperty)
//         {
//             float value = valueProperty.floatValue;
//
//             EditorGUI.DrawRect(rect, new Color(0.1f, 0.1f, 0.1f));
//
//             Rect fillRect = new Rect(rect.x, rect.y, rect.width * (value / 100f), rect.height);
//
//             EditorGUI.DrawRect(fillRect, new Color(0.3f, 0.3f, 1.0f));
//
//             string valueText = value.ToString("F1") + "%";
//             GUIStyle centeredStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };
//             EditorGUI.LabelField(rect, valueText, centeredStyle);
//
//             EditorGUI.BeginChangeCheck();
//             float newValue = value;
//
//             Event e = Event.current;
//             if (rect.Contains(e.mousePosition))
//             {
//                 if (e.type == EventType.MouseDown || e.type == EventType.MouseDrag)
//                 {
//                     newValue = Mathf.Clamp((e.mousePosition.x - rect.x) / rect.width * 100f, 0f, 100f);
//                     GUI.changed = true;
//                 }
//             }
//
//             newValue = Mathf.Round(newValue / 5f) * 5f;
//
//             if (EditorGUI.EndChangeCheck())
//             {
//                 valueProperty.floatValue = newValue;
//             }
//
//             if (rect.Contains(e.mousePosition) || e.type == EventType.MouseDrag)
//             {
//                 EditorUtility.SetDirty(target);
//             }
//         }
//         private void DrawAbilitySet()
//         {
//             EditorGUILayout.BeginVertical(EditorStyles.helpBox);
//             EditorGUILayout.LabelField("Ability Set", EditorStyles.boldLabel);
//
//             if (abilitySetProperty != null)
//             {
//                 SerializedProperty abilities = abilitySetProperty.FindPropertyRelative("abilities");
//
//                 if (abilities != null && abilities.isArray)
//                 {
//                     for (int i = 0; i < abilities.arraySize; i++)
//                     {
//                         DrawAbilitySetRow(abilities.GetArrayElementAtIndex(i), i);
//                     }
//                 }
//
//                 if (GUILayout.Button("Adicionar Habilidade"))
//                 {
//                     abilities.InsertArrayElementAtIndex(abilities.arraySize);
//                 }
//             }
//
//             EditorGUILayout.EndVertical();
//         }
//
//
//         private void DrawAbilitySetRow(SerializedProperty abilityProperty, int index)
//         {
//             EditorGUILayout.BeginHorizontal();
//
//             SerializedProperty abilityType = abilityProperty.FindPropertyRelative("abilityType");
//             SerializedProperty isActive = abilityProperty.FindPropertyRelative("isActive");
//
//             EditorGUILayout.PropertyField(abilityType, GUIContent.none);
//             EditorGUILayout.PropertyField(isActive, GUIContent.none, GUILayout.Width(50));
//
//             if (GUILayout.Button("Remover", GUILayout.Width(75)))
//             {
//                 var abilities = abilitySetProperty.FindPropertyRelative("abilities");
//                 abilities.DeleteArrayElementAtIndex(index);
//             }
//
//             EditorGUILayout.EndHorizontal();
//         }
//
//     }
// }
//
// #endif