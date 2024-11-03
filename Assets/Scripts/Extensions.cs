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
    }
}