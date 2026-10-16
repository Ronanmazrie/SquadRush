using UnityEngine;
using UnityEngine.UI;

namespace SquadRush
{
    public enum SquadIcon { Rifle, Vest, Magazine, Supply, Battle, Helmet }

    // Original vector silhouettes, sharing the UI material and requiring no bitmap downloads.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class SquadRushUiIcon : MaskableGraphic
    {
        public SquadIcon Kind;
        public int GeneratedVertices { get; private set; }
        static readonly Color dark = new Color(0.035f, 0.06f, 0.10f);
        static readonly Color steel = new Color(0.65f, 0.77f, 0.84f);
        static readonly Color cyan = new Color(0.17f, 0.72f, 1);
        static readonly Color amber = new Color(1, 0.72f, 0.18f);
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            if (Kind == SquadIcon.Rifle)
            {
                Box(vh, 0.14f, 0.41f, 0.77f, 0.59f, dark); Box(vh, 0.27f, 0.44f, 0.70f, 0.56f, steel);
                Poly(vh, dark, new Vector2(0.09f, 0.28f), new Vector2(0.28f, 0.36f), new Vector2(0.34f, 0.57f), new Vector2(0.09f, 0.56f));
                Box(vh, 0.37f, 0.28f, 0.47f, 0.46f, dark); Box(vh, 0.55f, 0.24f, 0.65f, 0.45f, dark);
                Box(vh, 0.72f, 0.46f, 0.94f, 0.54f, steel); Box(vh, 0.85f, 0.42f, 0.94f, 0.58f, dark);
                Box(vh, 0.34f, 0.57f, 0.62f, 0.65f, dark); Box(vh, 0.39f, 0.59f, 0.56f, 0.63f, cyan);
                Box(vh, 0.29f, 0.45f, 0.34f, 0.54f, amber);
            }
            else if (Kind == SquadIcon.Vest)
            {
                Poly(vh, dark, new Vector2(0.12f, 0.21f), new Vector2(0.12f, 0.73f), new Vector2(0.29f, 0.90f), new Vector2(0.42f, 0.80f), new Vector2(0.58f, 0.80f), new Vector2(0.71f, 0.90f), new Vector2(0.88f, 0.73f), new Vector2(0.88f, 0.21f));
                Box(vh, 0.21f, 0.25f, 0.79f, 0.73f, new Color(0.18f, 0.59f, 0.55f));
                Box(vh, 0.25f, 0.30f, 0.44f, 0.51f, steel); Box(vh, 0.56f, 0.30f, 0.75f, 0.51f, steel);
                Box(vh, 0.47f, 0.25f, 0.53f, 0.75f, dark); Box(vh, 0.24f, 0.60f, 0.76f, 0.67f, amber);
            }
            else if (Kind == SquadIcon.Magazine)
            {
                Poly(vh, dark, new Vector2(0.22f, 0.14f), new Vector2(0.16f, 0.76f), new Vector2(0.73f, 0.86f), new Vector2(0.85f, 0.22f));
                Box(vh, 0.29f, 0.24f, 0.72f, 0.70f, steel);
                for (int i = 0; i < 3; i++) { float x = 0.32f + i * 0.13f; Box(vh, x, 0.32f, x + 0.07f, 0.64f, amber); }
                Box(vh, 0.24f, 0.19f, 0.79f, 0.27f, dark);
            }
            else if (Kind == SquadIcon.Supply)
            {
                Box(vh, 0.12f, 0.19f, 0.88f, 0.83f, dark); Box(vh, 0.17f, 0.23f, 0.83f, 0.78f, new Color(0.67f, 0.36f, 0.16f));
                Box(vh, 0.24f, 0.20f, 0.31f, 0.81f, steel); Box(vh, 0.69f, 0.20f, 0.76f, 0.81f, steel);
                Box(vh, 0.44f, 0.33f, 0.56f, 0.67f, cyan); Box(vh, 0.34f, 0.44f, 0.66f, 0.56f, cyan);
            }
            else if (Kind == SquadIcon.Battle)
            {
                Poly(vh, dark, new Vector2(0.28f, 0.11f), new Vector2(0.11f, 0.28f), new Vector2(0.65f, 0.85f), new Vector2(0.90f, 0.90f), new Vector2(0.85f, 0.65f));
                Poly(vh, steel, new Vector2(0.24f, 0.28f), new Vector2(0.66f, 0.73f), new Vector2(0.82f, 0.82f), new Vector2(0.72f, 0.64f), new Vector2(0.29f, 0.22f));
                Box(vh, 0.15f, 0.22f, 0.48f, 0.30f, amber);
            }
            else
            {
                Box(vh, 0.28f, 0.18f, 0.72f, 0.61f, new Color(1, 0.76f, 0.51f));
                Box(vh, 0.18f, 0.56f, 0.82f, 0.84f, cyan); Box(vh, 0.12f, 0.50f, 0.88f, 0.59f, dark);
                Box(vh, 0.28f, 0.42f, 0.72f, 0.53f, dark); Box(vh, 0.32f, 0.44f, 0.66f, 0.50f, steel);
                Box(vh, 0.40f, 0.23f, 0.60f, 0.29f, dark);
            }
            GeneratedVertices = vh.currentVertCount;
        }
        void Box(VertexHelper vh, float x0, float y0, float x1, float y1, Color tint) => Poly(vh, tint, new Vector2(x0, y0), new Vector2(x0, y1), new Vector2(x1, y1), new Vector2(x1, y0));
        void Poly(VertexHelper vh, Color tint, params Vector2[] points)
        {
            Rect rect = rectTransform.rect; float size = Mathf.Min(rect.width, rect.height);
            Vector2 origin = rect.center - Vector2.one * size * 0.5f; int first = vh.currentVertCount;
            for (int i = 0; i < points.Length; i++) vh.AddVert(origin + points[i] * size, tint * color, Vector2.zero);
            for (int i = 1; i + 1 < points.Length; i++) vh.AddTriangle(first, first + i, first + i + 1);
        }
    }
}
