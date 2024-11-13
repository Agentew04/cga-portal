using PortalGame;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(UnityDictionary<,>.KeyValuePair), true)]
public class KeyValuePairDrawer : PropertyDrawer {

    private VisualElement container;

    public override VisualElement CreatePropertyGUI(SerializedProperty property) {
        // cria a arvore
        var vsTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Scripts/Editor/KeyValuePairVisualTree.uxml");
        container = vsTree.CloneTree(property.propertyPath);

        var keyProperty = property.FindPropertyRelative("key");
        var valueProperty = property.FindPropertyRelative("value");

        var keyfield = container.Q<PropertyField>("KeyField");
        keyfield.BindProperty(keyProperty);

        var valuefield = container.Q<PropertyField>("ValueField");
        valuefield.BindProperty(valueProperty);

        return container;
    }
}
