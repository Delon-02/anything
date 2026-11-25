# The Little Matcha Bakery — Unity 2D Tilemap Asset Pack

## Source and assumptions
- Built from the supplied 1535x1024 image. The requested canvas is 1536x1024, so one transparent pixel was added at the right edge (no rescaling).
- Target grid: 48 x 32 tiles, 32 x 32 px per tile, Pixels Per Unit = 32.
- Every PNG in `Layers/` is 1536x1024 RGBA and aligned to the same top-left origin. Place all layer GameObjects at the same world position.
- Extraction from a single flattened illustration is approximate: overlapping objects and pixels with similar colors cannot be perfectly separated. Inspect edges in Unity and refine masks if needed.
- The two green rugs are supplied as their own layer. Door sprites are also duplicated in `Doors/` for separate collider/animation setup.
- Sprite pivots are documented as Bottom Center in adjacent JSON metadata. Set each sprite's Unity Sprite Editor pivot to Bottom Center (Unity does not read the JSON pivot automatically).

## Layer order (back to front)
1. `1_Floor.png` — wooden floor region.
2. `2_Rugs.png` — two rugs.
3. `3_Walls_Frame.png` — upper wall, perimeter frame, menu/window/shelves and side/bottom frame; door openings excluded.
4. `4_Decoration.png` — counter, register, tables/chairs, plants and flower boxes.
5. `5_AbovePlayer.png` — awning and hanging lamps; place above the player.

## Unity import
- Texture Type: Sprite (2D and UI)
- Sprite Mode: Single for layer images; Multiple for atlases/sheets only
- Pixels Per Unit: 32
- Mesh Type: Tight for individual objects; Full Rect for full-size layers
- Filter Mode: Point (no filter) for crisp pixel art, or Bilinear if preserving the painted softness of the source
- Compression: None
- Layer PNGs: pivot Center; keep all at (0,0) so they align exactly
- Object sprites: set pivot to Bottom Center in Sprite Editor.
- Create a Grid with Cell Size (1,1,0), Tilemap Renderer sorting layers as needed. Since the illustration uses perspective, use the layers as aligned overlay tilemaps or place the extracted object sprites as individual GameObjects for more reliable sorting.

## Suggested sorting
Floor (0), Rugs (1), Walls (2), Props (3), Player (4), AbovePlayer (5).
For walk-behind behavior, split tall props into base/top sprites or use a 2D Transparency Sorter and custom pivots.

## Movement script
`Scripts/TopDownPlayerController.cs` supports the legacy Input Manager and the Input System package via `ENABLE_INPUT_SYSTEM`, and uses `Rigidbody2D.linearVelocity` on Unity 6 with `velocity` on earlier versions.

## Fix tambahan: meja kayu
- `Objects/Props/Display_Counter.png` sekarang mencakup etalase kaca beserta badan/meja kayu bagian bawah.
- `Objects/Props/Food_Display_Counter_Wood.png` adalah nama sprite alternatif untuk etalase lengkap.
- `Objects/Props/Wooden_Cashier_Desk.png` adalah meja kasir lengkap; mesin kasir tetap tersedia sebagai sprite terpisah `Cash_Register.png`.
- Layer `4_Decoration.png` sudah diperbarui agar kedua meja tidak hilang saat memakai layer penuh.

## Combined bakery counter (recommended)
- Gunakan satu sprite `Objects/Props/Bakery_Display_and_Cashier_Combined.png` untuk seluruh area etalase makanan + meja kayu kasir + mesin kasir.
- Sprite gabungan ini dibuat dari satu batas area sumber agar bagian meja tidak terpotong atau terpisah saat dipasang di Unity.
- Jangan menambahkan sprite meja pajangan, meja kasir, atau mesin kasir terpisah di atasnya. Objek-objek terpisah versi sebelumnya sudah dihapus dari folder Props agar tidak tertukar.
- Layer penuh `4_Decoration.png` juga sudah diperbarui.

## Sambungan meja kasir dan etalase
- Sprite gabungan telah diperbarui dengan panel kayu penghubung di antara sisi etalase dan meja kasir.
- Kedua bagian sekarang menjadi satu siluet/aset PNG yang tersambung. Gunakan hanya `Bakery_Display_and_Cashier_Combined.png`.
- Layer `4_Decoration.png` juga sudah diperbarui dengan versi tersambung ini.

- Sambungan memakai bagian kayu penghubung asli dari gambar sumber (bukan bidang warna buatan), sehingga etalase dan meja kasir terlihat menyatu secara alami.


## Perbaikan kerapian susunan Unity (versi grid 32 px)
- Semua sprite objek di folder `Objects/` dan `Doors/` sudah diberi kanvas transparan kelipatan 32 px.
- Gambar asli tidak diperbesar/dikecilkan dan tidak dipotong; hanya ditambah padding transparan.
- Artwork tiap sprite dipusatkan secara horizontal dan disejajarkan ke bagian bawah kanvas agar pivot `Bottom Center` konsisten.
- Layer penuh di folder `Layers/` tetap berukuran 1536x1024 (48x32 tile), tidak diubah.
- Import semua sprite dengan Pixels Per Unit = 32, lalu gunakan pivot Bottom Center untuk objek individual.
- Saat menempatkan objek, aktifkan Grid/Snap 1 unit. Untuk layer penuh, posisikan semuanya pada koordinat Transform yang sama (mis. X=0, Y=0).
- Catatan: padding membantu pivot dan snap lebih konsisten, tetapi posisi antarobjek tetap perlu mengikuti layout peta; jangan menggeser layer penuh satu per satu.
