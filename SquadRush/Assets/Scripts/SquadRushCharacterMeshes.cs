using System.Collections.Generic;
using UnityEngine;

namespace SquadRush
{
    public sealed partial class SquadRushArt
    {
        // Connected ring surfaces replace the block-shaped body, clothes and boots.
        // Each team shares the finished body/leg meshes; animation never rebuilds them.
        Mesh HumanBody(Color uniform, bool boss, string name)
        {
            var b = new Recipe(this);
            Color cloth = uniform * 0.91f, skin = boss ? new Color(0.82f, 0.67f, 0.49f) : Skin;
            AddProfile(b, new float[,] {
                {0.45f,0.115f,0.105f,0}, {0.49f,0.15f,0.115f,0}, {0.57f,0.15f,0.115f,0},
                {0.68f,0.155f,0.125f,0}, {0.80f,0.195f,0.135f,0}, {0.89f,0.19f,0.12f,0},
                {0.95f,0.095f,0.075f,0}, {0.97f,0.065f,0.065f,0}}, cloth, 14);
            Oval(b, new Vector3(0,0.985f,0), new Vector3(0.135f,0.16f,0.13f), skin);
            // Shaped jaw, cheeks and forehead; the face looks along local +Z.
            AddProfile(b, new float[,] {
                {1.015f,0.055f,0.065f,0.025f}, {1.035f,0.09f,0.09f,0.03f},
                {1.08f,0.125f,0.115f,0.025f}, {1.145f,0.135f,0.12f,0.018f},
                {1.205f,0.13f,0.11f,0.01f}, {1.255f,0.095f,0.09f,0},
                {1.275f,0.045f,0.045f,0}}, skin, 16);
            for (int s = -1; s <= 1; s += 2)
            {
                Oval(b, new Vector3(s*0.137f,1.12f,0.01f), new Vector3(0.055f,0.085f,0.06f), skin*0.94f);
                Oval(b, new Vector3(s*0.06f,1.153f,0.122f), new Vector3(0.064f,0.044f,0.026f), new Color(1,0.96f,0.88f));
                Oval(b, new Vector3(s*0.058f,1.153f,0.135f), new Vector3(0.026f,0.032f,0.014f), Gear);
                SoftPanel(b, new Vector3(s*0.06f,1.183f,0.125f), new Vector3(0.070f,0.015f,0.018f), Gear, new Vector3(0,0,s*5));
            }
            Oval(b, new Vector3(0,1.112f,0.144f), new Vector3(0.048f,0.057f,0.055f), skin*1.04f);
            Tube(b, new[] {new Vector3(-0.038f,1.067f,0.127f),new Vector3(0,1.060f,0.14f),new Vector3(0.038f,1.067f,0.127f)},
                new[] {0.004f,0.006f,0.004f}, new Color(0.40f,0.23f,0.20f), 6);
            // A single curved helmet dome with a rounded projecting brow, rather than stacked discs.
            AddProfile(b, new float[,] {
                {1.195f,0.155f,0.14f,-0.01f}, {1.22f,0.17f,0.155f,-0.012f},
                {1.275f,0.163f,0.15f,-0.015f}, {1.32f,0.135f,0.125f,-0.018f},
                {1.353f,0.085f,0.08f,-0.02f}, {1.365f,0.025f,0.025f,-0.02f}}, uniform, 20);
            SoftPanel(b, new Vector3(0,1.207f,0.05f), new Vector3(0.34f,0.033f,0.31f), uniform*0.88f);
            SoftPanel(b, new Vector3(0,1.28f,-0.151f), new Vector3(0.105f,0.026f,0.016f), uniform*1.18f);
            // Contoured vest, rounded backpack and stitched straps follow the body silhouette.
            SoftPanel(b, new Vector3(0,0.765f,0.10f), new Vector3(0.31f,0.29f,0.10f), Gear);
            SoftPanel(b, new Vector3(0,0.765f,0.158f), new Vector3(0.23f,0.17f,0.025f), Gear*1.30f);
            SoftPanel(b, new Vector3(0,0.75f,-0.13f), new Vector3(0.29f,0.32f,0.13f), Gear);
            SoftPanel(b, new Vector3(0,0.75f,-0.203f), new Vector3(0.22f,0.19f,0.035f), cloth*0.74f);
            SoftPanel(b, new Vector3(0,0.52f,0.005f), new Vector3(0.30f,0.052f,0.23f), Gear);
            SoftPanel(b, new Vector3(0,0.52f,0.128f), new Vector3(0.060f,0.046f,0.022f), new Color(0.75f,0.66f,0.43f));
            for (int s = -1; s <= 1; s += 2)
            {
                SoftPanel(b, new Vector3(s*0.13f,0.84f,0.13f), new Vector3(0.04f,0.18f,0.038f), cloth*0.55f, new Vector3(0,0,s*12));
                SoftPanel(b, new Vector3(s*0.085f,0.64f,0.146f), new Vector3(0.095f,0.095f,0.055f), Gear*1.25f);
                CurvedSleeve(b,s,cloth);
                Vector3 wrist = s < 0 ? new Vector3(0.075f,0.695f,0.40f) : new Vector3(0.165f,0.69f,0.245f);
                Tube(b, new[] {new Vector3(s*0.26f,0.74f,0.06f),new Vector3(s*0.24f,0.705f,0.14f),wrist},
                    new[] {0.056f,0.052f,0.038f}, skin, 10);
                Oval(b, wrist, new Vector3(0.095f,0.09f,0.11f), Gear);
                // Subtle curved fabric seams, instead of flat rectangular knee/shoulder blocks.
                SoftPanel(b, new Vector3(s*0.12f,0.60f,-0.101f), new Vector3(0.043f,0.016f,0.012f), cloth*1.20f);
            }
            SoftPanel(b, new Vector3(0.15f,0.705f,0.37f), new Vector3(0.095f,0.09f,0.33f), Gear);
            SoftPanel(b, new Vector3(0.15f,0.758f,0.31f), new Vector3(0.065f,0.034f,0.15f), Steel);
            SoftPanel(b, new Vector3(0.15f,0.65f,0.29f), new Vector3(0.055f,0.11f,0.08f), Gear);
            Tube(b, new[] {new Vector3(0.15f,0.71f,0.48f),new Vector3(0.15f,0.71f,0.65f)}, new[] {0.032f,0.026f}, Steel, 10);
            if (boss)
            {
                SoftPanel(b, new Vector3(0,0.80f,0.17f), new Vector3(0.27f,0.10f,0.05f), new Color(0.83f,0.19f,0.14f));
                for (int s = -1; s <= 1; s += 2)
                    Oval(b, new Vector3(s*0.225f,0.91f,-0.005f), new Vector3(0.19f,0.13f,0.24f), new Color(0.69f,0.39f,0.16f));
            }
            return b.Bake(name);
        }
        Mesh HumanLeg(Color uniform, string name)
        {
            var b = new Recipe(this);
            AddProfile(b, new float[,] {
                {-0.01f,0.055f,0.060f,0}, {-0.065f,0.077f,0.078f,0}, {-0.16f,0.066f,0.073f,0.007f},
                {-0.22f,0.056f,0.064f,0.022f}, {-0.29f,0.059f,0.061f,0.005f}, {-0.38f,0.046f,0.050f,-0.005f}}, uniform*0.91f, 12);
            Oval(b, new Vector3(0,-0.225f,0.066f), new Vector3(0.108f,0.105f,0.045f), Steel*0.64f);
            AddProfile(b,new float[,] {
                {-0.48f,0.061f,0.081f,0.018f}, {-0.43f,0.060f,0.074f,0.008f},
                {-0.36f,0.057f,0.060f,0}, {-0.32f,0.050f,0.054f,0}}, Gear,12);
            Oval(b,new Vector3(0,-0.455f,0.065f),new Vector3(0.14f,0.10f,0.24f),Gear*1.10f);
            SoftPanel(b, new Vector3(0,-0.493f,0.063f), new Vector3(0.145f,0.025f,0.24f), Gear*0.75f);
            return b.Bake(name);
        }
        void CurvedSleeve(Recipe b,int side,Color color)
        {
            var centers=new Vector3[7]; var radii=new Vector2[7];
            Vector3 a=new Vector3(side*0.15f,0.89f,0), d=new Vector3(side*0.255f,0.72f,0.07f);
            Vector3 controlA=new Vector3(side*0.23f,0.91f,-0.005f),controlB=new Vector3(side*0.28f,0.80f,0.02f);
            for(int i=0;i<7;i++)
            {
                float t=i/6f,u=1-t; centers[i]=u*u*u*a+3*u*u*t*controlA+3*u*t*t*controlB+t*t*t*d;
                radii[i]=Vector2.one*(Mathf.Lerp(0.067f,0.052f,t)+Mathf.Sin(t*Mathf.PI)*0.011f);
            }
            b.AddMesh(RingSurface("Continuous sleeve",centers,radii,10,true),Vector3.zero,Vector3.one,color);
        }
        void Oval(Recipe b, Vector3 position, Vector3 scale, Color color)
        {
            const string key = "Character smooth oval";
            if (!models.TryGetValue(key, out Mesh mesh))
            {
                var centers = new Vector3[7]; var radii = new Vector2[7];
                for (int i=0;i<7;i++) { float a=-Mathf.PI*0.5f+i*Mathf.PI/6; centers[i]=Vector3.up*Mathf.Sin(a)*0.5f; radii[i]=Vector2.one*Mathf.Max(0.001f,Mathf.Cos(a)*0.5f); }
                mesh=RingSurface(key,centers,radii,10,false); models.Add(key,mesh);
            }
            b.AddMesh(mesh,position,scale,color);
        }
        void SoftPanel(Recipe b, Vector3 position, Vector3 scale, Color color, Vector3 rotation=default)
        {
            const string key="Character rounded leather";
            if (!models.TryGetValue(key,out Mesh mesh))
            {
                var centers=new[] {Vector3.up*-0.5f,Vector3.up*-0.45f,Vector3.up*0.45f,Vector3.up*0.5f};
                var radii=new[] {Vector2.one*0.39f,Vector2.one*0.5f,Vector2.one*0.5f,Vector2.one*0.39f};
                mesh=RingSurface(key,centers,radii,12,false,4); models.Add(key,mesh);
            }
            b.AddMesh(mesh,position,scale,color,rotation);
        }
        void AddProfile(Recipe b,float[,] rings,Color color,int slices)
        {
            int count=rings.GetLength(0); var centers=new Vector3[count]; var radii=new Vector2[count];
            // All ring surfaces run from bottom to top for consistent outward triangle winding.
            bool reverse=rings[0,0]>rings[count-1,0];
            for(int i=0;i<count;i++) { int n=reverse?count-1-i:i; centers[i]=new Vector3(0,rings[n,0],rings[n,3]); radii[i]=new Vector2(rings[n,1],rings[n,2]); }
            b.AddMesh(RingSurface("Tailored character surface",centers,radii,slices,false),Vector3.zero,Vector3.one,color);
        }
        void Tube(Recipe b,Vector3[] centers,float[] radius,Color color,int slices)
        {
            var radii=new Vector2[radius.Length]; for(int i=0;i<radii.Length;i++) radii[i]=Vector2.one*radius[i];
            b.AddMesh(RingSurface("Curved character limb",centers,radii,slices,true),Vector3.zero,Vector3.one,color);
        }
        Mesh RingSurface(string name,Vector3[] centers,Vector2[] radii,int slices,bool follow,float exponent=2)
        {
            var vertices=new List<Vector3>(); var colors=new List<Color>(); var triangles=new List<int>();
            for(int ring=0;ring<centers.Length;ring++)
            {
                Vector3 tangent=centers[Mathf.Min(ring+1,centers.Length-1)]-centers[Mathf.Max(0,ring-1)];
                Vector3 axisX=Vector3.right,axisZ=Vector3.forward;
                if(follow)
                {
                    tangent.Normalize(); Vector3 reference=Mathf.Abs(Vector3.Dot(tangent,Vector3.forward))>0.95f?Vector3.right:Vector3.forward;
                    axisX=Vector3.Cross(tangent,reference).normalized; axisZ=Vector3.Cross(axisX,tangent).normalized;
                }
                for(int s=0;s<slices;s++)
                {
                    float angle=s*Mathf.PI*2/slices, x=Mathf.Cos(angle), z=Mathf.Sin(angle);
                    Vector3 p=centers[ring]+axisX*(Mathf.Sign(x)*Mathf.Pow(Mathf.Abs(x),2/exponent)*radii[ring].x)+axisZ*(Mathf.Sign(z)*Mathf.Pow(Mathf.Abs(z),2/exponent)*radii[ring].y);
                    vertices.Add(p); colors.Add(Color.white*(0.97f+Mathf.PerlinNoise(p.x*19+17,p.y*23+p.z*11)*0.06f));
                    if(ring==centers.Length-1) continue;
                    int a=ring*slices+s,b=ring*slices+(s+1)%slices,c=a+slices,d=b+slices;
                    triangles.Add(a);triangles.Add(c);triangles.Add(b);triangles.Add(b);triangles.Add(c);triangles.Add(d);
                }
            }
            for(int cap=0;cap<2;cap++)
            {
                int ring=cap==0?0:centers.Length-1,start=vertices.Count; vertices.Add(centers[ring]); colors.Add(Color.white);
                for(int s=0;s<slices;s++) { int a=ring*slices+s,b=ring*slices+(s+1)%slices; triangles.Add(start);triangles.Add(cap==0?a:b);triangles.Add(cap==0?b:a); }
            }
            Mesh mesh=NewMesh(name,vertices,colors,triangles); mesh.RecalculateNormals(); return mesh;
        }
    }
}
