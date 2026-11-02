using System;
using UnityEngine;

namespace Inkwood.Editor
{
    public static partial class InkwoodSceneBuilder
    {
        // Ground anchors are measured against the supplied 1740 x 703 illustration.
        // Inverting the orthographic projection preserves its layout in a genuine 3D clearing.
        static Vector3 ReferencePoint(float x,float y)
        {
            float sx=(x/1740f-.5f)*38.4f, sy=(.5f-y/703f)*16f;
            float pitch=43*Mathf.Deg2Rad, yaw=20*Mathf.Deg2Rad;
            var right=V(Mathf.Cos(yaw),0,-Mathf.Sin(yaw));
            var forward=V(Mathf.Sin(yaw),0,Mathf.Cos(yaw));
            return right*sx+forward*((sy+1.2f*Mathf.Cos(pitch))/Mathf.Sin(pitch));
        }
        static void RefProp(Action<Transform,float,float> build,Transform parent,float x,float y)
        {
            var p=ReferencePoint(x,y);build(parent,p.x,p.z);
        }
        static void RefTree(Transform parent,float x,float y,float scale,int kind)
        {
            var p=ReferencePoint(x,y);ReferenceTree(parent,p.x,p.z,scale,kind);
        }
        static void RefWall(Transform parent,float ax,float ay,float bx,float by)
        {
            var a=ReferencePoint(ax,ay);var b=ReferencePoint(bx,by);var d=b-a;
            Wall(parent,(a.x+b.x)*.5f,(a.z+b.z)*.5f,d.magnitude,Mathf.Atan2(-d.z,d.x)*Mathf.Rad2Deg);
        }
        static void RefCharacter(GameObject prefab,Transform parent,float x,float y,float yaw,float scale=1)
        {
            PlacePrefab(prefab,parent,ReferencePoint(x,y),yaw,scale);
        }
        static void BuildReferenceComposition()
        {
            var forest=Group("02 · Forest fringe",null);
            RefTree(forest,12,95,1.2f,1);RefTree(forest,160,80,1.2f,1);
            RefTree(forest,290,290,1.2f,0);RefTree(forest,410,280,1.3f,2);
            RefTree(forest,565,335,1.25f,0);RefTree(forest,660,335,1.0f,4);
            RefTree(forest,685,150,1.35f,0);RefTree(forest,800,120,.95f,4);
            RefTree(forest,1125,20,1.1f,0);RefTree(forest,1300,85,.85f,2);
            RefTree(forest,1680,245,1.3f,2);RefTree(forest,1775,65,.95f,4);
            RefTree(forest,1605,740,1.55f,3);RefTree(forest,575,760,1.1f,3);
            RefTree(forest,151,290,.9f,3);RefTree(forest,405,280,.8f,3);
            RefTree(forest,45,665,1.05f,4);RefTree(forest,130,734,.9f,2);
            RefTree(forest,104,137,.60f,5);RefTree(forest,514,293,.58f,5);
            RefTree(forest,240,737,1.2f,5);RefTree(forest,455,783,1.6f,5);

            var ruins=Group("03 · Blue stone ruins",null);
            RefWall(ruins,285,298,444,354);RefWall(ruins,450,362,547,401);RefWall(ruins,557,422,697,470);
            RefWall(ruins,1145,77,1325,145);RefWall(ruins,1420,96,1560,40);
            RefWall(ruins,950,556,1115,617);RefWall(ruins,1120,620,1285,680);
            RefWall(ruins,135,610,340,680);RefWall(ruins,350,690,530,758);
            RefWall(ruins,730,665,887,606);RefWall(ruins,1630,337,1800,400);
            var portal=ReferencePoint(819,535);Dolmen(ruins,portal.x,portal.z);
            ruins.GetChild(ruins.childCount-1).localScale=V(1.3f,1.65f,1.3f);
            RefProp((t,x,z)=>RuinedArch(t,x,z,1.4f,0),ruins,856,282);
            ruins.GetChild(ruins.childCount-1).localScale=V(1.5f,2.05f,1.4f);
            RefProp((t,x,z)=>RuinedArch(t,x,z,1.1f,25),ruins,1590,561);
            RefProp((t,x,z)=>Sarcophagus(t,x,z,52),ruins,1100,705);
            ruins.GetChild(ruins.childCount-1).localScale=Vector3.one*1.25f;
            RefProp((t,x,z)=>Sarcophagus(t,x,z,52),ruins,800,728);
            RefProp(Fountain,ruins,1301,410);
            RefProp((t,x,z)=>Bush(t,x,z,1.05f),forest,579,396);
            RefProp((t,x,z)=>Bush(t,x,z,.9f),forest,660,407);
            RefProp((t,x,z)=>Bush(t,x,z,.95f),forest,535,357);
            RefProp((t,x,z)=>Bush(t,x,z,.8f),forest,715,358);
            RefProp((t,x,z)=>Bush(t,x,z,.9f),forest,1530,540);

            var heroes=Group("04 · Adventurers",null);
            RefCharacter(Wizard(),heroes,246,504,-8,1.08f);
            RefCharacter(Knight(),heroes,403,507,-15,1.05f);
            RefCharacter(Ranger(),heroes,205,382,8,1.05f);
            RefCharacter(Cleric(),heroes,341,448,-5,1.05f);
            RefCharacter(Monk(),heroes,82,463,-15,1);
            RefCharacter(Skeleton(),heroes,975,335,-10,.95f);

            var enemies=Group("05 · Goblin gathering",null);
            var swordsman=Goblin("Goblin swordsman",0);var shield=Goblin("Goblin shieldbearer",1);
            var archer=Goblin("Goblin archer",2);var shaman=Goblin("Goblin banner shaman",3);
            RefCharacter(swordsman,enemies,1038,299,-20);
            RefCharacter(shield,enemies,1138,349,10);
            RefCharacter(archer,enemies,1306,260,-25);
            RefCharacter(swordsman,enemies,1450,301,-10);
            RefCharacter(shield,enemies,1056,478,-20);
            RefCharacter(shield,enemies,1203,502,5);
            RefCharacter(archer,enemies,1387,499,15);
            RefCharacter(shaman,enemies,1510,453,-15,1.05f);

            var props=Group("06 · Camp & little discoveries",null);
            RefProp(Tent,props,-8,289);
            RefProp((t,x,z)=>Chest(t,x,z,55,false),props,928,183);
            RefProp((t,x,z)=>Chest(t,x,z,50,true),props,1002,133);
            RefProp((t,x,z)=>Chest(t,x,z,52,false),props,900,246);
            RefProp(Barrel,props,1160,129);RefProp(Barrel,props,1220,144);RefProp(Barrel,props,1125,203);
            RefProp((t,x,z)=>Crate(t,x,z,1.05f),props,1544,64);
            RefProp((t,x,z)=>Crate(t,x,z,.85f),props,1615,47);
            RefProp((t,x,z)=>Crate(t,x,z,1),props,1700,8);
            RefProp(Campfire,props,1542,169);
            RefProp(Stump,props,1471,230);RefProp(Stump,props,1505,128);RefProp(Stump,props,1600,218);
            RefProp((t,x,z)=>Mushrooms(t,x,z,10),props,956,582);
            RefProp((t,x,z)=>Mushrooms(t,x,z,5),props,714,446);
            RefProp((t,x,z)=>Mushrooms(t,x,z,5),props,1450,660);
            RefProp((t,x,z)=>SkullStake(t,x,z,1.5f),props,720,479);
            RefProp((t,x,z)=>SkullStake(t,x,z,1.25f),props,830,568);
            RefProp((t,x,z)=>SkullStake(t,x,z,1.2f),props,937,363);
            RefProp((t,x,z)=>Skull(t,x,z,.75f),props,672,471);
            RefProp((t,x,z)=>Skull(t,x,z,.75f),props,1194,720);
            RefProp((t,x,z)=>Skull(t,x,z,.60f),props,1097,645);
            RefProp((t,x,z)=>SwordOnGround(t,x,z,75),props,990,706);
            RefProp((t,x,z)=>SwordOnGround(t,x,z,20),props,252,434);
            RefProp(ThornPatch,props,1383,651);RefProp(ThornPatch,props,1501,704);
            RefProp(Rat,props,877,632);RefProp(Rat,props,1390,696);
            RefProp(Bones,props,1080,684);RefProp(Bones,props,864,265);
            RefProp((t,x,z)=>Candle(t,x,.85f,z),props,1657,303);
            RefProp((t,x,z)=>Candle(t,x,.85f,z),props,1690,318);
            RefProp((t,x,z)=>Candle(t,x,.85f,z),props,1728,334);
            RefProp((t,x,z)=>RocksAt(t,x,z),props,252,299);
            RefProp((t,x,z)=>RocksAt(t,x,z),props,1453,70);

            var details=Group("07 · Meadow marks",null);
            for(int i=0;i<108;i++)
            {
                float x=Rand(0,1740), y=Rand(70,760);
                if(x>710&&x<925&&y>310&&y<530)continue;
                if(x>1175&&x<1420&&y>300&&y<430)continue;
                var p=ReferencePoint(x,y);
                if(i%5==0)Rock(details,p.x,p.z,Rand(.18f,.34f));
                else if(i%3==0)Sprout(details,p.x,p.z,Rand(.45f,.8f));
                else Grass(details,p.x,p.z,Rand(.3f,.6f));
            }
        }
    }
}
