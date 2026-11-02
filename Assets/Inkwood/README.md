# Inkwood Glade

Open `Scenes/InkwoodGlade.unity` and press Play. A 2.4:1 wide Game view matches the reference composition; narrower views automatically preserve its horizontal framing. The clearing is surrounded by fully modeled scenery, so the camera can rotate around it.

## Camera controls

| Control | Action |
| --- | --- |
| Right mouse drag | Rotate around the clearing |
| A / D or left / right arrows | Rotate with the keyboard |
| Mouse wheel | Zoom |
| R | Restore the starting angle and zoom |
| H | Hide or show the control hint |
| S | Toggle real cast shadows |

The camera remains orthographic. Elevation, focus point, rotation speed, home framing, and zoom limits are editable on Main Camera's Diorama Camera component.

## Artwork

Six adventurers (wizard, bucket knight, ranger, pilgrim, monk, skeleton guard) and four goblin variants use simple, posed 3D forms with modeled equipment: spellbooks, satchels, feathers, quivers, bowed shields, shaped axes, flower bucklers, and a skull standard. The goblins have smaller pointed heads, swept ears, and bent stick limbs.

Trees include tall scalloped crowns, broad oaks, round teal trees, orange cloud trees, rounded layered cedars, sweeping pines, and tapered bare trees. Prefabs retain their authored shape proportions. There are 31 reusable character and environment prefabs in `Prefabs/`.

The layout uses ground anchors measured from the supplied illustration. Major landmarks follow the reference: adventurers and forest on the left, central standing stones and overgrown ruin, camp behind the fountain, and goblins on the right. Additional details include ivy, skull stakes, supply crates, candles, field mice, bones, scattered mushrooms, weapons, treasure, barrels, cut logs, foliage, and brambles.

Colors are shared materials in `Materials/`. Toon Color uses the actual directional Sun and distinct artist-selected lit and shadow colors. The band boundary and narrow transition are adjustable per material. Grounding shadows are simple pale ovals when cast shadows are off. PaletteProfile disables tone mapping, bloom, and vignetting to preserve the selected colors.

Select **08 · Toon lighting** to compare **No cast shadows** and **Cast shadows** in the Inspector, or press **S** in Play Mode. Both modes retain directional toon bands. Rotate its child Sun to move the lit/shadow boundary. The outlines and interior ink strokes stay unlit and do not cast shadows; only the actual model surfaces cast. Grounding ovals hide automatically when real shadows are enabled.

### Outer lines

Each outlined surface has a child named Silhouette, sharing its mesh and the Outline material. Ink Outline expands the mesh along its world-space normals and renders its back faces. Smooth normals and welded canopy poles prevent cracks and fans. The expansion is calculated from the camera projection to keep line weight controlled while zooming. Outline width is expressed at a reference image height of 1080 pixels and scales with output resolution.

Change **Outline width at 1080p** and **Ink color** on `Materials/Outline.mat`. Change `Materials/Ink.mat` to match the interior stroke color.

### Broken interior lines

Foliage arches, masonry marks, cracks, clothing folds, and facial details are intentionally placed mesh tubes, with spaces between strokes. These are actual 3D details attached to the relevant model, not textures or automatic edge detection. Canopy marks are sampled from the canopy surface, with marks on both sides. Their visibility changes naturally with occlusion during camera rotation. Geometry intersections and creases are not automatically inked, so the scene keeps a selective illustration-like finish.

## Character animation

All ten character prefabs include editable Unity Idle and Walk clips and an Animator controller in `Animations/`. Idles add breathing, small head turns, arm and equipment sway, and blinking on characters with eyes. Walks are intentionally in place so the reference layout stays intact. Rigid body-part pivots preserve the simple shapes and carry their outlines and interior strokes with them. These are stylized rigid rigs, rather than deforming humanoid rigs.

In Play Mode, use the top-left controls or **1** for Idle, **2** for Walk, and **Space** to pause/resume. The **09 · Character motion** object controls the whole scene; individual Character Motion components expose speed and mode through their public methods. Rebuilding the scene retains existing animation clips, allowing hand edits in Unity's Animation window.

Validation checked all animation bindings and looping settings, unchanged rest poses, and all fourteen scene actors entering Idle and Walk. Live Play Mode checks confirmed moving leg poses, stationary scene anchors, and pause/resume at half speed.

Open `Scenes/AnimationStudio.unity` and press Play for the close-up viewer. **P/N** selects the previous/next character; **1/2** selects Idle/Walk; **Space** pauses. The panel also has a speed slider and a cast-shadow switch. Camera rotation, zoom, **R** reset, **S** shadows, and **H** hide controls work here too. Selection retains the current motion, speed, pause state, and lighting mode. **Inkwood > Build Animation Studio** recreates this separate preview scene after open scenes are saved. Keyboard checks verified Next, Walk, and Pause; selection wrapping, speed retention, and both shadow modes were also checked live.

`Previews/Character-Walk.gif` compares one synchronized, slowed walk cycle from the actual clips. The Studio uses each character's authored playback timing.

## Rendering

`Settings/InkwoodPipeline.asset` is assigned to the active PC quality level. Its dedicated forward renderer uses 4x MSAA; the camera also uses SMAA. The original PC pipeline and renderer assets remain available in `Assets/Settings/`. The scene does not require Linework or Blender imports.

## Rebuilding

The saved scene and prefabs are ready to edit directly; rebuilding is optional. **Inkwood > Build Reference Diorama** deterministically regenerates the scene, meshes, materials, and prefabs. It replaces edits to those generated assets. Save all open scenes and stop Play Mode first; the builder refuses to run otherwise. Mesh updates preserve asset references and explicitly refresh their GPU buffers.

## Verification

The project compiled with no C# errors or warnings. Asset validation checked scene identity, mesh/material references, shader errors, and the orthographic camera. Play Mode checks used temporary Input System devices to exercise keyboard rotation, right-mouse rotation, both zoom limits, and reset, while confirming advancing frames. The temporary devices were removed afterward. Full-scene, rotated, and character close-up captures were visually reviewed, including the native Game view at a smaller resolution. Unity is left out of Play Mode.
