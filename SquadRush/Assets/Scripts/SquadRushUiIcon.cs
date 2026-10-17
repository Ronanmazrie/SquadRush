using UnityEngine;
using UnityEngine.UI;

namespace SquadRush
{
    public enum SquadIcon
    {
        Rifle, Vest, Magazine, Supply, Battle, Helmet,
        Gem, Ticket, Coin, Character, Gift, Missions, Heal, Blast, Lightning,
        Allies, Target, Merge, Lock, Check, Trophy
    }

    // Original, resolution-independent illustrations. No textures or additional UI materials.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class SquadRushUiIcon : MaskableGraphic
    {
        [SerializeField] SquadIcon kind;
        public SquadIcon Kind
        {
            get => kind;
            set { if (kind == value) return; kind = value; SetVerticesDirty(); }
        }
        public int GeneratedVertices { get; private set; }
        static readonly Color dark = Hex(0x122238), steel = Hex(0xA5C9DD), light = Hex(0xE9F9FF);
        static readonly Color blue = Hex(0x269BD9), cyan = Hex(0x57E6FA), amber = Hex(0xFFCB55);
        static readonly Color orange = Hex(0xD78828), green = Hex(0x53C8A1), purple = Hex(0x9670E8);
        static Color Hex(int rgb) => new Color(((rgb >> 16) & 255) / 255f, ((rgb >> 8) & 255) / 255f, (rgb & 255) / 255f);

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            switch (Kind)
            {
                case SquadIcon.Gem:
                    P(vh,dark,.08f,.59f,.25f,.85f,.74f,.85f,.93f,.59f,.50f,.10f);
                    P(vh,blue,.15f,.59f,.29f,.78f,.70f,.78f,.85f,.59f,.50f,.19f);
                    P(vh,cyan,.15f,.59f,.29f,.78f,.44f,.60f,.50f,.19f);
                    P(vh,Hex(0xA9F5FF),.29f,.78f,.70f,.78f,.58f,.60f,.44f,.60f);
                    P(vh,Hex(0x1474B9),.58f,.60f,.85f,.59f,.50f,.19f);
                    P(vh,light,.31f,.75f,.44f,.60f,.51f,.60f,.45f,.75f);
                    Spark(vh,.78f,.88f,.085f);break;
                case SquadIcon.Ticket:
                    Ticket(vh);break;
                case SquadIcon.Coin:
                    Ellipse(vh,.56f,.40f,.35f,.29f,dark);Ellipse(vh,.56f,.42f,.29f,.24f,orange);
                    Ellipse(vh,.44f,.55f,.35f,.35f,dark);Ellipse(vh,.44f,.56f,.29f,.29f,amber);
                    Ellipse(vh,.44f,.56f,.215f,.215f,orange);Ellipse(vh,.44f,.58f,.18f,.18f,Hex(0xFFE48A));
                    Star(vh,.44f,.58f,.13f,orange);Spark(vh,.83f,.80f,.09f);break;
                case SquadIcon.Character:
                    Ellipse(vh,.50f,.71f,.21f,.23f,dark);Ellipse(vh,.50f,.72f,.15f,.17f,Hex(0xFFD3A1));
                    P(vh,dark,.12f,.13f,.17f,.38f,.33f,.51f,.67f,.51f,.83f,.38f,.88f,.13f);
                    P(vh,blue,.21f,.20f,.25f,.36f,.38f,.44f,.62f,.44f,.75f,.36f,.79f,.20f);
                    P(vh,cyan,.38f,.44f,.50f,.31f,.62f,.44f);Box(vh,.46f,.18f,.54f,.30f,amber);break;
                case SquadIcon.Gift:
                    Gift(vh);break;
                case SquadIcon.Missions:
                    Box(vh,.19f,.10f,.82f,.88f,dark);Box(vh,.25f,.17f,.76f,.81f,steel);
                    Box(vh,.33f,.76f,.67f,.94f,dark);Box(vh,.39f,.80f,.61f,.89f,amber);
                    for(int i=0;i<3;i++){float y=.59f-i*.16f;Check(vh,.32f,y,.17f,green);Box(vh,.52f,y,.68f,y+.04f,Hex(0x52778F));}break;
                case SquadIcon.Heal:
                    Box(vh,.27f,.68f,.73f,.89f,dark);Box(vh,.34f,.72f,.66f,.83f,steel);
                    Box(vh,.10f,.17f,.90f,.77f,dark);Box(vh,.17f,.24f,.83f,.70f,green);
                    Box(vh,.18f,.59f,.82f,.70f,Hex(0x89E8BD));Box(vh,.44f,.31f,.56f,.61f,light);Box(vh,.34f,.41f,.66f,.51f,light);break;
                case SquadIcon.Blast:
                    P(vh,dark,.06f,.38f,.28f,.54f,.17f,.88f,.43f,.73f,.59f,.97f,.67f,.66f,.94f,.72f,.80f,.46f,.97f,.24f,.65f,.25f,.46f,.06f,.34f,.30f);
                    P(vh,orange,.18f,.39f,.38f,.49f,.29f,.73f,.48f,.61f,.57f,.81f,.62f,.53f,.80f,.59f,.68f,.42f,.81f,.32f,.60f,.33f,.47f,.20f,.40f,.40f);
                    Ellipse(vh,.50f,.47f,.19f,.19f,amber);Ellipse(vh,.48f,.50f,.10f,.11f,light);break;
                case SquadIcon.Lightning:
                    P(vh,dark,.48f,.95f,.12f,.43f,.43f,.40f,.35f,.04f,.89f,.65f,.57f,.65f,.72f,.95f);
                    P(vh,amber,.51f,.86f,.25f,.49f,.53f,.47f,.48f,.24f,.75f,.58f,.46f,.58f,.61f,.86f);
                    P(vh,light,.51f,.86f,.25f,.49f,.38f,.49f,.60f,.86f);break;
                case SquadIcon.Allies:
                    Person(vh,.23f,.56f,.15f,steel);Person(vh,.77f,.56f,.15f,steel);Person(vh,.50f,.61f,.21f,cyan);break;
                case SquadIcon.Target:
                    Ellipse(vh,.50f,.50f,.38f,.38f,dark);Ellipse(vh,.50f,.50f,.31f,.31f,Hex(0xE9836B));
                    Ellipse(vh,.50f,.50f,.22f,.22f,dark);Ellipse(vh,.50f,.50f,.14f,.14f,amber);
                    Box(vh,.47f,.05f,.53f,.30f,light);Box(vh,.47f,.70f,.53f,.95f,light);Box(vh,.05f,.47f,.30f,.53f,light);Box(vh,.70f,.47f,.95f,.53f,light);break;
                case SquadIcon.Merge:
                    Box(vh,.09f,.14f,.34f,.40f,dark);Box(vh,.15f,.20f,.28f,.34f,green);
                    Box(vh,.66f,.14f,.91f,.40f,dark);Box(vh,.72f,.20f,.85f,.34f,blue);
                    P(vh,dark,.35f,.45f,.19f,.59f,.50f,.93f,.81f,.59f,.65f,.45f,.58f,.52f,.58f,.18f,.42f,.18f,.42f,.52f);
                    P(vh,amber,.31f,.59f,.50f,.80f,.69f,.59f,.55f,.59f,.55f,.25f,.45f,.25f,.45f,.59f);break;
                case SquadIcon.Lock:
                    Ellipse(vh,.50f,.66f,.26f,.29f,dark);Ellipse(vh,.50f,.66f,.15f,.18f,steel);Ellipse(vh,.50f,.64f,.08f,.13f,dark);
                    Box(vh,.17f,.13f,.83f,.64f,dark);Box(vh,.24f,.20f,.76f,.57f,steel);
                    Ellipse(vh,.50f,.42f,.07f,.07f,dark);Box(vh,.47f,.28f,.53f,.42f,dark);break;
                case SquadIcon.Check:
                    Ellipse(vh,.50f,.50f,.43f,.43f,dark);Ellipse(vh,.50f,.50f,.35f,.35f,green);Check(vh,.25f,.43f,.50f,light);break;
                case SquadIcon.Trophy:
                    P(vh,dark,.07f,.79f,.93f,.79f,.88f,.47f,.61f,.35f,.59f,.23f,.75f,.19f,.75f,.09f,.25f,.09f,.25f,.19f,.41f,.23f,.39f,.35f,.12f,.47f);
                    P(vh,orange,.15f,.72f,.85f,.72f,.81f,.52f,.61f,.43f,.39f,.43f,.19f,.52f);
                    P(vh,amber,.28f,.86f,.72f,.86f,.66f,.48f,.50f,.35f,.34f,.48f);
                    P(vh,Hex(0xFFF0A5),.31f,.82f,.43f,.82f,.43f,.49f,.37f,.55f);Box(vh,.45f,.19f,.55f,.38f,amber);Box(vh,.31f,.14f,.69f,.20f,amber);break;
                case SquadIcon.Rifle:
                    Box(vh,.08f,.41f,.85f,.62f,dark);Box(vh,.28f,.46f,.78f,.57f,steel);
                    P(vh,dark,.08f,.23f,.29f,.32f,.34f,.55f,.08f,.55f);Box(vh,.39f,.24f,.49f,.46f,dark);Box(vh,.56f,.22f,.68f,.46f,dark);
                    Box(vh,.75f,.47f,.96f,.56f,steel);Box(vh,.88f,.43f,.96f,.60f,dark);Box(vh,.35f,.60f,.65f,.71f,dark);Box(vh,.40f,.63f,.60f,.68f,cyan);Box(vh,.31f,.47f,.36f,.56f,amber);break;
                case SquadIcon.Vest:
                    P(vh,dark,.11f,.18f,.11f,.72f,.28f,.92f,.42f,.81f,.58f,.81f,.72f,.92f,.89f,.72f,.89f,.18f);
                    P(vh,green,.20f,.25f,.20f,.69f,.29f,.80f,.40f,.72f,.60f,.72f,.71f,.80f,.80f,.69f,.80f,.25f);
                    Box(vh,.24f,.29f,.45f,.49f,steel);Box(vh,.55f,.29f,.76f,.49f,steel);Box(vh,.47f,.24f,.53f,.73f,dark);Box(vh,.24f,.59f,.76f,.66f,amber);break;
                case SquadIcon.Magazine:
                    P(vh,dark,.22f,.12f,.15f,.77f,.73f,.87f,.86f,.21f);P(vh,steel,.29f,.23f,.24f,.70f,.67f,.77f,.76f,.27f);
                    for(int i=0;i<3;i++){float x=.32f+i*.13f;Box(vh,x,.33f,x+.07f,.64f,amber);}Box(vh,.24f,.18f,.79f,.27f,dark);break;
                case SquadIcon.Supply:
                    Box(vh,.12f,.17f,.88f,.84f,dark);Box(vh,.19f,.24f,.81f,.77f,orange);Box(vh,.20f,.64f,.80f,.77f,amber);
                    Box(vh,.25f,.20f,.32f,.80f,steel);Box(vh,.68f,.20f,.75f,.80f,steel);Star(vh,.50f,.47f,.18f,light);break;
                case SquadIcon.Battle:
                    P(vh,dark,.22f,.09f,.08f,.24f,.65f,.86f,.93f,.93f,.86f,.65f);
                    P(vh,steel,.23f,.28f,.67f,.75f,.82f,.82f,.75f,.66f,.30f,.21f);P(vh,light,.29f,.29f,.67f,.75f,.82f,.82f,.73f,.68f);
                    P(vh,dark,.13f,.40f,.22f,.48f,.54f,.16f,.45f,.08f);P(vh,amber,.19f,.39f,.22f,.42f,.48f,.16f,.45f,.13f);break;
                default:
                    Ellipse(vh,.50f,.42f,.25f,.30f,dark);Ellipse(vh,.50f,.43f,.19f,.23f,Hex(0xFFD3A1));
                    Ellipse(vh,.50f,.65f,.35f,.28f,dark);Ellipse(vh,.50f,.66f,.28f,.21f,blue);
                    P(vh,cyan,.27f,.68f,.35f,.80f,.54f,.84f,.48f,.67f);Box(vh,.12f,.49f,.88f,.60f,dark);Box(vh,.20f,.53f,.80f,.58f,steel);
                    Box(vh,.32f,.36f,.68f,.48f,dark);Box(vh,.36f,.40f,.64f,.45f,cyan);Box(vh,.42f,.23f,.58f,.28f,dark);break;
            }
            GeneratedVertices = vh.currentVertCount;
        }
        void Ticket(VertexHelper vh)
        {
            P(vh,dark,.11f,.20f,.11f,.41f,.21f,.50f,.11f,.59f,.11f,.80f,.89f,.80f,.89f,.59f,.79f,.50f,.89f,.41f,.89f,.20f);
            P(vh,purple,.18f,.27f,.18f,.38f,.30f,.50f,.18f,.62f,.18f,.73f,.82f,.73f,.82f,.62f,.70f,.50f,.82f,.38f,.82f,.27f);
            Box(vh,.27f,.64f,.73f,.70f,Hex(0xC6A8FF));Star(vh,.45f,.50f,.17f,amber);
            for(int i=0;i<3;i++)Box(vh,.66f,.32f+i*.13f,.69f,.39f+i*.13f,light);
        }
        void Gift(VertexHelper vh)
        {
            P(vh,dark,.48f,.69f,.18f,.78f,.18f,.94f,.40f,.92f,.50f,.78f,.60f,.92f,.82f,.94f,.82f,.78f,.52f,.69f);
            P(vh,amber,.45f,.76f,.25f,.82f,.25f,.87f,.36f,.86f);P(vh,amber,.55f,.76f,.75f,.82f,.75f,.87f,.64f,.86f);
            Box(vh,.14f,.13f,.86f,.70f,dark);Box(vh,.21f,.20f,.79f,.64f,Hex(0xD65F7B));
            Box(vh,.09f,.59f,.91f,.77f,dark);Box(vh,.16f,.64f,.84f,.71f,Hex(0xFF96AC));
            Box(vh,.44f,.20f,.56f,.71f,amber);Box(vh,.45f,.23f,.49f,.69f,Hex(0xFFF0A5));
        }
        void Person(VertexHelper vh,float x,float y,float radius,Color tint)
        {
            Ellipse(vh,x,y,radius,radius,dark);Ellipse(vh,x,y+.01f,radius*.70f,radius*.70f,tint);
            Ellipse(vh,x,y-radius*1.8f,radius*1.17f,radius*.92f,dark);Ellipse(vh,x,y-radius*1.7f,radius*.88f,radius*.66f,tint);
        }
        void Check(VertexHelper vh,float x,float y,float size,Color tint) => P(vh,tint,x,y+size*.28f,x+size*.30f,y,x+size,y+size*.75f,x+size*.85f,y+size*.90f,x+size*.29f,y+size*.31f,x+size*.13f,y+size*.46f);
        void Spark(VertexHelper vh,float x,float y,float radius) => P(vh,light,x-radius,y,x-radius*.22f,y+radius*.22f,x,y+radius,x+radius*.22f,y+radius*.22f,x+radius,y,x+radius*.22f,y-radius*.22f,x,y-radius,x-radius*.22f,y-radius*.22f);
        void Star(VertexHelper vh,float x,float y,float radius,Color tint)
        {
            // Separate triangles avoid a fan across the concave star's outline.
            for(int i=0;i<10;i++){float a=(90+i*36)*Mathf.Deg2Rad,b=(90+(i+1)*36)*Mathf.Deg2Rad;float r=i%2==0?radius:radius*.46f,s=i%2==0?radius*.46f:radius;P(vh,tint,x,y,x+Mathf.Cos(a)*r,y+Mathf.Sin(a)*r,x+Mathf.Cos(b)*s,y+Mathf.Sin(b)*s);}
        }
        void Ellipse(VertexHelper vh,float x,float y,float rx,float ry,Color tint)
        {
            for(int i=0;i<20;i++){float a=i*Mathf.PI/10,b=(i+1)*Mathf.PI/10;P(vh,tint,x,y,x+Mathf.Cos(a)*rx,y+Mathf.Sin(a)*ry,x+Mathf.Cos(b)*rx,y+Mathf.Sin(b)*ry);}
        }
        void Box(VertexHelper vh,float x0,float y0,float x1,float y1,Color tint) => P(vh,tint,x0,y0,x0,y1,x1,y1,x1,y0);
        void P(VertexHelper vh,Color tint,params float[] xy)
        {
            Rect rect=rectTransform.rect;float size=Mathf.Min(rect.width,rect.height);Vector2 origin=rect.center-Vector2.one*size*.5f;int first=vh.currentVertCount;
            for(int i=0;i<xy.Length;i+=2)vh.AddVert(origin+new Vector2(xy[i],xy[i+1])*size,tint*color,Vector2.zero);
            int count=xy.Length/2;
            if(count<=4){for(int i=1;i+1<count;i++)vh.AddTriangle(first,first+i,first+i+1);return;}
            // Ear clipping preserves notches in tickets, bows, checkmarks and burst silhouettes.
            var order=new int[count];float area=0;
            for(int i=0;i<count;i++){order[i]=i;int j=(i+1)%count;area+=xy[i*2]*xy[j*2+1]-xy[j*2]*xy[i*2+1];}
            float winding=area>=0?1:-1;
            while(count>2)
            {
                bool clipped=false;
                for(int i=0;i<count;i++)
                {
                    int a=order[(i+count-1)%count],b=order[i],c=order[(i+1)%count];
                    if(Cross(xy,a,b,c)*winding<=.000001f)continue;
                    bool contains=false;
                    for(int j=0;j<count;j++){int p=order[j];if(p==a||p==b||p==c)continue;if(Cross(xy,a,b,p)*winding>=0&&Cross(xy,b,c,p)*winding>=0&&Cross(xy,c,a,p)*winding>=0){contains=true;break;}}
                    if(contains)continue;
                    vh.AddTriangle(first+a,first+b,first+c);for(int j=i;j<count-1;j++)order[j]=order[j+1];count--;clipped=true;break;
                }
                if(!clipped)break;
            }
        }
        static float Cross(float[] p,int a,int b,int c) => (p[b*2]-p[a*2])*(p[c*2+1]-p[a*2+1])-(p[b*2+1]-p[a*2+1])*(p[c*2]-p[a*2]);
    }
}
