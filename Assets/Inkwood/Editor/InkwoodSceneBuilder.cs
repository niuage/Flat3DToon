using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace Inkwood.Editor
{
    /// <summary>Deterministic, editable geometry; no runtime generation or external art dependencies.</summary>
    public static partial class InkwoodSceneBuilder
    {
        const string Root = "Assets/Inkwood";
        const string ScenePath = Root + "/Scenes/InkwoodGlade.unity";
        static readonly Dictionary<string, Material> palette = new Dictionary<string, Material>();
        static readonly Dictionary<string, Mesh> meshes = new Dictionary<string, Mesh>();
        static System.Random random;
        static Transform library;
        static Material ink;
        static int strokeId;

        [MenuItem("Inkwood/Build Reference Diorama")]
        public static void Build()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play Mode before rebuilding.");
            for (int i = 0; i < SceneManager.sceneCount; i++)
                if (SceneManager.GetSceneAt(i).isDirty)
                    throw new InvalidOperationException("Save every open scene before rebuilding.");
            random = new System.Random(8023);
            strokeId = 0;
            palette.Clear(); meshes.Clear();
            foreach (string folder in new[] { "Materials", "Meshes", "Prefabs", "Scenes", "Settings" })
                Directory.CreateDirectory(Root + "/" + folder);
            AssetDatabase.Refresh();
            MakePalette();
            ConfigureRendering();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Inkwood Glade";
            library = Group("Reusable models", null);

            MakeMeshLibrary();
            var ground = Group("01 · Meadow", null);
            Part("Pale chartreuse meadow", ground, "box", V(0,-0.17f,0), V(200,0.3f,200), "Meadow", false);

            BuildReferenceComposition();

            var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(DioramaCamera));
            cameraObject.tag="MainCamera";
            var camera=cameraObject.GetComponent<Camera>();
            camera.clearFlags=CameraClearFlags.SolidColor;
            camera.backgroundColor=palette["Meadow"].GetColor("_BaseColor");
            camera.nearClipPlane=.1f; camera.farClipPlane=150;
            camera.allowHDR=false; camera.allowMSAA=true;
            var cameraData=camera.GetUniversalAdditionalCameraData();
            cameraData.renderPostProcessing=true;
            cameraData.antialiasing=AntialiasingMode.SubpixelMorphologicalAntiAliasing;
            cameraData.antialiasingQuality=AntialiasingQuality.High;
            var volumeObject=new GameObject("Palette preservation",typeof(Volume));
            var volume=volumeObject.GetComponent<Volume>(); volume.isGlobal=true; volume.priority=100;
            string profilePath=Root+"/Settings/PaletteProfile.asset";
            var profile=AssetDatabase.LoadAssetAtPath<VolumeProfile>(profilePath);
            if(!profile) { profile=ScriptableObject.CreateInstance<VolumeProfile>(); AssetDatabase.CreateAsset(profile,profilePath); }
            if(!profile.TryGet<Tonemapping>(out var tone)) { tone=profile.Add<Tonemapping>(true); AssetDatabase.AddObjectToAsset(tone,profile); }
            tone.mode.Override(TonemappingMode.None);
            if(!profile.TryGet<Bloom>(out var bloom)) { bloom=profile.Add<Bloom>(true); AssetDatabase.AddObjectToAsset(bloom,profile); }
            bloom.intensity.Override(0);
            if(!profile.TryGet<Vignette>(out var vignette)) { vignette=profile.Add<Vignette>(true); AssetDatabase.AddObjectToAsset(vignette,profile); }
            vignette.intensity.Override(0);
            volume.sharedProfile=profile; EditorUtility.SetDirty(profile);
            var orbit=cameraObject.GetComponent<DioramaCamera>();
            orbit.target=V(0,1.2f,0); orbit.homeYaw=20; orbit.elevation=43; orbit.homeSize=8;orbit.referenceAspect=2.4f;
            orbit.ResetView();
            RenderSettings.fog=false;
            RenderSettings.ambientMode=AmbientMode.Flat;
            RenderSettings.ambientLight=Color.white;
            var light=new GameObject("Soft daylight", typeof(Light));
            light.GetComponent<Light>().type=LightType.Directional;
            light.GetComponent<Light>().intensity=.6f;
            light.transform.rotation=Quaternion.Euler(55,-30,0);

            ExportEnvironmentPrefabs(scene);
            UnityEngine.Object.DestroyImmediate(library.gameObject);
            SetupLighting(scene);
            InkwoodAnimationAuthoring.EnsureDirector(scene);
            EditorSceneManager.SaveScene(scene,ScenePath);
            AssetDatabase.SaveAssets();
            var sceneView=SceneView.lastActiveSceneView;
            if(sceneView) { sceneView.LookAt(orbit.target,camera.transform.rotation,25,true,true); sceneView.Repaint(); }
            Selection.activeGameObject=cameraObject;
            Debug.Log("Inkwood Glade built and saved: " + ScenePath);
        }

        static Vector3 V(float x,float y,float z) => new Vector3(x,y,z);
        static float Rand(float min,float max) => min+(float)random.NextDouble()*(max-min);
        static Transform Group(string name,Transform parent)
        {
            var t=new GameObject(name).transform;
            t.SetParent(parent,false);
            return t;
        }
        static void MakePalette()
        {
            var colors=new Dictionary<string,string>
            {
                {"Ink","49362F"},{"Meadow","D2DC84"},{"Shadow","C3CD7A"},
                {"Stone","8EBABD"},{"StoneTop","A4CACE"},{"StoneDark","718E98"},
                {"Moss","A6B77C"},{"Leaf","95AF78"},{"LeafLight","A5BB83"},
                {"LeafDark","7E9F70"},{"Pine","77AA8B"},{"Orange","F6A16D"},
                {"Wood","99705A"},{"WoodLight","BD8D6C"},{"WoodDark","785643"},
                {"Robe","8FC1C4"},{"Cream","FFF6D8"},{"Beard","F2F3D9"},
                {"Skin","E8D9B2"},{"Goblin","9BB874"},{"GoblinDark","829F64"},
                {"Leather","B9836C"},{"Rose","CE8D87"},{"Red","D76F62"},
                {"Gold","EBCB69"},{"Water","BCE1E1"},{"WaterLight","D7F1E6"}
            };
            foreach(var pair in colors)
            {
                string path=Root+"/Materials/"+pair.Key+".mat";
                var m=AssetDatabase.LoadAssetAtPath<Material>(path);
                if(!m) { m=new Material(Shader.Find("Inkwood/Toon Color")); AssetDatabase.CreateAsset(m,path); }
                ColorUtility.TryParseHtmlString("#"+pair.Value,out Color c);
                m.shader=Shader.Find("Inkwood/Toon Color");
                m.SetColor("_BaseColor",c);
                m.SetColor("_ShadowColor",ShadowColor(pair.Key,c));
                bool unlit=pair.Key=="Meadow"||pair.Key=="Ink"||pair.Key=="Shadow"||pair.Key=="WaterLight";
                m.SetFloat("_LightingEnabled",unlit?0:1);
                m.SetFloat("_ReceiveShadows",pair.Key=="Ink"||pair.Key=="Shadow"||pair.Key=="WaterLight"?0:1);
                m.SetFloat("_BandThreshold",.22f); m.SetFloat("_BandSoftness",.012f);
                EditorUtility.SetDirty(m); palette[pair.Key]=m;
            }
            string inkPath=Root+"/Materials/Outline.mat";
            ink=AssetDatabase.LoadAssetAtPath<Material>(inkPath);
            if(!ink) { ink=new Material(Shader.Find("Inkwood/Ink Outline")); AssetDatabase.CreateAsset(ink,inkPath); }
            ink.SetColor("_BaseColor",palette["Ink"].GetColor("_BaseColor")); ink.SetFloat("_OutlinePixels",3.2f);
            EditorUtility.SetDirty(ink);
        }
        static Color ShadowColor(string name,Color fallback)
        {
            var colors=new Dictionary<string,string>
            {
                {"Meadow","B5C477"},{"Stone","6D8998"},{"StoneTop","7898A5"},{"StoneDark","556D7D"},
                {"Moss","7D965E"},{"Leaf","78965F"},{"LeafLight","819F67"},{"LeafDark","5C7D50"},
                {"Pine","548574"},{"Orange","D48154"},{"Wood","76523F"},{"WoodLight","95694D"},
                {"WoodDark","573D32"},{"Robe","64959F"},{"Cream","CFC6A0"},{"Beard","BDCCB2"},
                {"Skin","BFAA86"},{"Goblin","71914F"},{"GoblinDark","56763D"},{"Leather","94644F"},
                {"Rose","A96368"},{"Red","AA4F4C"},{"Gold","C49E4C"},{"Water","87B5C3"}
            };
            if(colors.TryGetValue(name,out string hex)) { ColorUtility.TryParseHtmlString("#"+hex,out var color);return color; }
            return fallback;
        }
        public static void UpgradeLighting()
        {
            if(EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play Mode first.");
            MakePalette(); ConfigureRendering();
            SetupLighting(SceneManager.GetActiveScene());
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            AssetDatabase.SaveAssets();
        }
        static void SetupLighting(Scene scene)
        {
            var roots=scene.GetRootGameObjects();
            var control=roots.SelectMany(g=>g.GetComponentsInChildren<DioramaLighting>(true)).FirstOrDefault();
            if(!control) control=new GameObject("08 · Toon lighting",typeof(DioramaLighting)).GetComponent<DioramaLighting>();
            var sun=roots.SelectMany(g=>g.GetComponentsInChildren<Light>(true)).FirstOrDefault(l=>l.type==LightType.Directional);
            if(!sun) sun=new GameObject("Sun",typeof(Light)).GetComponent<Light>();
            sun.name="Sun · rotate to change toon bands";
            sun.type=LightType.Directional;sun.intensity=1;sun.color=Color.white;
            sun.transform.SetParent(control.transform,true);
            sun.transform.rotation=Quaternion.Euler(50,-40,0);
            control.sun=sun;control.RefreshSurfaces();control.SetCastShadows(false);
            RenderSettings.sun=sun;
            EditorUtility.SetDirty(control);
        }
        static void Store(string key,Mesh mesh)
        {
            mesh.name=key;
            string path=Root+"/Meshes/"+key+".asset";
            var existing=AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if(existing)
            {
                // Update through the Mesh API so the renderer also invalidates cached GPU buffers.
                existing.Clear(); existing.indexFormat=mesh.indexFormat;
                existing.SetVertices(mesh.vertices); existing.SetNormals(mesh.normals);
                existing.SetTriangles(mesh.triangles,0); existing.RecalculateBounds();
                existing.UploadMeshData(false); EditorUtility.SetDirty(existing);
                UnityEngine.Object.DestroyImmediate(mesh); mesh=existing;
            }
            else AssetDatabase.CreateAsset(mesh,path);
            meshes[key]=mesh;
        }
        static void ConfigureRendering()
        {
            // A small forward renderer avoids applying deferred/GPU-driven assumptions to ink hulls.
            string rendererPath=Root+"/Settings/InkwoodRenderer.asset";
            var renderer=AssetDatabase.LoadAssetAtPath<UniversalRendererData>(rendererPath);
            if(!renderer)
            {
                renderer=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<UniversalRendererData>("Assets/Settings/PC_Renderer.asset"));
                renderer.name="InkwoodRenderer"; renderer.rendererFeatures.Clear();
                AssetDatabase.CreateAsset(renderer,rendererPath);
            }
            renderer.renderingMode=RenderingMode.Forward;
            EditorUtility.SetDirty(renderer);
            string pipelinePath=Root+"/Settings/InkwoodPipeline.asset";
            var pipeline=AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(pipelinePath);
            if(!pipeline)
            {
                pipeline=UniversalRenderPipelineAsset.Create(renderer);
                AssetDatabase.CreateAsset(pipeline,pipelinePath);
            }
            pipeline.gpuResidentDrawerMode=GPUResidentDrawerMode.Disabled;
            pipeline.msaaSampleCount=4; pipeline.supportsHDR=false;
            pipeline.supportsCameraDepthTexture=false; pipeline.supportsCameraOpaqueTexture=false;
            var pipelineSettings=new SerializedObject(pipeline);
            pipelineSettings.FindProperty("m_MainLightShadowsSupported").boolValue=true;
            pipelineSettings.ApplyModifiedPropertiesWithoutUndo();
            pipeline.mainLightShadowmapResolution=4096;
            pipeline.shadowDistance=100;pipeline.shadowCascadeCount=4;
            QualitySettings.renderPipeline=pipeline;
            EditorUtility.SetDirty(pipeline);
        }
        static void MakeMeshLibrary()
        {
            var primitive=GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Store("sphere",UnityEngine.Object.Instantiate(primitive.GetComponent<MeshFilter>().sharedMesh));
            UnityEngine.Object.DestroyImmediate(primitive);
            Store("box",RoundedBox(.10f));
            Store("cylinder",Lathe(new[]{new Vector2(0,-.5f),new Vector2(.5f,-.5f),new Vector2(.5f,.5f),new Vector2(0,.5f)},32));
            Store("robe",Lathe(new[]{new Vector2(0,0),new Vector2(.33f,.02f),new Vector2(.43f,.13f),new Vector2(.39f,.40f),new Vector2(.26f,.82f),new Vector2(.20f,1.08f),new Vector2(0,1.1f)},32));
            Store("hat",Lathe(new[]{new Vector2(0,0),new Vector2(.44f,0),new Vector2(.31f,.13f),new Vector2(.20f,.5f),new Vector2(.02f,.91f),new Vector2(0,.93f)},32));
            Store("cone",Lathe(new[]{new Vector2(0,0),new Vector2(.5f,0),new Vector2(.36f,.36f),new Vector2(.18f,.75f),new Vector2(0,1)},32));
            Store("beard",Lathe(new[]{new Vector2(0,-.48f),new Vector2(.11f,-.23f),new Vector2(.19f,.10f),new Vector2(.16f,.28f),new Vector2(0,.34f)},24));
            Store("barrel",Lathe(new[]{new Vector2(0,0),new Vector2(.42f,0),new Vector2(.49f,.22f),new Vector2(.52f,.52f),new Vector2(.46f,.95f),new Vector2(.40f,1.05f),new Vector2(0,1.05f)},32));
            Store("bowl",Lathe(new[]{new Vector2(0,-.03f),new Vector2(.22f,0),new Vector2(.43f,.12f),new Vector2(.62f,.40f),new Vector2(.64f,.48f),new Vector2(.55f,.48f),new Vector2(.51f,.38f),new Vector2(.32f,.18f),new Vector2(0,.16f)},48));
            Store("rock",RockMesh());
            Store("leaf",LeafMesh());
            Store("shield",Panel(new[]{new Vector2(-.32f,.35f),new Vector2(0,.28f),new Vector2(.32f,.35f),new Vector2(.25f,-.22f),new Vector2(0,-.52f),new Vector2(-.25f,-.22f)},.09f));
            Store("ear",Panel(new[]{new Vector2(-.12f,-.14f),new Vector2(.35f,.22f),new Vector2(.10f,.23f),new Vector2(-.14f,.11f)},.10f));
            for(int i=0;i<4;i++) Store("canopy"+i,Canopy(i));
            Store("torus",Torus(1,.085f));
            MakeDetailedMeshes();
        }
        static GameObject Part(string name,Transform parent,string mesh,Vector3 pos,Vector3 scale,string material,bool outline=true)
        {
            var go=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));
            go.transform.SetParent(parent,false); go.transform.localPosition=pos; go.transform.localScale=scale;
            go.GetComponent<MeshFilter>().sharedMesh=meshes[mesh];
            var renderer=go.GetComponent<MeshRenderer>(); renderer.sharedMaterial=palette[material];
            renderer.shadowCastingMode=ShadowCastingMode.Off; renderer.receiveShadows=false;
            if(outline)
            {
                var hull=new GameObject("Silhouette",typeof(MeshFilter),typeof(MeshRenderer));
                hull.transform.SetParent(go.transform,false); hull.GetComponent<MeshFilter>().sharedMesh=meshes[mesh];
                var hr=hull.GetComponent<MeshRenderer>(); hr.sharedMaterial=ink; hr.shadowCastingMode=ShadowCastingMode.Off; hr.receiveShadows=false;
            }
            return go;
        }
        static GameObject Sphere(string name,Transform t,Vector3 p,Vector3 s,string color,bool outline=true) => Part(name,t,"sphere",p,s,color,outline);
        static GameObject Box(string name,Transform t,Vector3 p,Vector3 s,string color) => Part(name,t,"box",p,s,color);
        static void Rod(string name,Transform t,Vector3 a,Vector3 b,float radius,string color,bool outline=true)
        {
            var go=Part(name,t,"cylinder",(a+b)*.5f,V(radius*2,(b-a).magnitude,radius*2),color,outline);
            go.transform.localRotation=Quaternion.FromToRotation(Vector3.up,b-a);
        }
        static void Stroke(Transform t,float radius,params Vector3[] points) => Curve(t,"Ink stroke",points,radius,"Ink");
        static void Curve(Transform t,string name,Vector3[] points,float radius,string color)
        {
            // Real round tubes, not camera-facing billboards: detail remains attached during orbit.
            string key="stroke"+(strokeId++).ToString("D4");
            Store(key,Tube(points,radius,6));
            Part(name,t,key,Vector3.zero,Vector3.one,color,false);
        }
        static void Arc(Transform t,Vector3 center,float radius,float from,float to,float width,string color="Ink",bool horizontal=false)
        {
            var points=new Vector3[13];
            for(int i=0;i<points.Length;i++)
            {
                float a=Mathf.Lerp(from,to,i/(float)(points.Length-1))*Mathf.Deg2Rad;
                points[i]=center+(horizontal?V(Mathf.Cos(a)*radius,0,Mathf.Sin(a)*radius):V(Mathf.Cos(a)*radius,Mathf.Sin(a)*radius,0));
            }
            Curve(t,"Broken curved detail",points,width,color);
        }
        static void Shadow(Transform t,float x,float z,float width,float depth)
        {
            Sphere("Quiet ground shadow",t,V(x,.014f,z),V(width,.014f,depth),"Shadow",false);
        }
        static GameObject SavePrefab(Transform t,string name)
        {
            InkwoodAnimationAuthoring.RigCharacter(t.gameObject,name);
            var prefab=PrefabUtility.SaveAsPrefabAsset(t.gameObject,Root+"/Prefabs/"+name+".prefab");
            UnityEngine.Object.DestroyImmediate(t.gameObject);
            return prefab;
        }
        static void PlacePrefab(GameObject prefab,Transform parent,Vector3 pos,float yaw,float scale)
        {
            var go=(GameObject)PrefabUtility.InstantiatePrefab(prefab,parent);
            go.transform.localPosition=pos; go.transform.localRotation=Quaternion.Euler(0,yaw,0); go.transform.localScale=Vector3.one*scale;
        }

        static void OutlineTube(Transform t,string name,Vector3[] points,float radius,string color)
        {
            string key="stroke"+(strokeId++).ToString("D4"); Store(key,Tube(points,radius,10));
            Part(name,t,key,Vector3.zero,Vector3.one,color);
        }
        static void Wall(Transform parent,float x,float z,float length,float yaw)
        {
            var t=Group("Broken mossy wall",parent); t.localPosition=V(x,0,z); t.localRotation=Quaternion.Euler(0,yaw,0);
            Shadow(t,0,0,length,1);
            Box("Stone foundation",t,V(0,.33f,0),V(length,.64f,.61f),"Stone");
            Box("Long capstone",t,V(-.1f,.72f,0),V(length-.25f,.26f,.76f),"StoneTop");
            Box("Broken end",t,V(-length*.5f,.42f,.08f),V(.42f,.78f,.76f),"Stone");
            for(int side=-1;side<=1;side+=2)for(int i=0;i<5;i++)
            {
                float bx=Mathf.Lerp(-length*.42f,length*.42f,i/4f);
                Stroke(t,.019f,V(bx,.12f,side*.323f),V(bx,.37f,side*.323f),V(bx+.2f,.37f,side*.323f));
            }
            Stroke(t,.022f,V(-length*.4f,.56f,-.34f),V(length*.36f,.56f,-.34f));
            Stroke(t,.020f,V(-length*.4f,.56f,.34f),V(length*.36f,.56f,.34f));
            for(int i=0;i<3;i++)
            {
                float mx=Rand(-length*.4f,length*.4f);
                Part("Scalloped moss cushion",t,"canopy2",V(mx,.88f,Rand(-.2f,.18f)),V(.42f,.06f,.28f),"Moss",false);
            }
            Rock(t,length*.42f,-.7f,.27f);
        }
        static void BrokenTower(Transform parent,float x,float z,float yaw)
        {
            var t=Group("Standing ruin & ivy",parent); t.localPosition=V(x,0,z); t.localRotation=Quaternion.Euler(0,yaw,0);
            Box("Left pier",t,V(-.9f,1.7f,0),V(.75f,3.4f,.7f),"Stone");
            Box("Right broken pier",t,V(.85f,1.05f,.05f),V(.8f,2.1f,.7f),"Stone");
            Box("Low wall",t,V(.1f,.7f,.08f),V(1.6f,1.3f,.55f),"Stone");
            Box("Uneven broken top",t,V(-1.05f,3.55f,.05f),V(.44f,.45f,.7f),"StoneTop");
            for(int i=0;i<5;i++)
            {
                float xx=i%2==0?-.9f:.85f, yy=.45f+i*.53f;
                Sphere("Climbing moss",t,V(xx,yy,-.383f),V(.66f,.45f,.08f),"Moss",false);
                Stroke(t,.021f,V(xx-.2f,yy+.15f,-.44f),V(xx-.28f,yy+.05f,-.44f),V(xx-.18f,yy-.03f,-.44f));
            }
            Stroke(t,.025f,V(-1.1f,1.7f,-.37f),V(-.87f,1.62f,-.38f),V(-.94f,1.3f,-.38f));
            Stroke(t,.024f,V(.58f,.4f,-.35f),V(.58f,.9f,-.35f),V(.73f,.96f,-.35f));
        }
        static void Dolmen(Transform parent,float x,float z)
        {
            var t=Group("Ancient standing stones",parent); t.localPosition=V(x,0,z);
            Part("Left leaning monolith",t,"pillarLeft",V(-.95f,0,0),Vector3.one,"Stone");
            Part("Right leaning monolith",t,"pillarRight",V(.95f,0,0),Vector3.one,"Stone");
            var cap=Part("Asymmetric weathered lintel",t,"lintel",V(0,2.94f,0),Vector3.one,"StoneTop");cap.transform.localRotation=Quaternion.Euler(0,-8,-4);
            Stroke(t,.027f,V(-1.06f,.25f,-.41f),V(-1.12f,1.05f,-.42f),V(-.98f,2.05f,-.4f));
            Stroke(t,.024f,V(.91f,1.5f,-.45f),V(.74f,1.1f,-.46f),V(.9f,.9f,-.46f));
            Stroke(t,.021f,V(-1.4f,3.12f,-.63f),V(-.99f,3.10f,-.72f),V(-.55f,3.02f,-.72f));
            Rock(t,1.56f,-.65f,.35f);
        }
        static void Sarcophagus(Transform parent,float x,float z,float yaw)
        {
            var t=Group("Mossy stone sarcophagus",parent); t.localPosition=V(x,0,z); t.localRotation=Quaternion.Euler(0,yaw,0);
            Box("Stone chest",t,V(0,.55f,0),V(2.5f,1.05f,1.15f),"Stone");
            Box("Overhanging lid",t,V(0,1.13f,0),V(2.8f,.23f,1.4f),"StoneDark");
            Box("Base plinth",t,V(0,.12f,0),V(2.65f,.2f,1.28f),"StoneTop");
            Stroke(t,.024f,V(-1.05f,.93f,-.59f),V(-1.05f,.68f,-.59f));
            Stroke(t,.023f,V(.88f,.95f,-.59f),V(.88f,.56f,-.59f));
            Stroke(t,.023f,V(-.84f,1.263f,-.45f),V(-.47f,1.263f,-.45f),V(-.1f,1.263f,-.45f));
            Stroke(t,.025f,V(.2f,1.263f,.1f),V(.67f,1.263f,.1f));
            for(int i=0;i<4;i++) Part("Trailing scalloped moss",t,"canopy2",V(Rand(-1.1f,1.1f),Rand(.2f,.85f),-.61f),V(.22f,.17f,.04f),"Moss",false);
            Skull(t,1.55f,-.78f,.65f); Sprout(t,-1.7f,.15f,.55f);
        }
        static void Fountain(Transform parent,float x,float z)
        {
            var t=Group("Quiet circular fountain",parent); t.localPosition=V(x,0,z);
            Shadow(t,0,0,4.5f,3.5f);
            Part("Round blue foundation",t,"cylinder",V(0,.22f,0),V(4.5f,.4f,4.5f),"StoneDark");
            Part("Still pale water",t,"cylinder",V(0,.51f,0),V(3.98f,.07f,3.98f),"Water",false);
            Part("Carved rim",t,"torus",V(0,.51f,0),V(2.12f,1.55f,2.12f),"Stone");
            for(int i=0;i<12;i++)
            {
                float a=i*Mathf.PI/6;
                Stroke(t,.023f,V(Mathf.Cos(a)*1.98f,.55f,Mathf.Sin(a)*1.98f),V(Mathf.Cos(a)*2.25f,.55f,Mathf.Sin(a)*2.25f));
                Stroke(t,.019f,V(Mathf.Cos(a)*2.254f,.07f,Mathf.Sin(a)*2.254f),V(Mathf.Cos(a)*2.254f,.31f,Mathf.Sin(a)*2.254f));
            }
            Arc(t,V(0,.21f,0),2.254f,195,265,.018f,"Ink",true);Arc(t,V(0,.21f,0),2.254f,280,345,.018f,"Ink",true);
            Part("Pedestal foot",t,"cylinder",V(0,.61f,0),V(.7f,.21f,.7f),"StoneTop");
            Part("Pedestal",t,"robe",V(0,.6f,0),V(.57f,1.49f,.57f),"Stone");
            Part("Upper basin",t,"bowl",V(0,2.12f,0),V(1.55f,1.3f,1.55f),"Stone");
            Part("Upper still water",t,"cylinder",V(0,2.64f,0),V(1.58f,.025f,1.58f),"Water");
            Arc(t,V(-.2f,.56f,-.1f),1.35f,180,260,.017f,"WaterLight",true);
            Arc(t,V(.05f,.56f,.15f),1.6f,5,72,.018f,"WaterLight",true);
            Arc(t,V(0,2.67f,0),.55f,10,130,.018f,"WaterLight",true);
        }

        static Transform Character(string name)
        {
            var t=Group(name,library); Shadow(t,0,0,.9f,.6f); return t;
        }
        static void Legs(Transform t,string color="WoodDark")
        {
            Rod("Little left leg",t,V(-.14f,.12f,0),V(-.13f,.54f,0),.041f,color,false);
            Rod("Little right leg",t,V(.17f,.1f,-.03f),V(.14f,.54f,0),.041f,color,false);
        }
        static void Eyes(Transform t,float y,float z,float spread=.105f)
        {
            Sphere("Left dot eye",t,V(-spread,y,z),V(.052f,.068f,.045f),"Ink",false);
            Sphere("Right dot eye",t,V(spread,y,z),V(.052f,.068f,.045f),"Ink",false);
        }
        // Character meshes and poses are authored in InkwoodCharacters.cs.

        static void Rock(Transform parent,float x,float z,float size)
        {
            var rock=Part("Loose blue stone",parent,"rock",V(x,size*.42f,z),V(size*1.3f,size,size),"StoneTop");
            rock.transform.localRotation=Quaternion.Euler(0,Rand(0,360),0);
        }
        static void RocksAt(Transform parent,float x,float z)
        {
            for(int i=0;i<5;i++) Rock(parent,x+Rand(-.7f,.7f),z+Rand(-.4f,.4f),Rand(.25f,.6f));
            Part("Stacked wayside stone",parent,"rock",V(x,.7f,z),V(.6f,.6f,.58f),"Stone");
            Part("Stacked wayside stone",parent,"rock",V(x,.99f,z),V(.43f,.4f,.42f),"StoneTop");
        }
        static void Grass(Transform parent,float x,float z,float size)
        {
            var t=Group("Three little grass marks",parent); t.localPosition=V(x,.025f,z);
            Stroke(t,.024f,V(-size,0,0),V(-size*.5f,.12f,0),V(-size*.18f,.02f,0));
            Stroke(t,.024f,V(-size*.18f,.02f,0),V(-size*.05f,.27f,.04f),V(size*.18f,.06f,0));
            Stroke(t,.024f,V(size*.18f,.06f,0),V(size*.55f,.13f,.03f),V(size,.01f,0));
        }
        static void Sprout(Transform parent,float x,float z,float size)
        {
            var t=Group("Three-leaf sprout",parent); t.localPosition=V(x,.03f,z); t.localScale=Vector3.one*size;
            for(int i=0;i<3;i++)
            {
                var leaf=Part("Leaf",t,"leaf",V(0,.01f,0),V(1,1,.18f),"Moss");
                leaf.transform.localRotation=Quaternion.Euler(-16,i*115,-30+i*30);
                Stroke(leaf.transform,.009f,V(0,.13f,-.10f),V(0,.4f,-.16f));
            }
        }
        static void Mushrooms(Transform parent,float x,float z,int count)
        {
            var t=Group("Coral mushroom cluster",parent); t.localPosition=V(x,0,z);
            for(int i=0;i<count;i++)
            {
                float a=i*2.39996f,spread=.92f*Mathf.Sqrt((i+.5f)/count);
                float xx=Mathf.Cos(a)*spread, zz=Mathf.Sin(a)*spread*.8f, h=Rand(.22f,.62f), r=Rand(.14f,.23f);
                Rod("Cream stem",t,V(xx,.03f,zz),V(xx,h,zz),.055f,"Cream");
                Sphere("Rounded mushroom cap",t,V(xx,h,zz),V(r*2,r*1.05f,r*2),i%3==0?"Orange":"Rose");
                for(int j=0;j<3;j++) Sphere("Cream cap dot",t,V(xx+Rand(-r*.5f,r*.5f),h+r*.48f,zz+Rand(-r*.35f,r*.35f)),V(.06f,.028f,.06f),"Cream",false);
            }
        }
        static void Skull(Transform parent,float x,float z,float scale)
        {
            var t=Group("Little ivory skull",parent); t.localPosition=V(x,.02f,z); t.localScale=Vector3.one*scale;
            Sphere("Skull",t,V(0,.24f,0),V(.52f,.48f,.4f),"Cream");
            for(int i=0;i<3;i++) Box("Tooth",t,V(-.11f+i*.11f,.08f,-.1f),V(.1f,.16f,.15f),"Cream");
            Sphere("Left eye socket",t,V(-.12f,.27f,-.18f),V(.12f,.16f,.035f),"Ink",false);
            Sphere("Right eye socket",t,V(.12f,.27f,-.18f),V(.12f,.16f,.035f),"Ink",false);
            Sphere("Nose socket",t,V(0,.17f,-.2f),V(.055f,.065f,.025f),"Ink",false);
        }
        static void Barrel(Transform parent,float x,float z)
        {
            var t=Group("Banded wooden barrel",parent); t.localPosition=V(x,0,z);
            Part("Bulging staves",t,"barrel",Vector3.zero,V(1,1.15f,1),"Gold");
            for(int i=0;i<3;i++)
                Part("Brown hoop",t,"torus",V(0,.15f+i*.45f,0),V(i==1?.52f:.46f,.6f,i==1?.52f:.46f),"WoodDark");
            Part("Barrel lid",t,"cylinder",V(0,1.22f,0),V(.8f,.08f,.8f),"Wood");
            for(int i=-1;i<=1;i++) Stroke(t,.017f,V(i*.2f,.25f,-.48f),V(i*.2f,.83f,-.48f));
            Stroke(t,.02f,V(-.3f,1.27f,0),V(.3f,1.27f,0));
        }
        static void Chest(Transform parent,float x,float z,float yaw,bool open)
        {
            var t=Group(open?"Open treasure chest":"Rose wooden chest",parent); t.localPosition=V(x,0,z); t.localRotation=Quaternion.Euler(0,yaw,0);
            Box("Chest base",t,V(0,.4f,0),V(1.4f,.75f,.82f),"Wood");
            if(open)
            {
                Box("Dark open interior",t,V(0,.8f,0),V(1.15f,.08f,.6f),"WoodDark");
                var lid=Group("Open lid",t); lid.localPosition=V(0,.77f,.4f); lid.localRotation=Quaternion.Euler(-65,0,0);
                Box("Raised lid",lid,V(0,.08f,-.39f),V(1.4f,.22f,.82f),"Rose");
                for(int i=0;i<13;i++) Sphere("Gold treasure",t,V(Rand(-.5f,.5f),Rand(.77f,.98f),Rand(-.25f,.25f)),V(.16f,.11f,.14f),"Gold");
            }
            else
            {
                Sphere("Rounded chest lid",t,V(0,.83f,0),V(1.43f,.64f,.85f),"Rose");
                for(int i=-1;i<=1;i++) Stroke(t,.025f,V(i*.36f,.8f,-.43f),V(i*.36f,1.01f,-.32f),V(i*.36f,1.15f,0));
            }
            Box("Center latch",t,V(0,.62f,-.46f),V(.15f,.27f,.08f),"Gold");
            for(int i=-1;i<=1;i+=2) Stroke(t,.026f,V(i*.5f,.12f,-.43f),V(i*.5f,.75f,-.43f));
        }
        static void Stump(Transform parent,float x,float z)
        {
            Part("Little log seat",parent,"cylinder",V(x,.26f,z),V(.5f,.5f,.48f),"Wood");
            Part("Cut wood top",parent,"cylinder",V(x,.52f,z),V(.46f,.025f,.44f),"WoodLight");
            Arc(parent,V(x,.54f,z),.13f,20,320,.018f,"Ink",true);
        }
        static void Campfire(Transform parent,float x,float z)
        {
            var t=Group("Campfire & roasting spit",parent); t.localPosition=V(x,0,z);
            for(int i=0;i<7;i++)
            {
                float a=i*Mathf.PI*2/7; Rock(t,Mathf.Cos(a)*.6f,Mathf.Sin(a)*.45f,.22f);
            }
            for(int i=0;i<3;i++)
            {
                var log=Part("Campfire log",t,"cylinder",V(0,.12f,0),V(.17f,.95f,.17f),"Wood"); log.transform.localRotation=Quaternion.Euler(90,i*65,0);
            }
            Part("Flat gold flame",t,"cone",V(0,.18f,0),V(.5f,.6f,.44f),"Gold");
            Part("Coral flame",t,"cone",V(.1f,.18f,.02f),V(.3f,.38f,.3f),"Orange");
            Rod("Left spit support",t,V(-.8f,.05f,0),V(-.8f,1.4f,0),.033f,"Wood");
            Rod("Right spit support",t,V(.8f,.05f,0),V(.8f,1.4f,0),.033f,"Wood");
            Rod("Roasting pole",t,V(-1,1.35f,0),V(1,1.35f,0),.035f,"Wood");
            Sphere("Roasting supper",t,V(0,1.36f,0),V(1,.5f,.44f),"Rose");
            Stroke(t,.022f,V(-.28f,1.52f,-.16f),V(-.16f,1.36f,-.24f));
        }
        static void Tent(Transform parent,float x,float z)
        {
            var t=Group("Golden pavilion at forest edge",parent); t.localPosition=V(x,0,z);
            Part("Canvas walls",t,"cylinder",V(0,1.25f,0),V(3.5f,2.5f,3.5f),"Gold");
            Part("Pointed tent roof",t,"cone",V(0,2.5f,0),V(4.1f,1.8f,4.1f),"Gold");
            for(int i=0;i<6;i++)
            {
                float a=i*Mathf.PI/3;
                Stroke(t,.025f,V(Mathf.Cos(a)*1.76f,.1f,Mathf.Sin(a)*1.76f),V(Mathf.Cos(a)*1.76f,2.4f,Mathf.Sin(a)*1.76f));
                Stroke(t,.025f,V(Mathf.Cos(a)*1.98f,2.6f,Mathf.Sin(a)*1.98f),V(0,4.3f,0));
            }
            Rod("Tent mast",t,V(0,4.1f,0),V(0,4.7f,0),.035f,"Wood");
        }
        static void SwordOnGround(Transform parent,float x,float z,float yaw)
        {
            var t=Group("Forgotten sword",parent); t.localPosition=V(x,.08f,z); t.localRotation=Quaternion.Euler(90,yaw,0);
            Box("Blade",t,V(0,.45f,0),V(.12f,.9f,.045f),"Cream");
            Rod("Hilt",t,V(0,-.27f,0),V(0,0,0),.035f,"Wood");
            Box("Crossguard",t,Vector3.zero,V(.42f,.06f,.07f),"WoodDark");
        }
        static void Banner(Transform parent,float x,float z)
        {
            var t=Group("Little goblin standard",parent); t.localPosition=V(x,0,z);
            Rod("Standard pole",t,V(0,0,0),V(0,2.5f,0),.035f,"Wood");
            Box("Brown banner",t,V(-.34f,2.03f,0),V(.62f,.75f,.045f),"WoodDark");
            Skull(t,-.34f,-.08f,.38f);
            Sphere("Banner skull emblem",t,V(-.34f,2.1f,-.05f),V(.28f,.27f,.05f),"Cream");
            Sphere("Emblem eye",t,V(-.4f,2.12f,-.083f),V(.05f,.07f,.02f),"Ink",false);
            Sphere("Emblem eye",t,V(-.28f,2.12f,-.083f),V(.05f,.07f,.02f),"Ink",false);
        }
        static void ThornPatch(Transform parent,float x,float z)
        {
            var t=Group("Curling bramble patch",parent); t.localPosition=V(x,0,z);
            for(int i=0;i<4;i++)
            {
                var points=new List<Vector3>();
                for(int j=0;j<26;j++)
                {
                    float a=j/25f*Mathf.PI*3, r=Mathf.Lerp(.7f,.08f,j/25f);
                    points.Add(V(i*.45f+Mathf.Cos(a)*r,.75f+Mathf.Sin(a)*r,i*.22f));
                }
                OutlineTube(t,"Thick curling vine",points.ToArray(),.16f,"Pine");
                for(int j=0;j<4;j++)
                {
                    var p=points[j*4];
                    Part("Bramble thorn",t,"cone",p,V(.17f,.25f,.17f),"Wood").transform.localRotation=Quaternion.Euler(0,0,40-j*30);
                }
            }
        }

        static Mesh Lathe(Vector2[] profile,int segments)
        {
            var vertices=new List<Vector3>(); var triangles=new List<int>(); var normals=new List<Vector3>();
            for(int j=0;j<profile.Length;j++)
            {
                Vector2 delta=profile[Mathf.Min(j+1,profile.Length-1)]-profile[Mathf.Max(0,j-1)];
                for(int i=0;i<=segments;i++)
                {
                    float a=i*Mathf.PI*2/segments;
                    vertices.Add(V(Mathf.Cos(a)*profile[j].x,profile[j].y,Mathf.Sin(a)*profile[j].x));
                    normals.Add(V(Mathf.Cos(a)*delta.y,-delta.x,Mathf.Sin(a)*delta.y).normalized);
                    if(j<profile.Length-1 && i<segments)
                    {
                        int v=j*(segments+1)+i, b=v+segments+1;
                        triangles.AddRange(new[]{v,b,v+1,v+1,b,b+1});
                    }
                }
            }
            var m=new Mesh(); m.SetVertices(vertices); m.SetTriangles(triangles,0); m.SetNormals(normals); m.RecalculateBounds(); return m;
        }
        static Mesh RoundedBox(float bevel)
        {
            var vertices=new List<Vector3>(); var normals=new List<Vector3>(); var triangles=new List<int>();
            var faces=new[]{Vector3.right,Vector3.left,Vector3.up,Vector3.down,Vector3.forward,Vector3.back};
            int steps=4;
            foreach(var face in faces)
            {
                Vector3 u=Mathf.Abs(face.y)>.5f?Vector3.right:Vector3.up;
                Vector3 v=Vector3.Cross(face,u);
                int offset=vertices.Count;
                for(int y=0;y<=steps;y++) for(int x=0;x<=steps;x++)
                {
                    Vector3 p=face*.5f+u*(x/(float)steps-.5f)+v*(y/(float)steps-.5f);
                    Vector3 inner=V(Mathf.Clamp(p.x,-.5f+bevel,.5f-bevel),Mathf.Clamp(p.y,-.5f+bevel,.5f-bevel),Mathf.Clamp(p.z,-.5f+bevel,.5f-bevel));
                    Vector3 n=(p-inner).normalized; vertices.Add(inner+n*bevel); normals.Add(n);
                    if(x<steps && y<steps)
                    {
                        int a=offset+y*(steps+1)+x, b=a+steps+1;
                        triangles.AddRange(new[]{a,a+1,b,a+1,b+1,b});
                    }
                }
            }
            var mesh=new Mesh(); mesh.SetVertices(vertices); mesh.SetNormals(normals); mesh.SetTriangles(triangles,0); mesh.RecalculateBounds(); return mesh;
        }
        static Mesh Canopy(int seed)
        {
            int rings=40, sides=64; var vertices=new List<Vector3>(); var triangles=new List<int>();
            vertices.Add(Vector3.up);
            for(int j=1;j<rings;j++) for(int i=0;i<sides;i++)
            {
                float phi=j*Mathf.PI/rings, theta=i*Mathf.PI*2/sides;
                vertices.Add(CanopyPoint(phi,theta,seed));
                if(j<rings-1)
                {
                    int a=1+(j-1)*sides+i, next=1+(j-1)*sides+(i+1)%sides, b=a+sides;
                    triangles.AddRange(new[]{a,next,b,next,next+sides,b});
                }
            }
            int bottom=vertices.Count; vertices.Add(Vector3.down);
            for(int i=0;i<sides;i++)
            {
                int next=(i+1)%sides;
                triangles.AddRange(new[]{0,1+next,1+i,bottom,1+(rings-2)*sides+i,1+(rings-2)*sides+next});
            }
            var mesh=new Mesh(); mesh.SetVertices(vertices); mesh.SetTriangles(triangles,0);
            // Shared radial normals prevent split UV poles from opening black fans in the hull.
            mesh.SetNormals(vertices.Select(p=>p.normalized).ToList());
            mesh.RecalculateBounds(); return mesh;
        }
        static Vector3 CanopyPoint(float phi,float theta,int seed)
        {
            float bump=1+Mathf.Sin(phi)*((seed==1?.08f:.055f)*Mathf.Sin(theta*(seed==1?6:9)+seed+phi*3)+.045f*Mathf.Cos(phi*(seed==2?20:16)+theta*2+seed));
            float taper=seed==0?1-.32f*Mathf.Cos(phi):seed==1?1+.1f*Mathf.Sin(phi):seed==3?.8f-.12f*Mathf.Cos(phi):1;
            return V(Mathf.Sin(phi)*Mathf.Cos(theta)*bump*taper,Mathf.Cos(phi)*bump,Mathf.Sin(phi)*Mathf.Sin(theta)*bump);
        }
        static Mesh Torus(float radius,float tube)
        {
            var v=new List<Vector3>(); var tri=new List<int>(); var normals=new List<Vector3>(); int rings=64,sides=10;
            for(int j=0;j<=rings;j++) for(int i=0;i<=sides;i++)
            {
                float a=j*Mathf.PI*2/rings,b=i*Mathf.PI*2/sides;
                var n=V(Mathf.Cos(a)*Mathf.Cos(b),Mathf.Sin(b),Mathf.Sin(a)*Mathf.Cos(b));
                v.Add(V(Mathf.Cos(a)*radius,0,Mathf.Sin(a)*radius)+n*tube); normals.Add(n);
                if(j<rings && i<sides)
                {
                    int k=j*(sides+1)+i,l=k+sides+1; tri.AddRange(new[]{k,k+1,l,k+1,l+1,l});
                }
            }
            var mesh=new Mesh(); mesh.SetVertices(v); mesh.SetTriangles(tri,0); mesh.SetNormals(normals); mesh.RecalculateBounds(); return mesh;
        }
        static Mesh Tube(Vector3[] path,float radius,int sides,float tipRadius=-1)
        {
            var v=new List<Vector3>(); var tri=new List<int>(); var normals=new List<Vector3>();
            // Round ends keep isolated marks from looking like cut pipe.
            var centers=new List<Vector3>(); var radii=new List<float>();
            Vector3 first=(path[1]-path[0]).normalized, last=(path[path.Length-1]-path[path.Length-2]).normalized;
            centers.Add(path[0]-first*radius); radii.Add(.001f);
            centers.AddRange(path); for(int i=0;i<path.Length;i++) radii.Add(tipRadius<0?radius:Mathf.Lerp(radius,tipRadius,i/(float)(path.Length-1)));
            centers.Add(path[path.Length-1]+last*(tipRadius<0?radius:tipRadius)); radii.Add(.001f);
            for(int j=0;j<centers.Count;j++)
            {
                var tangent=(centers[Mathf.Min(j+1,centers.Count-1)]-centers[Mathf.Max(0,j-1)]).normalized;
                var u=Vector3.Cross(tangent,Mathf.Abs(tangent.y)>.9f?Vector3.forward:Vector3.up).normalized;
                var w=Vector3.Cross(tangent,u);
                for(int i=0;i<=sides;i++)
                {
                    float a=i*Mathf.PI*2/sides; var n=u*Mathf.Cos(a)+w*Mathf.Sin(a);
                    v.Add(centers[j]+n*radii[j]); normals.Add(n);
                    if(j<centers.Count-1 && i<sides)
                    {
                        int k=j*(sides+1)+i,l=k+sides+1; tri.AddRange(new[]{k,k+1,l,k+1,l+1,l});
                    }
                }
            }
            var m=new Mesh(); m.SetVertices(v); m.SetTriangles(tri,0); m.SetNormals(normals); m.RecalculateBounds(); return m;
        }
        static Mesh RockMesh()
        {
            int sides=7; var vertices=new List<Vector3>(); var triangles=new List<int>();
            for(int j=0;j<3;j++) for(int i=0;i<sides;i++)
            {
                float a=i*Mathf.PI*2/sides;
                float radius=j==0?.44f:j==1?.57f:.28f;
                vertices.Add(V(Mathf.Cos(a)*radius*(1+.1f*Mathf.Sin(i*5)),j==0?-.48f:j==1?-.04f:.48f,Mathf.Sin(a)*radius));
            }
            for(int j=0;j<2;j++) for(int i=0;i<sides;i++)
            {
                int a=j*sides+i,b=j*sides+(i+1)%sides;
                triangles.AddRange(new[]{a,a+sides,b,b,a+sides,b+sides});
            }
            for(int i=1;i<sides-1;i++) { triangles.AddRange(new[]{0,i,(i+1)}); triangles.AddRange(new[]{14,14+i+1,14+i}); }
            var mesh=new Mesh(); mesh.SetVertices(vertices); mesh.SetTriangles(triangles,0); mesh.RecalculateNormals(); mesh.RecalculateBounds(); return mesh;
        }
        static Mesh Panel(Vector2[] shape,float depth)
        {
            var v=new List<Vector3>(); var tri=new List<int>(); int n=shape.Length;
            foreach(float z in new[]{-depth/2,depth/2}) foreach(var p in shape) v.Add(V(p.x,p.y,z));
            float area=0;
            for(int i=0;i<n;i++) { var a=shape[i]; var b=shape[(i+1)%n]; area+=a.x*b.y-b.x*a.y; }
            for(int i=1;i<n-1;i++)
                tri.AddRange(area<0?new[]{0,i,i+1,n,n+i+1,n+i}:new[]{0,i+1,i,n,n+i,n+i+1});
            for(int i=0;i<n;i++)
            {
                int j=(i+1)%n;
                tri.AddRange(area<0?new[]{i,n+i,j,j,n+i,n+j}:new[]{i,j,n+i,j,n+j,n+i});
            }
            var m=new Mesh(); m.SetVertices(v); m.SetTriangles(tri,0); m.RecalculateNormals(); m.RecalculateBounds(); return m;
        }
        static Mesh LeafMesh()
        {
            return Lathe(new[]{new Vector2(0,0),new Vector2(.12f,.17f),new Vector2(.15f,.36f),new Vector2(.08f,.6f),new Vector2(0,.8f)},12);
        }

        public static object Validate()
        {
            var scene=SceneManager.GetActiveScene();
            var renderers=UnityEngine.Object.FindObjectsByType<MeshRenderer>();
            var missing=renderers.Where(r=>!r.sharedMaterial || !r.GetComponent<MeshFilter>() || !r.GetComponent<MeshFilter>().sharedMesh).Select(r=>r.name).ToArray();
            var materials=AssetDatabase.FindAssets("t:Material",new[]{Root+"/Materials"}).Select(g=>AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(g)));
            var badShaders=materials.Where(m=>!m.shader || ShaderUtil.ShaderHasError(m.shader)).Select(m=>m.name).ToArray();
            if(scene.path!=ScenePath || missing.Length>0 || badShaders.Length>0 || !Camera.main || !Camera.main.orthographic)
                throw new InvalidOperationException("Diorama validation failed: "+string.Join(",",missing.Concat(badShaders)));
            return new { scene=scene.path, scene.isDirty, renderers=renderers.Length, missing, badShaders, prefabs=AssetDatabase.FindAssets("t:Prefab",new[]{Root+"/Prefabs"}).Length, playing=EditorApplication.isPlaying };
        }
        static void ExportEnvironmentPrefabs(Scene scene)
        {
            var names=new Dictionary<string,string>
            {
                {"Tall scalloped tree","WoodlandTree"},{"Tall sweeping pine","PineTree"},
                {"Broad old oak","BroadOak"},{"Round teal tree","RoundTealTree"},{"Orange cloud tree","OrangeTree"},{"Small layered cedar","CedarTree"},
                {"Bare twisting oak","BareOak"},{"Broken mossy wall","MossyWall"},
                {"Ancient standing stones","StandingStones"},{"Quiet circular fountain","Fountain"},
                {"Mossy stone sarcophagus","Sarcophagus"},{"Coral mushroom cluster","Mushrooms"},
                {"Banded wooden barrel","Barrel"},{"Open treasure chest","TreasureChest"},
                {"Little ivory skull","Skull"},{"Ivy covered broken arch","BrokenTower"},
                {"Scalloped undergrowth","Undergrowth"},{"Planked supply crate","SupplyCrate"},{"Little blue field mouse","FieldMouse"},
                {"Skull marker on crooked stake","SkullMarker"},{"Scattered ivory bones","Bones"}
            };
            var all=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Transform>()).ToArray();
            foreach(var pair in names)
            {
                var source=all.FirstOrDefault(t=>t.name==pair.Key);
                if(!source) continue;
                var copy=UnityEngine.Object.Instantiate(source.gameObject);
                copy.transform.SetParent(null); copy.transform.SetPositionAndRotation(Vector3.zero,Quaternion.identity);
                copy.transform.localScale=source.localScale/Mathf.Max(source.localScale.z,.001f);
                PrefabUtility.SaveAsPrefabAsset(copy,Root+"/Prefabs/"+pair.Value+".prefab");
                UnityEngine.Object.DestroyImmediate(copy);
            }
        }
    }
}
