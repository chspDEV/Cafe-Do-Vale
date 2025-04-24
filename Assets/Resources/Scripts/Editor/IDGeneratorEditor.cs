// EntityEditorStyles.cs
#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using Tcp4.Resources.Scripts.Core;
using UnityEngine;
using UnityEditor;

namespace Tcp4.Resources.Scripts.Editor
{
    public static class EntityEditorStyles
    {
        private static GUIStyle headerStyle;
        private static GUIStyle entityStyle;
        private static GUIStyle toolbarStyle;
        private static Color selectedColor;
        
        public static GUIStyle HeaderStyle => headerStyle ??= CreateHeaderStyle();
        public static GUIStyle EntityStyle => entityStyle ??= CreateEntityStyle();
        public static GUIStyle ToolbarStyle => toolbarStyle ??= CreateToolbarStyle();
        public static Color SelectedColor => selectedColor == default ? selectedColor = new Color(0.6f, 0.8f, 1f, 0.5f) : selectedColor;

        private static GUIStyle CreateHeaderStyle()
        {
            var style = new GUIStyle
            {
                normal = { background = EditorUtils.MakeTexture(1, 1, new Color(0.4f, 0.4f, 0.4f)), textColor = Color.white },
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(10, 10, 8, 8)
            };
            return style;
        }

        private static GUIStyle CreateEntityStyle()
        {
            var style = new GUIStyle(EditorStyles.helpBox)
            {
                padding = new RectOffset(10, 10, 5, 5),
                margin = new RectOffset(5, 5, 2, 2)
            };
            return style;
        }

        private static GUIStyle CreateToolbarStyle()
        {
            var style = new GUIStyle(EditorStyles.toolbar)
            {
                fixedHeight = 30
            };
            return style;
        }
    }
    
// EditorUtils.cs
    public static class EditorUtils
    {
        public static Texture2D MakeTexture(int width, int height, Color color)
        {
            var pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = color;
                
            var texture = new Texture2D(width, height);
            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }

        public static void CreateNewGenerator(System.Action<IDGenerator> onGeneratorCreated)
        {
            string path = EditorUtility.SaveFilePanelInProject(
                "Criar ID Generator",
                "IDGenerator",
                "asset",
                "Escolha onde salvar o ID Generator"
            );

            if (!string.IsNullOrEmpty(path))
            {
                var newGenerator = ScriptableObject.CreateInstance<IDGenerator>();
                AssetDatabase.CreateAsset(newGenerator, path);
                AssetDatabase.SaveAssets();
                onGeneratorCreated?.Invoke(newGenerator);
            }
        }

        public static void ScanForEntities(IDGenerator generator)
        {
            if (generator == null) return;

            string[] guids = AssetDatabase.FindAssets("t:BaseEntitySO");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var entity = AssetDatabase.LoadAssetAtPath<BaseEntitySO>(path);
                if (entity != null)
                {
                    generator.GenerateID(entity);
                }
            }

            EditorUtility.SetDirty(generator);
        }
    }
    
// EntityListView.cs
  public class EntityListView
    {
        private Vector2 scrollPosition;
        private readonly List<BaseEntitySO> selectedEntities = new List<BaseEntitySO>();
        private BaseEntitySO currentlySelectedEntity;
        
        public BaseEntitySO CurrentEntity => currentlySelectedEntity;
        public event System.Action OnSelectionChanged;

        public void DrawList(IDGenerator generator, string searchQuery)
        {
            DrawHeader();
            DrawEntities(generator, searchQuery);
        }

        private void DrawHeader()
        {
            EditorGUILayout.BeginHorizontal(EntityEditorStyles.HeaderStyle);
            EditorGUILayout.LabelField("ID", GUILayout.Width(50));
            EditorGUILayout.LabelField("Nome", GUILayout.ExpandWidth(true));
            EditorGUILayout.LabelField("Tipo", GUILayout.Width(100));
            EditorGUILayout.EndHorizontal();
        }

        private void DrawEntities(IDGenerator generator, string searchQuery)
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            if (generator.AllEntities != null)
            {
                var filteredEntities = generator.AllEntities
                    .Where(e => e != null && (string.IsNullOrEmpty(searchQuery) ||
                                            e.name.ToLower().Contains(searchQuery.ToLower()) ||
                                            e.Id.ToString().Contains(searchQuery)))
                    .OrderBy(e => e.Id);

                foreach (var entity in filteredEntities)
                {
                    DrawEntityItem(entity);
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawEntityItem(BaseEntitySO entity)
        {
            Color originalColor = GUI.backgroundColor;
            if (selectedEntities.Contains(entity))
            {
                GUI.backgroundColor = EntityEditorStyles.SelectedColor;
            }

            EditorGUILayout.BeginHorizontal(EntityEditorStyles.EntityStyle);
            
            EditorGUILayout.LabelField(entity.Id.ToString(), GUILayout.Width(50));
            EditorGUILayout.LabelField(entity.name, GUILayout.ExpandWidth(true));
            EditorGUILayout.LabelField(entity.GetType().Name, GUILayout.Width(100));
            
            if (GUILayout.Button("Ping", GUILayout.Width(40)))
            {
                EditorGUIUtility.PingObject(entity);
            }
            
            EditorGUILayout.EndHorizontal();

            GUI.backgroundColor = originalColor;
            HandleSelection(entity);
        }

        private void HandleSelection(BaseEntitySO entity)
        {
            Rect lastRect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.MouseDown && lastRect.Contains(Event.current.mousePosition))
            {
                if (Event.current.control)
                {
                    if (selectedEntities.Contains(entity))
                        selectedEntities.Remove(entity);
                    else
                        selectedEntities.Add(entity);
                }
                else
                {
                    selectedEntities.Clear();
                    selectedEntities.Add(entity);
                }
                currentlySelectedEntity = entity;
                OnSelectionChanged?.Invoke();
                Event.current.Use();
            }
        }
    }
  
// EntityInspectorView.cs
    public class EntityInspectorView
    {
        public void Draw(BaseEntitySO entity)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.LabelField("Inspetor", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            if (entity != null)
            {
                DrawEntityInspector(entity);
            }
            else
            {
                EditorGUILayout.HelpBox("Selecione uma entidade para ver suas propriedades.", MessageType.Info);
            }
            
            EditorGUILayout.EndVertical();
        }

        private void DrawEntityInspector(BaseEntitySO entity)
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField("Entidade", entity, typeof(BaseEntitySO), false);
            EditorGUILayout.IntField("ID", entity.Id);
            EditorGUI.EndDisabledGroup();
            
            EditorGUILayout.Space();
            
            if (GUILayout.Button("Editar Entidade"))
            {
                Selection.activeObject = entity;
            }
        }
    }
    
// IDGeneratorEditor.cs
    public class IDGeneratorEditor : EditorWindow
    {
        private IDGenerator idGenerator;
        private string searchQuery = "";
        private bool showInspector = true;
        
        private EntityListView entityListView;
        private EntityInspectorView inspectorView;

        [MenuItem("Tools/ID Generator Editor")]
        public static void ShowWindow()
        {
            var window = GetWindow<IDGeneratorEditor>("ID Manager");
            window.minSize = new Vector2(600, 400);
        }

        private void OnEnable()
        {
            entityListView = new EntityListView();
            inspectorView = new EntityInspectorView();
            entityListView.OnSelectionChanged += Repaint;
            FindIDGenerator();
        }

        private void FindIDGenerator()
        {
            if (idGenerator != null) return;

            string[] guids = AssetDatabase.FindAssets("t:IDGenerator");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                idGenerator = AssetDatabase.LoadAssetAtPath<IDGenerator>(path);
            }
        }

        private void OnGUI()
        {
            if (idGenerator == null)
            {
                DrawNoGeneratorGUI();
                return;
            }

            EditorGUI.BeginChangeCheck();
            DrawToolbar();
            EditorGUILayout.Space();
            DrawMainContent();

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(idGenerator);
            }
        }

        private void DrawNoGeneratorGUI()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.HelpBox("Nenhum ID Generator encontrado!", MessageType.Warning);
            EditorGUILayout.Space(10);
            
            if (GUILayout.Button("Criar Novo ID Generator", GUILayout.Height(30)))
            {
                EditorUtils.CreateNewGenerator(generator => idGenerator = generator);
            }
            
            if (GUILayout.Button("Procurar ID Generator Existente", GUILayout.Height(30)))
            {
                FindIDGenerator();
            }
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EntityEditorStyles.ToolbarStyle);
            
            if (GUILayout.Button("Atualizar Lista", EditorStyles.toolbarButton, GUILayout.Width(100)))
            {
                EditorUtils.ScanForEntities(idGenerator);
            }
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("Selecionar Generator", EditorStyles.toolbarButton, GUILayout.Width(120)))
            {
                SelectGenerator();
            }
            
            GUILayout.FlexibleSpace();
            
            showInspector = GUILayout.Toggle(showInspector, "Mostrar Inspetor", EditorStyles.toolbarButton, GUILayout.Width(110));
            
            EditorGUILayout.EndHorizontal();

            DrawSearchBar();
        }

        private void DrawSearchBar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label("Buscar:", GUILayout.Width(50));
            searchQuery = EditorGUILayout.TextField(searchQuery, GUILayout.ExpandWidth(true));
            if (GUILayout.Button("×", EditorStyles.toolbarButton, GUILayout.Width(24)))
            {
                searchQuery = "";
                GUI.FocusControl(null);
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawMainContent()
        {
            EditorGUILayout.BeginHorizontal();
            
            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
            entityListView.DrawList(idGenerator, searchQuery);
            EditorGUILayout.EndVertical();

            if (showInspector)
            {
                EditorGUILayout.BeginVertical(GUILayout.Width(300));
                inspectorView.Draw(entityListView.CurrentEntity);
                EditorGUILayout.EndVertical();
            }
            
            EditorGUILayout.EndHorizontal();
        }

        private void SelectGenerator()
        {
            var generator = EditorUtility.OpenFilePanel("Selecionar ID Generator", "Assets", "asset");
            if (!string.IsNullOrEmpty(generator))
            {
                generator = "Assets" + generator.Substring(Application.dataPath.Length);
                idGenerator = AssetDatabase.LoadAssetAtPath<IDGenerator>(generator);
            }
        }
    }
}
#endif