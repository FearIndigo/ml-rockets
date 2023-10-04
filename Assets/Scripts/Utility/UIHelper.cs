using System;
using UnityEngine.UIElements;

namespace FearIndigo.Utility
{
    public static class UIHelper
    {
        /// <summary>
        /// <para>
        /// Create a ui element with pre-defined uss classes.
        /// </para>
        /// </summary>
        /// <param name="classes">Uss classes to add to the element</param>
        public static T Create<T>(params string[] classes)
            where T : VisualElement, new()
        {
            var element = new T();

            foreach (var ussClass in classes)
            {
                element.AddToClassList(ussClass);
            }
            
            return element;
        }
        
        /// <summary>
        /// <para>
        /// Create a visual element with pre-defined uss classes.
        /// </para>
        /// </summary>
        /// <param name="classes">Uss classes to add to the element</param>
        public static VisualElement Create(params string[] classes) => Create<VisualElement>(classes);

        /// <summary>
        /// <para>
        /// Create a text element with pre-defined text and uss classes.
        /// </para>
        /// </summary>
        /// <param name="text">Button text</param>
        /// <param name="classes">Uss classes to add to the element</param>
        public static T Create<T>(string text, params string[] classes)
            where T : TextElement, new()
        {
            var element = Create<T>(classes);
            element.text = text;
            return element;
        }
        
        /// <summary>
        /// <para>
        /// Create a button element with pre-defined text, click action and uss classes.
        /// </para>
        /// </summary>
        /// <param name="text">Button text.</param>
        /// <param name="clickAction">Button click action.</param>
        /// <param name="classes">Uss classes to add to the element.</param>
        public static T Create<T>(string text, Action clickAction, params string[] classes)
            where T : Button, new()
        {
            var element = Create<T>(text, classes);
            element.clicked += clickAction;
            return element;
        }
        
        /// <summary>
        /// <para>
        /// Add multiple elements to this element's contentContainer.
        /// </para>
        /// </summary>
        /// <param name="container"></param>
        /// <param name="children"></param>
        public static T Add<T>(this T container, params VisualElement[] children)
            where T : VisualElement
        {
            foreach (var child in children)
            {
                container.Add(child);
            }
            return container;
        }
    }
}