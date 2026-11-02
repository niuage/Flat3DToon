using UnityEngine;

namespace Inkwood.Editor
{
    public static partial class InkwoodSceneBuilder
    {
        static void Hand(Transform t,Vector3 p,string color="Skin",float size=.14f) => Sphere("Mitten hand",t,p,V(size,size,size),color);
        static void Book(Transform t,Vector3 p,float scale=1)
        {
            var bag=Group("Little leather spellbook",t);bag.localPosition=p;bag.localRotation=Quaternion.Euler(0,-8,-12);bag.localScale=Vector3.one*scale;
            Box("Cream page block",bag,Vector3.zero,V(.27f,.36f,.12f),"Cream");
            Box("Front leather cover",bag,V(0,0,-.075f),V(.32f,.4f,.035f),"WoodDark");
            Box("Book spine",bag,V(-.15f,0,0),V(.045f,.4f,.18f),"Leather");
            Box("Brass clasp",bag,V(.12f,.02f,-.1f),V(.09f,.08f,.025f),"Gold");
        }
        static void Satchel(Transform t,Vector3 p,float scale=1)
        {
            var bag=Group("Small traveling satchel",t);bag.localPosition=p;bag.localScale=Vector3.one*scale;
            Box("Satchel body",bag,Vector3.zero,V(.32f,.3f,.22f),"Leather");
            Box("Folded flap",bag,V(0,.09f,-.13f),V(.34f,.16f,.045f),"WoodLight");
            Sphere("Clasp",bag,V(0,.02f,-.16f),V(.06f,.07f,.025f),"Gold",false);
        }
        static GameObject Wizard()
        {
            var t=Character("Wizard");Legs(t);
            Part("Single flowing blue robe",t,"robe",V(0,.29f,0),V(.95f,1,1),"Robe");
            Sphere("Small face",t,V(0,1.47f,-.03f),V(.49f,.53f,.43f),"Skin");
            var hat=Part("Bent pointed wizard hat",t,"bentHat",V(0,1.66f,0),V(1,1.05f,1),"Robe");
            hat.transform.localRotation=Quaternion.Euler(0,0,8);
            Arc(hat.transform,V(0,.055f,0),.32f,200,277,.013f,"Ink",true);
            Part("Long pointed ivory beard",t,"beard",V(.015f,1.15f,-.29f),V(.9f,1.18f,.7f),"Beard");
            Eyes(t,1.51f,-.25f,.09f);
            Sphere("Nose",t,V(.015f,1.41f,-.3f),V(.13f,.15f,.16f),"Skin");
            Sphere("Left mustache",t,V(-.065f,1.35f,-.34f),V(.17f,.075f,.1f),"Beard");
            Sphere("Right mustache",t,V(.08f,1.35f,-.34f),V(.17f,.075f,.1f),"Beard");
            Stroke(t,.013f,V(.045f,1.15f,-.43f),V(.025f,1.02f,-.43f));
            Rod("Raised wide sleeve",t,V(-.2f,1.15f,0),V(-.43f,1.4f,-.025f),.115f,"Robe");Hand(t,V(-.47f,1.44f,-.025f));
            Rod("Staff hand sleeve",t,V(.18f,1.13f,.02f),V(.33f,.94f,.12f),.105f,"Robe");Hand(t,V(.36f,.91f,.12f));
            OutlineTube(t,"Crooked staff",new[]{V(.84f,.16f,.14f),V(.17f,1.2f,.14f),V(-.55f,2.34f,.14f)},.032f,"Wood");
            OutlineTube(t,"Staff fork",new[]{V(-.37f,2.06f,.14f),V(-.58f,2.13f,.14f),V(-.73f,2.29f,.14f)},.022f,"Wood");
            Part("Tiny leaf tied to staff",t,"leaf",V(-.54f,2.31f,.14f),V(.28f,.3f,.12f),"Moss").transform.localRotation=Quaternion.Euler(0,0,-35);
            Stroke(t,.015f,V(-.24f,.64f,-.3f),V(-.2f,.84f,-.3f));
            Book(t,V(.36f,.64f,.07f),.88f);
            return SavePrefab(t,"Wizard");
        }
        static GameObject Knight()
        {
            var t=Character("Bucket knight");Legs(t);
            Part("Short blue surcoat",t,"robe",V(0,.26f,0),V(.73f,.75f,.72f),"Robe");
            Part("Leather belt",t,"torus",V(0,.69f,0),V(.25f,.22f,.25f),"WoodDark");
            Part("Closed bucket helm",t,"cylinder",V(0,1.39f,0),V(.64f,.7f,.60f),"Leather");
            Part("Inset helmet top",t,"cylinder",V(0,1.76f,0),V(.64f,.045f,.60f),"WoodLight");
            Arc(t,V(0,1.788f,0),.245f,30,230,.012f,"Ink",true);
            Stroke(t,.024f,V(-.27f,1.43f,-.245f),V(-.13f,1.41f,-.28f),V(.02f,1.41f,-.3f));
            Stroke(t,.024f,V(.09f,1.41f,-.3f),V(.25f,1.43f,-.25f));
            Stroke(t,.016f,V(.04f,1.36f,-.308f),V(.04f,1.11f,-.308f));
            Rod("Shield arm",t,V(-.18f,1.02f,0),V(-.36f,.86f,-.16f),.07f,"Robe");
            var shield=Part("Bowed plain kite shield",t,"shield",V(-.31f,.74f,-.36f),V(.96f,.94f,1),"Leather");
            shield.transform.localRotation=Quaternion.Euler(0,-10,-8);
            Sphere("Shield top rivet",shield.transform,V(0,.23f,-.13f),V(.05f,.05f,.025f),"WoodLight",false);
            Rod("Axe arm",t,V(.18f,1.02f,0),V(.44f,1.02f,-.02f),.075f,"Robe");Hand(t,V(.45f,1.03f,-.02f));
            Rod("Axe haft",t,V(.46f,.46f,0),V(.46f,1.61f,0),.029f,"Wood");
            Part("Bearded axe blade",t,"axeHead",V(.47f,1.45f,0),Vector3.one,"Rose");
            Satchel(t,V(.25f,.55f,.2f),.65f);
            return SavePrefab(t,"BucketKnight");
        }
        static GameObject Ranger()
        {
            var t=Character("Little ranger");Legs(t);
            Part("Blue ranger tunic",t,"robe",V(0,.3f,0),V(.75f,.77f,.76f),"Robe");
            Sphere("Round face",t,V(0,1.4f,0),V(.53f,.52f,.47f),"Skin");Eyes(t,1.43f,-.226f,.09f);
            Sphere("Soft blue cap",t,V(-.025f,1.62f,.02f),V(.63f,.31f,.52f),"Robe");
            Part("Long cream feather",t,"leaf",V(.1f,1.69f,0),V(.34f,.48f,.1f),"Cream").transform.localRotation=Quaternion.Euler(0,0,-48);
            Stroke(t,.012f,V(.15f,1.74f,-.01f),V(.29f,1.87f,-.01f));
            OutlineTube(t,"Chest strap",new[]{V(.2f,1.18f,-.15f),V(.03f,.94f,-.22f),V(-.17f,.64f,-.27f)},.032f,"Leather");
            Rod("Bow sleeve",t,V(-.21f,1.09f,0),V(-.49f,1.07f,-.08f),.085f,"Robe");Hand(t,V(-.6f,1.05f,-.1f));
            Arc(t,V(-.32f,1.02f,-.1f),.34f,110,250,.027f,"Wood");
            Stroke(t,.011f,V(-.436f,1.34f,-.1f),V(-.436f,.7f,-.1f));
            Rod("Quiver",t,V(.26f,.83f,.23f),V(.4f,1.44f,.23f),.12f,"Leather");
            for(int i=0;i<3;i++)
            {
                Vector3 end=V(.37f+i*.07f,1.77f+(.03f*i),.23f);
                Rod("Arrow shaft",t,V(.3f+i*.055f,1.25f,.23f),end,.012f,"WoodDark",false);
                Part("Ivory arrow fletching",t,"leaf",end-V(.02f,.1f,0),V(.16f,.18f,.055f),"Cream").transform.localRotation=Quaternion.Euler(0,0,-20);
            }
            Satchel(t,V(-.25f,.63f,.13f),.7f);
            return SavePrefab(t,"Ranger");
        }
        static GameObject Cleric()
        {
            var t=Character("White turban pilgrim");Legs(t);
            Part("Ivory traveling robe",t,"robe",V(0,.26f,0),V(.72f,.85f,.74f),"Cream");
            Sphere("Head",t,V(0,1.42f,0),V(.47f,.49f,.43f),"Skin");
            Sphere("White wrapped turban",t,V(0,1.69f,0),V(.69f,.49f,.58f),"Beard");
            Stroke(t,.013f,V(-.28f,1.77f,-.15f),V(-.12f,1.66f,-.28f),V(.08f,1.61f,-.28f));
            Stroke(t,.013f,V(-.06f,1.85f,-.22f),V(.12f,1.79f,-.25f),V(.28f,1.69f,-.15f));
            Sphere("Gem setting",t,V(-.12f,1.71f,-.294f),V(.13f,.15f,.045f),"WoodDark");
            Sphere("Rose turban gem",t,V(-.12f,1.71f,-.324f),V(.083f,.10f,.035f),"Rose",false);
            Eyes(t,1.44f,-.217f,.085f);
            Sphere("Small nose",t,V(0,1.37f,-.25f),V(.12f,.13f,.13f),"Skin");
            Part("Pointed little beard",t,"beard",V(0,1.19f,-.25f),V(.68f,.61f,.5f),"WoodDark");
            Box("Left brown vest panel",t,V(-.15f,.97f,-.13f),V(.13f,.42f,.12f),"WoodDark");
            Box("Right brown vest panel",t,V(.15f,.97f,-.13f),V(.13f,.42f,.12f),"WoodDark");
            Rod("Pilgrim staff",t,V(.4f,.1f,0),V(.4f,1.36f,0),.026f,"Wood");Hand(t,V(.4f,.96f,0));
            OutlineTube(t,"Satchel shoulder strap",new[]{V(-.17f,1.12f,.02f),V(.05f,.81f,-.24f),V(.28f,.59f,-.14f)},.022f,"Leather");
            Satchel(t,V(.30f,.61f,.03f),.68f);
            return SavePrefab(t,"Pilgrim");
        }
        static GameObject Monk()
        {
            var t=Character("Hooded monk");
            Part("Simple brown habit",t,"robe",V(0,.1f,0),V(.94f,1.1f,.97f),"Leather");
            Sphere("Hair and hood",t,V(0,1.42f,0),V(.61f,.6f,.55f),"WoodDark");
            Sphere("Face",t,V(0,1.42f,-.21f),V(.44f,.43f,.20f),"Skin");Eyes(t,1.47f,-.31f,.08f);
            for(int i=0;i<3;i++)Sphere("Short fringe",t,V(-.12f+i*.12f,1.63f,-.26f),V(.14f,.11f,.09f),"WoodDark",false);
            Sphere("Round beard",t,V(0,1.3f,-.31f),V(.29f,.25f,.12f),"WoodDark");
            Part("Folded shoulder cowl",t,"torus",V(0,1.04f,0),V(.38f,.52f,.32f),"Leather");
            Part("Rope belt",t,"torus",V(0,.67f,0),V(.34f,.3f,.34f),"WoodLight");
            Sphere("Rope knot",t,V(.12f,.66f,-.34f),V(.1f,.12f,.08f),"WoodLight");
            Curve(t,"Hanging rope",new[]{V(.12f,.63f,-.35f),V(.13f,.44f,-.38f),V(.19f,.29f,-.34f)},.021f,"WoodLight");
            Rod("Left sleeve",t,V(-.24f,1.03f,0),V(-.48f,.91f,-.04f),.10f,"Leather");Hand(t,V(-.53f,.89f,-.05f));
            Rod("Right sleeve",t,V(.24f,1.03f,0),V(.48f,.91f,-.04f),.10f,"Leather");Hand(t,V(.53f,.89f,-.05f));
            Stroke(t,.014f,V(-.19f,.25f,-.34f),V(-.17f,.42f,-.36f));
            Satchel(t,V(-.3f,.58f,.12f),.7f);
            return SavePrefab(t,"Monk");
        }
        static void FlowerShield(Transform t,Vector3 center,float diameter)
        {
            Part("Rose flower buckler",t,"cylinder",center,V(diameter,.085f,diameter),"Rose").transform.localRotation=Quaternion.Euler(90,0,0);
            for(int i=0;i<6;i++)
            {
                float a=i*Mathf.PI/3;
                var petal=Sphere("Ivory flower petal",t,center+V(Mathf.Sin(a)*diameter*.17f,Mathf.Cos(a)*diameter*.17f,-.064f),V(diameter*.13f,diameter*.23f,.035f),"Cream",false);
                petal.transform.localRotation=Quaternion.Euler(0,0,-i*60);
            }
            Sphere("Flower center",t,center+V(0,0,-.082f),V(diameter*.15f,diameter*.15f,.035f),"WoodDark",false);
        }
        static GameObject Goblin(string name,int kind)
        {
            var t=Character(name);
            OutlineTube(t,"Bent left leg",new[]{V(-.1f,.54f,0),V(-.17f,.28f,.025f),V(-.11f,.1f,-.08f)},.029f,"WoodDark");
            OutlineTube(t,"Bent right leg",new[]{V(.1f,.54f,0),V(.17f,.32f,.08f),V(.23f,.15f,-.06f)},.029f,"WoodDark");
            Part("Small leather shirt",t,"robe",V(0,.39f,0),V(.57f,kind==3?.8f:.61f,.58f),kind==3?"Robe":"Leather");
            Sphere("Small pointed goblin head",t,V(0,1.27f,-.02f),V(.54f,.46f,.45f),"Goblin");
            var ear=Part("Swept right ear",t,"ear",V(.24f,1.34f,0),V(.76f,.76f,.8f),"Goblin");ear.transform.localRotation=Quaternion.Euler(0,-25,-8);
            var ear2=Part("Swept left ear",t,"ear",V(-.24f,1.34f,0),V(.76f,.76f,.8f),"Goblin");ear2.transform.localRotation=Quaternion.Euler(0,205,8);
            Part("Pink ear inset",ear.transform,"ear",V(.015f,.03f,-.062f),V(.46f,.46f,.18f),"Rose",false);
            Part("Pink ear inset",ear2.transform,"ear",V(.015f,.03f,.062f),V(.46f,.46f,.18f),"Rose",false);
            var nose=Part("Long tapered goblin nose",t,"nose",V(-.035f,1.23f,-.17f),V(1,1,1),"Goblin");
            nose.transform.localRotation=Quaternion.FromToRotation(Vector3.up,V(0,-.12f,-1));
            Eyes(t,1.34f,-.226f,.10f);
            Stroke(t,.013f,V(-.15f,1.40f,-.19f),V(-.075f,1.37f,-.23f));
            Stroke(t,.013f,V(.075f,1.37f,-.23f),V(.145f,1.41f,-.19f));
            Stroke(t,.012f,V(-.12f,1.13f,-.27f),V(.09f,1.12f,-.29f));
            Part("One ivory tooth",t,"cone",V(.08f,1.09f,-.29f),V(.07f,.09f,.07f),"Cream",false);
            Rod("Left stick arm",t,V(-.16f,.94f,0),V(-.44f,.81f,-.08f),.032f,"GoblinDark",false);
            Rod("Right stick arm",t,V(.16f,.94f,0),V(.41f,.88f,-.07f),.032f,"GoblinDark",false);
            Part("Dark belt",t,"torus",V(0,.67f,0),V(.205f,.2f,.205f),"WoodDark");
            if(kind==3)
            {
                Part("Blue pointed cap",t,"bentHat",V(0,1.45f,.04f),V(.66f,.6f,.65f),"Robe");
                Rod("Banner staff",t,V(.48f,.08f,0),V(.48f,2.3f,0),.028f,"Wood");Hand(t,V(.45f,.98f,-.02f),"Goblin",.11f);
                Box("Tiny brown banner",t,V(.16f,1.91f,0),V(.56f,.63f,.045f),"WoodDark");
                SkullAt(t,V(.16f,1.87f,-.075f),.4f);
                Book(t,V(-.28f,.64f,.1f),.65f);
            }
            else if(kind==2)
            {
                Rod("Bow forearm",t,V(.4f,.88f,-.07f),V(.58f,1,-.1f),.032f,"GoblinDark",false);Hand(t,V(.6f,1,-.1f),"Goblin",.11f);
                Arc(t,V(.25f,1,-.1f),.35f,-80,80,.024f,"Wood");Stroke(t,.009f,V(.31f,1.345f,-.1f),V(.31f,.655f,-.1f));
                Rod("Nocked arrow",t,V(.12f,1.01f,-.13f),V(.96f,1.01f,-.13f),.012f,"WoodDark",false);
                Rod("Small back quiver",t,V(-.22f,.75f,.16f),V(-.3f,1.22f,.19f),.075f,"WoodDark");
            }
            else
            {
                Hand(t,V(-.48f,.80f,-.1f),"Goblin",.11f);
                Rod("Sword grip",t,V(-.47f,.78f,-.1f),V(-.61f,.99f,-.1f),.028f,"Wood");
                Vector3 a=V(-.61f,.99f,-.1f),b=V(-.99f,1.43f,-.1f);
                var blade=Box("Short pale sword",t,(a+b)*.5f,V(.105f,(b-a).magnitude,.045f),"Cream");blade.transform.localRotation=Quaternion.FromToRotation(Vector3.up,b-a);
                Rod("Sword guard",t,V(-.71f,.9f,-.1f),V(-.51f,1.08f,-.1f),.022f,"WoodDark");
                FlowerShield(t,V(.39f,.81f,-.25f),kind==1?.54f:.43f);
            }
            return SavePrefab(t,name.Replace(" ",""));
        }
        static void SkullAt(Transform parent,Vector3 position,float scale)
        {
            var t=Group("Ivory skull",parent);t.localPosition=position;t.localScale=Vector3.one*scale;
            Sphere("Skull cranium",t,V(0,.15f,0),V(.49f,.46f,.4f),"Cream");
            for(int i=0;i<3;i++)Box("Tiny tooth",t,V(-.1f+i*.1f,-.05f,-.08f),V(.085f,.13f,.14f),"Cream");
            Sphere("Left socket",t,V(-.11f,.17f,-.185f),V(.115f,.15f,.04f),"Ink",false);
            Sphere("Right socket",t,V(.11f,.17f,-.185f),V(.115f,.15f,.04f),"Ink",false);
            Sphere("Nose socket",t,V(0,.065f,-.2f),V(.05f,.06f,.025f),"Ink",false);
        }
        static GameObject Skeleton()
        {
            var t=Character("Little skeleton guard");SkullAt(t,V(0,1.4f,0),1);
            Rod("Spine",t,V(0,.57f,0),V(0,1.27f,0),.034f,"Cream");
            for(int i=0;i<3;i++)
            {
                float y=.85f+i*.13f;
                Arc(t,V(-.025f,y,-.01f),.17f,180,310,.022f,"Cream");
                Arc(t,V(.025f,y,-.01f),.17f,230,360,.022f,"Cream");
            }
            OutlineTube(t,"Left bone leg",new[]{V(-.08f,.63f,0),V(-.15f,.35f,.03f),V(-.13f,.08f,-.03f)},.028f,"Cream");
            OutlineTube(t,"Right bone leg",new[]{V(.08f,.63f,0),V(.21f,.35f,.07f),V(.28f,.12f,-.04f)},.028f,"Cream");
            Rod("Left arm",t,V(-.12f,1.14f,0),V(-.43f,.93f,-.02f),.028f,"Cream");
            Rod("Right arm",t,V(.12f,1.14f,0),V(.4f,.97f,0),.028f,"Cream");
            Box("Small blue shield",t,V(-.43f,.92f,-.13f),V(.36f,.53f,.08f),"Stone").transform.localRotation=Quaternion.Euler(0,-15,-12);
            Rod("Spear haft",t,V(.44f,.05f,.05f),V(.44f,2.05f,.05f),.025f,"Wood");
            Part("Spear tip",t,"cone",V(.44f,2.03f,.05f),V(.14f,.36f,.14f),"StoneTop");
            return SavePrefab(t,"SkeletonGuard");
        }
    }
}
