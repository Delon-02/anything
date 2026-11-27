FRENCH TOWN - LAYER ASSETS (1536x1024 px = 48 x 32 tile @ 32px, PPU 32)

Layers/ (semua ukuran sama, tinggal ditumpuk di posisi (0,0))
  1_Ground.png              rumput + trotoar (jalan dilubangi transparan)   -> Ground_Tilemap      (tanpa collision)
  2_Road.png                aspal, zebra cross, panah, lengkungan batu      -> Road_Tilemap        (tanpa collision)
  3_Buildings_Walls.png     dinding + pondasi 11 bangunan                   -> Building_Tilemap    (collision)
  4_Decoration.png          bangku, lampu jalan, pot bunga, pohon bawah     -> Decoration_Tilemap  (collision selektif)
  5_AbovePlayer_Roofs.png   atap semua bangunan                             -> AbovePlayer_Tilemap (tanpa collision)

Data/
  collision_grid_48x32.csv  1 = solid, 0 = bisa dilewati (1 sel = 1 tile 32x32)
  preview_collision_red.png pratinjau area collision (merah)
  regions_used.py           koordinat kotak yang dipakai untuk memotong

Import Unity: Texture Type = Sprite (2D and UI), Sprite Mode = Multiple, PPU = 32,
Filter Mode = Point (no filter), Compression = None, lalu Sprite Editor > Slice > Grid By Cell Size 32x32.
