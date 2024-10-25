using System;
using UnityEngine;
using UnityEditor;
using PortalGame.Models;
using System.Reflection;

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
        var dictionaryList = dictionaryField.GetValue(targetObject) as System.Collections.IList;

        // Draw the foldout
        property.isExpanded = EditorGUI.Foldout(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), property.isExpanded, label);

        if (property.isExpanded && dictionaryList != null) {
            EditorGUI.indentLevel++;

            // Iterate over each entry in the dictionary
            for (int i = 0; i < dictionaryList.Count; i++) {
                var element = dictionaryList[i];

                // Use reflection to get Key and Value fields
                var keyField = element.GetType().GetProperty("Key");
                var valueField = element.GetType().GetProperty("Value");

                var key = keyField.GetValue(element);
                var value = valueField.GetValue(element);

                var keyRect = new Rect(position.x, position.y + (i + 1) * EditorGUIUtility.singleLineHeight, position.width * 0.4f, EditorGUIUtility.singleLineHeight);
                var valueRect = new Rect(position.x + position.width * 0.45f, position.y + (i + 1) * EditorGUIUtility.singleLineHeight, position.width * 0.5f, EditorGUIUtility.singleLineHeight);

                // Display Key and Value using appropriate fields
                var newKey = EditorGUI.EnumPopup(keyRect, (Enum)key);
                var newValue = EditorGUI.ObjectField(valueRect, (UnityEngine.Object)value, value.GetType(), true);
                // Set the new key and value
                keyField.SetValue(element, newKey);
                valueField.SetValue(element, newValue);
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
        var dictionaryList = dictionaryField.GetValue(targetObject) as System.Collections.IList;

        if (property.isExpanded && dictionaryList != null) {
            return (dictionaryList.Count + 2) * EditorGUIUtility.singleLineHeight;
        }
        return EditorGUIUtility.singleLineHeight;
    }
}