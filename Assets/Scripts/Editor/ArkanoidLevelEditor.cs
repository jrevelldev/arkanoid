#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;

namespace Arkanoid.Core
{
    [CustomEditor(typeof(ArkanoidLevel))]
    public class ArkanoidLevelEditor : UnityEditor.Editor
    {
        private static char activeTool = 'R'; // Default tool is Red brick
        
        // Supported tools palette
        private static readonly char[] PaletteChars = { '.', 'Y', 'G', 'B', 'O', 'I', 'R', 'S' };
        
        private static Color GetBrickColor(char c)
        {
            switch (c)
            {
                case 'Y': return new Color(1f, 0.92f, 0.016f); // Yellow
                case 'G': return new Color(0f, 1f, 0.2f);    // Green
                case 'B': return new Color(0f, 0.7f, 1f);    // Blue
                case 'O': return new Color(1f, 0.5f, 0f);    // Orange
                case 'I': return new Color(0.5f, 0f, 1f);    // Indigo
                case 'R': return new Color(1f, 0f, 0.4f);    // Red
                case 'S': return new Color(0.5f, 0.5f, 0.6f); // Steel
                default:  return new Color(0.12f, 0.12f, 0.16f); // Empty
            }
        }

        public override void OnInspectorGUI()
        {
            ArkanoidLevel level = (ArkanoidLevel)target;

            // Draw default properties
            serializedObject.Update();
            SerializedProperty levelNameProp = serializedObject.FindProperty("levelName");
            SerializedProperty rowsProp = serializedObject.FindProperty("rows");
            SerializedProperty colsProp = serializedObject.FindProperty("columns");
            SerializedProperty layoutProp = serializedObject.FindProperty("asciiLayout");

            EditorGUILayout.PropertyField(levelNameProp);
            
            // Constrain rows and columns to reasonable values (e.g., 1 to 30)
            int prevRows = rowsProp.intValue;
            int prevCols = colsProp.intValue;
            
            EditorGUILayout.PropertyField(rowsProp);
            EditorGUILayout.PropertyField(colsProp);
            
            int rows = Mathf.Clamp(rowsProp.intValue, 1, 30);
            int cols = Mathf.Clamp(colsProp.intValue, 1, 30);
            rowsProp.intValue = rows;
            colsProp.intValue = cols;
            
            serializedObject.ApplyModifiedProperties();

            // Parse grid from the string
            char[,] grid = ParseLayoutString(layoutProp.stringValue, rows, cols);

            // Handle dimension change
            if (rows != prevRows || cols != prevCols)
            {
                // Reformat the layout string based on the new dimensions
                string newLayout = FormatGridToString(grid, rows, cols);
                layoutProp.stringValue = newLayout;
                serializedObject.ApplyModifiedProperties();
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Level Designer - Paint Palette", EditorStyles.boldLabel);

            // Draw palette selector
            DrawPalette();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Level Grid Layout Editor", EditorStyles.boldLabel);

            // Draw the grid of buttons
            bool gridChanged = false;
            
            // Set style for buttons
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.fontStyle = FontStyle.Bold;
            buttonStyle.alignment = TextAnchor.MiddleCenter;

            // Save the original GUI background color
            Color originalBg = GUI.backgroundColor;

            // Draw the grid
            for (int r = 0; r < rows; r++)
            {
                EditorGUILayout.BeginHorizontal();
                for (int c = 0; c < cols; c++)
                {
                    char cellChar = grid[r, c];
                    Color cellColor = GetBrickColor(cellChar);
                    GUI.backgroundColor = cellColor;

                    // Text display inside button: make empty look clean
                    string label = cellChar == '.' ? " " : cellChar.ToString();

                    // Adjust button size to fit nicely
                    if (GUILayout.Button(label, buttonStyle, GUILayout.Width(25), GUILayout.Height(20)))
                    {
                        grid[r, c] = activeTool;
                        gridChanged = true;
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            
            // Restore GUI background color
            GUI.backgroundColor = originalBg;

            EditorGUILayout.Space();

            // Clear and Fill utility buttons
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Clear Grid (Empty)"))
            {
                FillGridWith(grid, rows, cols, '.');
                gridChanged = true;
            }
            if (GUILayout.Button($"Fill Grid ({activeTool})"))
            {
                FillGridWith(grid, rows, cols, activeTool);
                gridChanged = true;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            // Draw the raw text area below it
            EditorGUILayout.LabelField("ASCII Text Layout (Edit / Copy-Paste)", EditorStyles.boldLabel);
            
            // If the grid was changed via visual paint, update the string
            if (gridChanged)
            {
                string newLayout = FormatGridToString(grid, rows, cols);
                layoutProp.stringValue = newLayout;
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(level);
            }

            EditorGUI.BeginChangeCheck();
            string newRawLayout = EditorGUILayout.TextArea(layoutProp.stringValue, GUILayout.Height(150));
            if (EditorGUI.EndChangeCheck())
            {
                // Direct edit in the text area
                layoutProp.stringValue = newRawLayout;
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(level);
            }
        }

        private void DrawPalette()
        {
            // Set style for palette
            GUIStyle paletteStyle = new GUIStyle(GUI.skin.button);
            paletteStyle.fontStyle = FontStyle.Bold;

            Color originalBg = GUI.backgroundColor;

            // Draw palette buttons in a horizontal row
            EditorGUILayout.BeginHorizontal();
            for (int i = 0; i < PaletteChars.Length; i++)
            {
                char c = PaletteChars[i];
                Color color = GetBrickColor(c);
                GUI.backgroundColor = color;

                // Make active tool stand out (bullet prefix)
                string prefix = (c == activeTool) ? "● " : "  ";
                string label = prefix + c;

                if (GUILayout.Button(label, paletteStyle, GUILayout.Height(28)))
                {
                    activeTool = c;
                }
            }
            GUI.backgroundColor = originalBg;
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.LabelField($"Selected Tool: {activeTool}", EditorStyles.miniLabel);
        }

        private char[,] ParseLayoutString(string layout, int rows, int cols)
        {
            char[,] grid = new char[rows, cols];
            
            // Initialize with empty
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                    grid[r, c] = '.';

            if (string.IsNullOrEmpty(layout))
                return grid;

            string[] lines = layout.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            int rowsToParse = Math.Min(rows, lines.Length);

            for (int r = 0; r < rowsToParse; r++)
            {
                string line = lines[r].Trim();
                int colsToParse = Math.Min(cols, line.Length);
                for (int c = 0; c < colsToParse; c++)
                {
                    char cell = line[c];
                    // Check if it is a valid palette character
                    if (Array.IndexOf(PaletteChars, cell) >= 0)
                    {
                        grid[r, c] = cell;
                    }
                    else
                    {
                        grid[r, c] = '.'; // Fallback on invalid character
                    }
                }
            }

            return grid;
        }

        private string FormatGridToString(char[,] grid, int rows, int cols)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    sb.Append(grid[r, c]);
                }
                if (r < rows - 1)
                {
                    sb.Append("\n");
                }
            }
            return sb.ToString();
        }

        private void FillGridWith(char[,] grid, int rows, int cols, char fillChar)
        {
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    grid[r, c] = fillChar;
                }
            }
        }
    }
}
#endif
