using System;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using PortalGame;

/// <summary>
/// <see cref="PropertyDrawer"/> que mostra no inspetor
/// um tipo de dicionario customizado.
/// </summary>
[CustomPropertyDrawer(typeof(UnityDictionary<,>))]

public class UnityDictionaryPropertyDrawer : PropertyDrawer {

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        EditorGUI.BeginProperty(position, label, property);

        // Find the private field m_Dictionary using reflection
        var targetObject = fieldInfo.GetValue(property.serializedObject.targetObject);
        var dictionaryField = targetObject.GetType().GetField("m_Dictionary", BindingFlags.NonPublic | BindingFlags.Instance);

        // Draw the foldout
        property.isExpanded = EditorGUI.Foldout(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), property.isExpanded, label);

        if (property.isExpanded && dictionaryField.GetValue(targetObject) is System.Collections.IList dictionaryList) {
            EditorGUI.indentLevel++;

            // Iterate over each entry in the dictionary
            for (int i = 0; i < dictionaryList.Count; i++) {
                var element = dictionaryList[i];

                // Use reflection to get Key and Value fields
                var keyField = element.GetType().GetProperty("Key");
                var valueField = element.GetType().GetProperty("Value");

                var key = keyField.GetValue(element);
                var value = valueField.GetValue(element);

                var keyRect = new Rect(position.x, position.y + (i + 1) * EditorGUIUtility.singleLineHeight, position.width * 0.35f, EditorGUIUtility.singleLineHeight);
                var valueRect = new Rect(position.x + position.width * 0.4f, position.y + (i + 1) * EditorGUIUtility.singleLineHeight, position.width * 0.45f, EditorGUIUtility.singleLineHeight);
                var deleteButtonRect = new Rect(position.x + position.width * 0.9f, position.y + (i + 1) * EditorGUIUtility.singleLineHeight, position.width * 0.1f, EditorGUIUtility.singleLineHeight);

                // Display Key and Value using appropriate fields
                var newKey = EditorGUI.EnumPopup(keyRect, (Enum)key);
                var valueType = value?.GetType() ?? typeof(UnityEngine.Object); // Use Object if value is null
                var newValue = EditorGUI.ObjectField(valueRect, (UnityEngine.Object)value, valueType, true);
                // Set the new key and value
                keyField.SetValue(element, newKey);
                valueField.SetValue(element, newValue);

                if (GUI.Button(deleteButtonRect, "X")) {
                    dictionaryList.RemoveAt(i);
                    break; // Exit loop to avoid modifying list while iterating
                }
            }

            // Add button to add new dictionary entry
            if (GUI.Button(new Rect(position.x, position.y + (dictionaryList.Count + 1) * EditorGUIUtility.singleLineHeight, position.width, EditorGUIUtility.singleLineHeight), "Add Entry")) {
                var keyValuePairType = dictionaryField.FieldType.GenericTypeArguments[0]; // Get KeyValuePair type
                var newElement = Activator.CreateInstance(keyValuePairType);
                dictionaryList.Add(newElement);
            }

            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        var targetObject = fieldInfo.GetValue(property.serializedObject.targetObject);
        var dictionaryField = targetObject.GetType().GetField("m_Dictionary", BindingFlags.NonPublic | BindingFlags.Instance);

        if (property.isExpanded && dictionaryField.GetValue(targetObject) is System.Collections.IList dictionaryList) {
            return (dictionaryList.Count + 2) * EditorGUIUtility.singleLineHeight;
        }
        return EditorGUIUtility.singleLineHeight;
    }
}