using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Inkwood.Editor
{
    public static partial class InkwoodSceneBuilder
    {
        static void MakeDetailedMeshes()
        {
            var hat=Lathe(new[]{new Vector2(0,0),new Vector2(.43f,0),new Vector2(.43f,.05f),new Vector2(.29f,.12f),new Vector2(.2f,.53f),new Vector2(.07f,.83f),new Vector2(0,.99f)},40);
            var hv=hat.vertices;var hn=hat.normals;
            for(int i=0;i<hv.Length;i++){float y=hv[i].y;hv[i].x-=.19f*y*y;hn[i]=V(hn[i].x,hn[i].y+.38f*y*hn[i].x,hn[i].z).normalized;}
            hat.vertices=hv;hat.normals=hn;hat.RecalculateBounds();Store("bentHat",hat);
            Store("nose",Lathe(new[]{new Vector2(0,0),new Vector2(.10f,.02f),new Vector2(.12f,.12f),new Vector2(.075f,.27f),new Vector2(.015f,.44f),new Vector2(0,.45f)},24));
            Store("axeHead",Panel(new[]{new Vector2(-.05f,.12f),new Vector2(.15f,.26f),new Vector2(.35f,.29f),new Vector2(.31f,-.11f),new Vector2(.1f,-.22f),new Vector2(.08f,.02f),new Vector2(-.05f,.01f)},.075f));
            Store("shield",BowedShield());
            var pine=Lathe(new[]{new Vector2(0,0),new Vector2(.5f,.035f),new Vector2(.46f,.14f),new Vector2(.37f,.4f),new Vector2(.22f,.75f),new Vector2(0,1.2f)},48);
            var pv=pine.vertices;
            for(int i=0;i<pv.Length;i++)
            {
                float a=Mathf.Atan2(pv[i].z,pv[i].x);float r=new Vector2(pv[i].x,pv[i].z).magnitude;
                if(r>.1f)pv[i].y+=.045f*Mathf.Cos(a*5)*Mathf.Exp(-pv[i].y*4);
            }
            pine.vertices=pv;pine.RecalculateBounds();Store("pineSkirt",pine);
            Store("cedarTier",Lathe(new[]{new Vector2(0,0),new Vector2(.4f,.025f),new Vector2(.5f,.17f),new Vector2(.48f,.38f),new Vector2(.3f,.60f),new Vector2(.14f,.7f),new Vector2(0,.72f)},40));
            Store("pillarLeft",WeatheredPrism(new[]{V(-.12f,0,0),V(-.03f,1.25f,.01f),V(.15f,2.8f,0)},new[]{V(.34f,0,.36f),V(.52f,0,.43f),V(.36f,0,.35f)}));
            Store("pillarRight",WeatheredPrism(new[]{V(.16f,0,0),V(.04f,1.45f,.02f),V(-.06f,2.8f,0)},new[]{V(.43f,0,.36f),V(.52f,0,.48f),V(.35f,0,.38f)}));
            Store("lintel",WeatheredPrism(new[]{V(0,-.18f,0),V(-.06f,.12f,.02f),V(.03f,.35f,.02f)},new[]{V(1.7f,0,.67f),V(1.94f,0,.75f),V(1.45f,0,.52f)}));
        }
        static Mesh BowedShield()
        {
            Vector2[] edge={new Vector2(-.32f,.34f),new Vector2(0,.27f),new Vector2(.32f,.34f),new Vector2(.25f,-.2f),new Vector2(0,-.53f),new Vector2(-.25f,-.2f)};
            var vertices=new List<Vector3>{V(0,-.04f,-.14f)};
            vertices.AddRange(edge.Select(p=>V(p.x,p.y,-.04f)));vertices.Add(V(0,-.04f,.05f));vertices.AddRange(edge.Select(p=>V(p.x,p.y,.05f)));
            var triangles=new List<int>();
            for(int i=0;i<6;i++)
            {
                int a=1+i,b=1+(i+1)%6;
                triangles.AddRange(new[]{0,a,b,7,b+7,a+7,a,a+7,b,b,a+7,b+7});
            }
            var mesh=new Mesh();mesh.SetVertices(vertices);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();return mesh;
        }
        static Mesh WeatheredPrism(Vector3[] centers,Vector3[] radii)
        {
            const int sides=8;var v=new List<Vector3>();var tr=new List<int>();
            for(int j=0;j<centers.Length;j++)for(int i=0;i<sides;i++)
            {
                float a=i*Mathf.PI*2/sides, wobble=1+.1f*Mathf.Sin(i*3+j);
                v.Add(centers[j]+V(Mathf.Cos(a)*radii[j].x*wobble,0,Mathf.Sin(a)*radii[j].z*wobble));
                if(j<centers.Length-1){int k=j*sides+i,n=j*sides+(i+1)%sides;tr.AddRange(new[]{k,k+sides,n,n,k+sides,n+sides});}
            }
            for(int i=1;i<sides-1;i++){tr.AddRange(new[]{0,i,i+1});int o=(centers.Length-1)*sides;tr.AddRange(new[]{o,o+i+1,o+i});}
            var mesh=new Mesh();mesh.SetVertices(v);mesh.SetTriangles(tr,0);mesh.RecalculateNormals();mesh.RecalculateBounds();return mesh;
        }
        static void Branch(Transform t,string name,Vector3[] points,float width,string color,float tip=.02f)
        {
            string key="stroke"+(strokeId++).ToString("D4");Store(key,Tube(points,width,10,tip));
            Part(name,t,key,Vector3.zero,Vector3.one,color);
        }
        static void ReferenceTree(Transform parent,float x,float z,float scale,int kind)
        {
            if(kind==5){ReferencePine(parent,x,z,scale);return;}
            string name=kind==3?"Bare twisting oak":kind==4?"Broad old oak":kind==2?"Orange cloud tree":kind==1?"Round teal tree":"Tall scalloped tree";
            var t=Group(name,parent);t.localPosition=V(x,0,z);
            t.localScale=V(scale*(kind==2?.88f:1),scale*(kind==0?1.25f:kind==2?1.35f:kind==4?1.5f:1),scale);
            Shadow(t,0,0,2.1f,1.2f);
            Branch(t,"Bent tapering trunk",new[]{V(0,0,0),V(-.06f,.8f,.02f),V(.06f,1.9f,0),V(-.12f,3.1f,0)},.26f,"Wood",.1f);
            Branch(t,"Left fork",new[]{V(0,1.4f,0),V(-.52f,2.15f,.03f),V(-.81f,2.93f,.02f)},.13f,"Wood");
            Branch(t,"Right fork",new[]{V(.02f,1.9f,0),V(.57f,2.55f,.07f),V(.78f,3.25f,.06f)},.12f,"Wood");
            if(kind==3)
            {
                Branch(t,"High crooked fork",new[]{V(-.1f,2.6f,0),V(-.5f,3.55f,.07f),V(-.62f,4.35f,.04f)},.12f,"Wood");
                Branch(t,"Outer right bough",new[]{V(.3f,2.2f,.02f),V(1.04f,2.8f,.08f),V(1.45f,3.61f,.09f)},.1f,"Wood");
                Branch(t,"Left twig",new[]{V(-.53f,2.28f,0),V(-1.16f,2.56f,0),V(-1.38f,3.04f,0)},.07f,"Wood",.012f);
                Branch(t,"Top twig",new[]{V(-.47f,3.5f,.04f),V(-.18f,3.76f,.05f),V(.04f,4.18f,.06f)},.055f,"Wood",.012f);
                t.localScale=V(scale*1.4f,scale*1.5f,scale);
                Stroke(t,.018f,V(.07f,.5f,-.25f),V(.02f,.9f,-.25f));return;
            }
            int shape=kind==0?0:kind==2?2:1;
            string color=kind==2?"Orange":kind==1?"Pine":kind==4?"LeafLight":"Leaf";
            Vector3 size=kind==0?V(1.5f,2.05f,1.25f):kind==4?V(2.25f,1.1f,1.75f):kind==1?V(1.7f,1.55f,1.45f):V(1.5f,1.5f,1.3f);
            Vector3 center=V(0,kind==4?3.9f:kind==0||kind==2?4.2f:3.7f,0);
            var crown=Part("Sculpted scalloped crown",t,"canopy"+shape,center,size,color);
            FoliageMarks(crown.transform,shape,kind==4?10:8);
            Stroke(t,.016f,V(-.08f,.6f,-.255f),V(-.1f,1,-.25f));
            if(kind==0)Sprout(t,.37f,-.2f,.5f);
        }
        static void FoliageMarks(Transform crown,int shape,int count)
        {
            for(int side=-1;side<=1;side+=2)for(int j=0;j<count;j++)
            {
                float cx=Rand(-.62f,.6f),cy=Rand(-.6f,.64f),r=Rand(.09f,.17f);var p=new Vector3[12];
                for(int k=0;k<p.Length;k++)
                {
                    float a=Mathf.Lerp(20,155,k/11f)*Mathf.Deg2Rad;
                    float px=cx+Mathf.Cos(a)*r,py=cy+Mathf.Sin(a)*r*.7f;
                    var direction=V(px,py,side*Mathf.Sqrt(Mathf.Max(.03f,1-px*px-py*py))).normalized;
                    p[k]=CanopyPoint(Mathf.Acos(direction.y),Mathf.Atan2(direction.z,direction.x),shape)*1.022f;
                }
                Curve(crown,"Broken canopy stroke",p,.011f,"Ink");
            }
        }
        static void ReferencePine(Transform parent,float x,float z,float scale)
        {
            var t=Group(scale<.8f?"Small layered cedar":"Tall sweeping pine",parent);t.localPosition=V(x,0,z);t.localScale=Vector3.one*scale;
            Rod("Visible brown trunk",t,V(0,0,0),V(0,1.2f,0),.15f,"Wood");
            if(scale<.8f)
            {
                for(int i=0;i<3;i++)
                    Part("Rounded layered cedar tuft",t,"cedarTier",V(0,.55f+i*.7f,0),V(1.95f-i*.4f,1.45f,1.7f-i*.36f),"Pine");
                Arc(t,V(0,1.15f,-.6f),.33f,205,315,.015f);return;
            }
            for(int i=0;i<3;i++)
            {
                float size=2.55f-i*.66f;
                Part("Curved scalloped needle tier",t,"pineSkirt",V(0,.55f+i*.84f,0),V(size,1.4f,size*.88f),"Pine");
            }
            for(int i=0;i<2;i++)Arc(t,V(.1f,1.17f+i*.78f,-.7f+i*.14f),.3f,210,305,.015f);
        }
        static void Bush(Transform parent,float x,float z,float scale)
        {
            var t=Group("Scalloped undergrowth",parent);t.localPosition=V(x,0,z);t.localScale=Vector3.one*scale;
            var crown=Part("Low cloud of leaves",t,"canopy0",V(0,.63f,0),V(1,.82f,.75f),"LeafDark");FoliageMarks(crown.transform,0,6);
            var small=Part("Side leaf tuft",t,"canopy2",V(.65f,.43f,-.1f),V(.65f,.5f,.55f),"Leaf");FoliageMarks(small.transform,2,3);
        }
        static void RuinedArch(Transform parent,float x,float z,float scale,float yaw)
        {
            var t=Group("Ivy covered broken arch",parent);t.localPosition=V(x,0,z);t.localScale=Vector3.one*scale;t.localRotation=Quaternion.Euler(0,yaw,0);
            Box("Tall left masonry pier",t,V(-.8f,1.95f,0),V(.65f,3.9f,.68f),"Stone");
            Box("Short broken pier",t,V(.95f,1.05f,.03f),V(.72f,2.1f,.7f),"Stone");
            Box("Broken upper block",t,V(-.98f,4.02f,0),V(.36f,.48f,.65f),"StoneTop");
            var beam=Box("Sloping fallen arch stone",t,V(.12f,2.73f,.03f),V(2.15f,.53f,.65f),"StoneTop");beam.transform.localRotation=Quaternion.Euler(0,0,-32);
            Box("Low connecting masonry",t,V(.16f,.52f,0),V(1.38f,1,.58f),"Stone");
            for(int i=0;i<5;i++)
            {
                float yy=.7f+i*.66f,xx=i%2==0?-.77f:-.95f;
                Part("Scalloped climbing moss",t,"canopy2",V(xx,yy,-.37f),V(.39f,.28f,.07f),"Moss",false);
                if(i%2==0)Stroke(t,.014f,V(xx-.18f,yy+.1f,-.43f),V(xx-.22f,yy-.03f,-.43f),V(xx-.13f,yy-.1f,-.43f));
            }
            OutlineTube(t,"Climbing ivy stem",new[]{V(.78f,.12f,-.4f),V(.8f,1.4f,-.4f),V(.4f,2.3f,-.4f),V(-.44f,3.24f,-.4f)},.075f,"Moss");
            for(int i=0;i<5;i++)Part("Ivy leaf",t,"leaf",V(.72f-i*.24f,.7f+i*.55f,-.44f),V(.7f,.55f,.12f),"LeafDark").transform.localRotation=Quaternion.Euler(0,0,i%2==0?-55:45);
            Stroke(t,.017f,V(-1.09f,1.77f,-.36f),V(-.83f,1.77f,-.36f),V(-.83f,1.48f,-.36f));
            Stroke(t,.017f,V(.7f,.47f,-.33f),V(.7f,.88f,-.33f),V(.91f,.88f,-.33f));
            Rock(t,-1.13f,-.42f,.42f);Rock(t,1.25f,-.27f,.39f);
        }
        static void Crate(Transform parent,float x,float z,float scale)
        {
            var t=Group("Planked supply crate",parent);t.localPosition=V(x,0,z);t.localScale=Vector3.one*scale;t.localRotation=Quaternion.Euler(0,50,0);
            Box("Dark crate interior",t,V(0,.5f,0),Vector3.one,"WoodDark");
            for(int i=0;i<4;i++)
            {
                float a=-.375f+i*.25f;
                Box("Front stave",t,V(a,.5f,-.52f),V(.205f,.91f,.065f),"Wood");
                Box("Lid plank",t,V(a,1.02f,0),V(.205f,.07f,1),"WoodLight");
            }
            Box("Upper frame rail",t,V(0,.97f,-.58f),V(1.12f,.12f,.08f),"WoodLight");
            Box("Lower frame rail",t,V(0,.08f,-.58f),V(1.12f,.12f,.08f),"WoodLight");
            for(int i=-1;i<=1;i+=2)Box("Corner batten",t,V(i*.48f,.52f,-.57f),V(.1f,1.1f,.09f),"WoodLight");
        }
        static void SkullStake(Transform parent,float x,float z,float height)
        {
            var t=Group("Skull marker on crooked stake",parent);t.localPosition=V(x,0,z);
            OutlineTube(t,"Wooden stake",new[]{V(0,0,0),V(.035f,height*.6f,0),V(-.025f,height,0)},.025f,"Wood");
            SkullAt(t,V(-.025f,height,0),.67f);Rod("Bone tied across stake",t,V(-.15f,height*.65f,-.035f),V(.16f,height*.72f,-.035f),.025f,"Cream");
        }
        static void Rat(Transform parent,float x,float z)
        {
            var t=Group("Little blue field mouse",parent);t.localPosition=V(x,.04f,z);t.localRotation=Quaternion.Euler(0,Rand(-40,40),0);
            Sphere("Mouse body",t,V(0,.14f,0),V(.42f,.26f,.25f),"StoneDark");
            Sphere("Mouse head",t,V(-.2f,.15f,-.015f),V(.25f,.23f,.23f),"StoneTop");
            Sphere("Round ear",t,V(-.15f,.28f,-.045f),V(.13f,.13f,.08f),"Stone");
            Sphere("Dot eye",t,V(-.25f,.18f,-.105f),V(.035f,.04f,.025f),"Ink",false);
            OutlineTube(t,"Curling mouse tail",new[]{V(.16f,.12f,0),V(.4f,.09f,.06f),V(.58f,.09f,-.02f),V(.61f,.11f,-.16f)},.016f,"Wood");
        }
        static void Bones(Transform parent,float x,float z)
        {
            var t=Group("Scattered ivory bones",parent);t.localPosition=V(x,.08f,z);t.localRotation=Quaternion.Euler(0,25,0);
            for(int i=0;i<2;i++)
            {
                Vector3 a=V(-.27f,.02f,i*.18f),b=V(.27f,.02f,-i*.18f);
                Rod("Old bone",t,a,b,.035f,"Cream");
                foreach(var p in new[]{a,b}){Sphere("Bone end",t,p+V(0,0,.028f),V(.09f,.08f,.07f),"Cream");Sphere("Bone end",t,p-V(0,0,.028f),V(.09f,.08f,.07f),"Cream");}
            }
        }
        static void Candle(Transform parent,float x,float y,float z)
        {
            Part("Small brass candlestick",parent,"cylinder",V(x,y+.045f,z),V(.21f,.09f,.21f),"Gold");
            Part("Ivory candle",parent,"cylinder",V(x,y+.24f,z),V(.11f,.35f,.11f),"Cream");
            Part("Still gold flame",parent,"cone",V(x,y+.42f,z),V(.10f,.20f,.10f),"Gold");
        }
    }
}
