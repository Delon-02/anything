THE LITTLE MATCHA BAKERY — UNITY ASSET STARTER
Target: Unity 2022.3 LTS

CONTENTS
Assets/Art/Sprites/Bakery_Interior_Spritesheet.png
  Modular visual atlas containing wall sections, doors, windows, counters,
  display cases, shelves, tables, chairs, plants, lamps, rugs, and decor.
Assets/Art/Backgrounds/TheLittleMatchaBakery_Interior.png
  Full-room concept reference/background.

IMPORTANT
The spritesheet is an atlas image, not yet a collection of individually
cropped PNG files. Several pieces are visually arranged in groups. Use the
Sprite Editor to slice the atlas, then adjust each rectangle around the
individual objects. Automatic slicing may group adjacent objects together,
so manual slicing is recommended for final gameplay assets.

UNITY IMPORT SETTINGS
1. Copy the Assets folder into your Unity project's root folder.
2. Select Bakery_Interior_Spritesheet.png in the Project window.
3. Inspector:
   Texture Type: Sprite (2D and UI)
   Sprite Mode: Multiple
   Pixels Per Unit: 16 (starting suggestion; adjust to your chosen tile scale)
   Mesh Type: Tight
   Filter Mode: Point (no filter)
   Compression: None
4. Click Apply.
5. Open Sprite Editor > Slice > use Grid by Cell Size only if you establish a
   consistent grid; otherwise use manual rectangles for each object.
6. Apply and expand the PNG asset in Project to see the sliced sprites.
7. Set pivots per object (usually Bottom Center for furniture/props).
8. Add BoxCollider2D only to gameplay obstacles and configure sorting layers.

RECOMMENDED SORTING LAYERS
Ground, GroundDetails, Furniture, Player, Foreground, Overlay.

NOTE ON GAMEPLAY READINESS
This starter includes artwork and import guidance. It does not include finished
colliders, prefabs, animation, scripts, or a completed Tilemap. Those should be
configured in the Unity scene after slicing and testing the sprites.
