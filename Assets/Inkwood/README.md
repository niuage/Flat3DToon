# Inkwood Glade

Open `Scenes/InkwoodGlade.unity` and press Play. Use a 16:9 Game view for the starting composition. The reference-inspired clearing is surrounded by fully modeled scenery, so the camera can rotate around it.

## Camera controls

| Control | Action |
| --- | --- |
| Right mouse drag | Rotate around the clearing |
| A / D or left / right arrows | Rotate with the keyboard |
| Mouse wheel | Zoom |
| R | Restore the starting angle and zoom |
| H | Hide or show the control hint |

The camera remains orthographic. Elevation, focus point, rotation speed, home framing, and zoom limits are editable on Main Camera's Diorama Camera component.

## Artwork

Five adventurers (wizard, bucket knight, ranger, pilgrim, monk), three goblin variants, scalloped trees, pines, standing stones, broken towers and walls, a fountain, sarcophagi, and a woodland camp are built from editable 3D mesh assets. Small props include skulls, mushrooms, weapons, treasure, barrels, cut logs, leaves, and brambles. Twenty reusable character and environment prefabs are in `Prefabs/`.

Colors are shared materials in `Materials/`. The Flat Color shader uses a small, fixed three-band shading difference to suggest volume without realistic highlights or cast shadows. Grounding shadows are deliberately simple pale ovals. PaletteProfile disables tone mapping, bloom, and vignetting to preserve the selected colors.

### Outer lines

Each outlined surface has a child named Silhouette, sharing its mesh and the Outline material. Ink Outline expands the mesh along its world-space normals and renders its back faces. Smooth normals and welded canopy poles prevent cracks and fans. The expansion is calculated from the camera projection to keep line weight controlled while zooming. Outline width is expressed at a reference image height of 1080 pixels and scales with output resolution.

Change **Outline width at 1080p** and **Ink color** on `Materials/Outline.mat`. Change `Materials/Ink.mat` to match the interior stroke color.

### Broken interior lines

Foliage arches, masonry marks, cracks, clothing folds, and facial details are intentionally placed mesh tubes, with spaces between strokes. These are actual 3D details attached to the relevant model, not textures or automatic edge detection. Canopy marks are sampled from the canopy surface, with marks on both sides. Their visibility changes naturally with occlusion during camera rotation. Geometry intersections and creases are not automatically inked, so the scene keeps a selective illustration-like finish.

## Rendering

`Settings/InkwoodPipeline.asset` is assigned to the active PC quality level. Its dedicated forward renderer uses 4x MSAA; the camera also uses SMAA. The original PC pipeline and renderer assets remain available in `Assets/Settings/`. The scene does not require Linework or Blender imports.

## Rebuilding

The saved scene and prefabs are ready to edit directly; rebuilding is optional. **Inkwood > Build Reference Diorama** deterministically regenerates the scene, meshes, materials, and prefabs. It replaces edits to those generated assets. Save all open scenes and stop Play Mode first; the builder refuses to run otherwise. Mesh updates preserve asset references and explicitly refresh their GPU buffers.

## Verification

The project compiled with no C# errors or warnings. Asset validation checked scene identity, mesh/material references, shader errors, and the orthographic camera. Play Mode checks used temporary Input System devices to exercise keyboard rotation, right-mouse rotation, both zoom limits, and reset, while confirming advancing frames. The temporary devices were removed afterward. Full-scene, rotated, and character close-up captures were visually reviewed, including the native Game view at a smaller resolution. Unity is left out of Play Mode.
