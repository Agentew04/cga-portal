using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PortalGame {
    public static class Extensions {
        
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, System.Action<T> action) {
            foreach (T element in source) {
                action(element);
            }
            return source;
        }

        public static void Toggle(this ref bool value) {
            value = !value;
        }

        public static (Vector2 min, Vector2 max) GetCanvasSpaceBounds(this RectTransform rectTransform, Canvas canvas) {
            Vector3[] corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);
            for (int i = 0; i < 4; i++) {
                corners[i] = canvas.worldCamera.WorldToScreenPoint(corners[i]);
            }
            return (corners[0], corners[2]);
        }

        public static T FindFirstOf<T>(this IEnumerable<GameObject> gos) where T:Component {
            foreach (GameObject go in gos) {
                T comp = go.GetComponentInChildren<T>();
                if (comp != null) {
                    return comp;
                }
            }
            return null;
        }
    }
}