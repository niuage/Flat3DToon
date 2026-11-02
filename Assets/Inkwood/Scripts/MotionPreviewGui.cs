using UnityEngine;

namespace Inkwood
{
    // Shared, deliberately small controls for the diorama and the character studio.
    public static class MotionPreviewGui
    {
        public static readonly Color Ink = new Color(.28f, .21f, .18f);
        static GUIStyle label, title, button, selected;
        static Texture2D paper, hover, accent;

        static Texture2D Pixel(Color color)
        {
            var texture = new Texture2D(1, 1) { hideFlags = HideFlags.HideAndDontSave };
            texture.SetPixel(0, 0, color); texture.Apply(); return texture;
        }
        static void EnsureStyles()
        {
            if (paper) return;
            paper = Pixel(new Color(.98f, .96f, .84f, .96f));
            hover = Pixel(new Color(.90f, .89f, .70f));
            accent = Pixel(new Color(.74f, .79f, .48f));
            label = new GUIStyle(GUI.skin.label) { fontSize = 13, alignment = TextAnchor.MiddleLeft };
            label.normal.textColor = Ink;
            title = new GUIStyle(label) { fontSize = 11, fontStyle = FontStyle.Bold };
            button = new GUIStyle(GUI.skin.button) { fontSize = 12, alignment = TextAnchor.MiddleCenter, border = new RectOffset(0, 0, 0, 0) };
            button.normal.background = paper; button.normal.textColor = Ink;
            button.hover.background = hover; button.hover.textColor = Ink;
            button.active.background = accent; button.active.textColor = Ink;
            selected = new GUIStyle(button); selected.normal.background = accent;
        }
        public static void Panel(Rect rect)
        {
            EnsureStyles(); var old = GUI.color;
            GUI.color = Ink; GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = Color.white; GUI.DrawTexture(new Rect(rect.x + 1, rect.y + 1, rect.width - 2, rect.height - 2), paper);
            GUI.color = old;
        }
        public static void Label(Rect rect, string text, bool heading = false)
        {
            EnsureStyles(); GUI.Label(rect, text, heading ? title : label);
        }
        public static bool Button(Rect rect, string text, bool active = false)
        {
            EnsureStyles(); return GUI.Button(rect, text, active ? selected : button);
        }
    }
}
