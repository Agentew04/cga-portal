using System;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using PortalGame;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;

/// <summary>
/// <see cref="PropertyDrawer"/> que mostra no inspetor
/// um tipo de dicionario customizado.
/// </summary>
[CustomPropertyDrawer(typeof(UnityDictionary<,>), true)]
public class UnityDictionaryPropertyDrawer : PropertyDrawer {

    private VisualElement container;

    private ListView itemsListView;
    private Toggle allowDuplicatesToggle;

    public override VisualElement CreatePropertyGUI(SerializedProperty property) {
        // cria a arvore
        var vsTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Scripts/Editor/UnityDictionaryTree.uxml");
        container = vsTree.CloneTree(property.propertyPath);

        // binda a checkbox
        allowDuplicatesToggle = container.Q<Toggle>("allowDuplicatesToggle");
        var duplicatesProperty = property.FindPropertyRelative("m_duplicatesAllowed");
        allowDuplicatesToggle.BindProperty(duplicatesProperty);

        // setar a list view
        itemsListView = container.Q<ListView>("itemsListView");
        var dictProperty = property.FindPropertyRelative("m_Dictionary");

        // inicializa a listview com os itens do dicionario
        itemsListView.itemsSource = GetKeyValuePairs(dictProperty);
        itemsListView.makeItem = () => new Label();
        itemsListView.bindItem = (element, i) => {
            var itemProperty = dictProperty.GetArrayElementAtIndex(i);
            var keyProperty = itemProperty.FindPropertyRelative("Key");
            var valueProperty = itemProperty.FindPropertyRelative("Value");

            // Display key-value pair in a flexible way based on type
            (element as Label).text = $"{GetPropertyDisplayName(keyProperty)} : {GetPropertyDisplayName(valueProperty)}";
        };

        itemsListView.


        return container;
    }

    private List<SerializedProperty> GetKeyValuePairs(SerializedProperty dictProperty) {
        List<SerializedProperty> keyValuePairs = new();
        for (int i = 0; i < dictProperty.arraySize; i++) {
            keyValuePairs.Add(dictProperty.GetArrayElementAtIndex(i));
        }
        return keyValuePairs;
    }

    private string GetPropertyDisplayName(SerializedProperty property) {
        switch (property.propertyType) {
            case SerializedPropertyType.Enum:
                return property.enumDisplayNames[property.enumValueIndex];
            case SerializedPropertyType.Integer:
                return property.intValue.ToString();
            case SerializedPropertyType.Float:
                return property.floatValue.ToString();
            case SerializedPropertyType.String:
                return property.stringValue;
            case SerializedPropertyType.ObjectReference:
                return property.objectReferenceValue ? property.objectReferenceValue.name : "None";
            default:
                return "Unsupported Type";
        }
    }

    //public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
    //    EditorGUI.BeginProperty(position, label, property);

    //    // Find the private field m_Dictionary using reflection
    //    var targetObject = fieldInfo.GetValue(property.serializedObject.targetObject);
    //    var dictionaryField = targetObject.GetType().GetField("m_Dictionary", BindingFlags.NonPublic | BindingFlags.Instance);

    //    // Draw the foldout
    //    property.isExpanded = EditorGUI.Foldout(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), property.isExpanded, label);

    //    if (property.isExpanded && dictionaryField.GetValue(targetObject) is System.Collections.IList dictionaryList) {
    //        EditorGUI.indentLevel++;

    //        // Iterate over each entry in the dictionary
    //        for (int i = 0; i < dictionaryList.Count; i++) {
    //            var element = dictionaryList[i];

    //            // Use reflection to get Key and Value fields
    //            var keyField = element.GetType().GetProperty("Key");
    //            var valueField = element.GetType().GetProperty("Value");

    //            var key = keyField.GetValue(element);
    //            var value = valueField.GetValue(element);

    //            var keyRect = new Rect(position.x, position.y + (i + 1) * EditorGUIUtility.singleLineHeight, position.width * 0.35f, EditorGUIUtility.singleLineHeight);
    //            var valueRect = new Rect(position.x + position.width * 0.4f, position.y + (i + 1) * EditorGUIUtility.singleLineHeight, position.width * 0.45f, EditorGUIUtility.singleLineHeight);
    //            var deleteButtonRect = new Rect(position.x + position.width * 0.9f, position.y + (i + 1) * EditorGUIUtility.singleLineHeight, position.width * 0.1f, EditorGUIUtility.singleLineHeight);

    //            // Display Key and Value using appropriate fields
    //            var newKey = EditorGUI.EnumPopup(keyRect, (Enum)key);
    //            var valueType = value?.GetType() ?? typeof(UnityEngine.Object); // Use Object if value is null
    //            var newValue = EditorGUI.ObjectField(valueRect, (UnityEngine.Object)value, valueType, true);
    //            // Set the new key and value
    //            keyField.SetValue(element, newKey);
    //            valueField.SetValue(element, newValue);

    //            if (GUI.Button(deleteButtonRect, "X")) {
    //                dictionaryList.RemoveAt(i);
    //                break; // Exit loop to avoid modifying list while iterating
    //            }
    //        }

    //        // Add button to add new dictionary entry
    //        if (GUI.Button(new Rect(position.x, position.y + (dictionaryList.Count + 1) * EditorGUIUtility.singleLineHeight, position.width, EditorGUIUtility.singleLineHeight), "Add Entry")) {
    //            var keyValuePairType = dictionaryField.FieldType.GenericTypeArguments[0]; // Get KeyValuePair type
    //            var newElement = Activator.CreateInstance(keyValuePairType);
    //            dictionaryList.Add(newElement);
    //        }

    //        EditorGUI.indentLevel--;
    //    }

    //    EditorGUI.EndProperty();
    //}

    //public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
    //    var targetObject = fieldInfo.GetValue(property.serializedObject.targetObject);
    //    var dictionaryField = targetObject.GetType().GetField("m_Dictionary", BindingFlags.NonPublic | BindingFlags.Instance);

    //    if (property.isExpanded && dictionaryField.GetValue(targetObject) is System.Collections.IList dictionaryList) {
    //        return (dictionaryList.Count + 2) * EditorGUIUtility.singleLineHeight;
    //    }
    //    return EditorGUIUtility.singleLineHeight;
    //}
}