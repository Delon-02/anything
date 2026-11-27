NPC BROWN HAIR — UNITY 2D SPRITE PACKAGE
=========================================

CONTENTS
- NPC_BrownHair_Sheet.png: 16-frame sheet, 8 columns x 2 rows.
- Frames/Idle/: 8 individual transparent PNG frames.
- Frames/Walk/: 8 individual transparent PNG frames.

CHARACTER / FRAME ORDER
Columns 1–2: Down, frames 1–2
Columns 3–4: Up, frames 1–2
Columns 5–6: Left, frames 1–2
Columns 7–8: Right, frames 1–2

Top row = Idle. Bottom row = Walk.

UNITY IMPORT SETTINGS (recommended)
1. Copy the NPC_BrownHair folder into your Unity project's Assets folder.
2. Select NPC_BrownHair_Sheet.png in the Project window.
3. Set:
   - Texture Type: Sprite (2D and UI)
   - Sprite Mode: Multiple
   - Pixels Per Unit: 192
   - Filter Mode: Point (no filter)
   - Compression: None
   - Mesh Type: Full Rect
4. Open Sprite Editor > Slice:
   - Type: Grid By Cell Size
   - Pixel Size: X = 192, Y = 256
   - Offset: X = 0, Y = 0
   - Padding: X = 0, Y = 0
   - Pivot: Bottom Center
5. Click Slice, then Apply.

ANIMATION SETUP
- Create an Animation Clip for each direction and animation type.
- Drag the two matching frame PNGs from Frames/Idle or Frames/Walk
  into the Animation timeline in frame order.
- For a simple starting point, use 6–8 samples per second for Idle
  and 8–10 samples per second for Walk.
- Set the SpriteRenderer's sorting layer/order as needed by your scene.

NOTE
The sheet and individual frame images have transparent RGBA backgrounds.
Unity generates its own .meta files when these assets are imported.
The PNGs are supplied as artwork assets; Animation Clips are not pre-authored.
