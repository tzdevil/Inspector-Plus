using System.Reflection;
using UnityEngine;

namespace tzdevil.InspectorPlus
{
    public class ComponentData
    {
        public ComponentData(Component component)
        {
            Component = component;
            Foldout = true; // so the foldout shows the details upon selection

            var componentType = component.GetType().GetCustomAttribute<AddComponentMenu>(true);

            if (componentType != null)
            {
                var componentMenu = componentType.componentMenu;
                Types = componentMenu.Split("/")[..^0];
            }
            else
            {
                // Types is a string array so I can create sub-tabs in the future (check line 18).
                if (component is Transform or RectTransform)
                    Types = new string[1] { "Transform" };
                else if (component is Camera)
                    Types = new string[1] { "Camera" };
                else if (component is Renderer)
                    Types = new string[1] { "Renderer" };
                else if (component is Collider or Collider2D)
                    Types = new string[1] { "Collider" };
                else
                    Types = new string[1] { "Miscellaneous" };
            }
        }

        public Component Component { get; private set; }
        public bool Foldout { get; set; }
        public string[] Types { get; private set; }
    }
}