THE LITTLE MATCHA BAKERY - NPC PEREMPUAN (UNITY 2D)
===================================================

ISI PAKET
- Sprites/SpriteSheets/ : 4 sprite sheet, masing-masing 6 frame.
- Sprites/Frames/       : 24 PNG frame terpisah (siap dipakai satu per satu).
- Setiap frame berukuran 200 x 256 px dan memiliki background transparan.
- Arah: Down (depan), Up (belakang), Left (kiri), Right (kanan).

PENGATURAN IMPORT DI UNITY
1. Salin folder Sprites ke dalam folder Assets di project Unity.
2. Pilih PNG sprite sheet atau frame di jendela Project.
3. Atur:
   Texture Type     = Sprite (2D and UI)
   Sprite Mode      = Multiple (untuk SpriteSheets), Single (untuk Frames)
   Pixels Per Unit  = 32
   Filter Mode      = Point (no filter) untuk tampilan pixel-art; jika ingin lebih lembut,
                      gunakan Bilinear.
   Compression      = None
   Alpha Is Transparency = aktif
4. Untuk SpriteSheets, buka Sprite Editor > Slice > Grid By Cell Size.
   Pixel Size: 200 x 256. Pivot: Bottom Center. Klik Slice lalu Apply.
5. Pilih enam sprite dari satu arah, lalu drag ke Scene/Hierarchy untuk membuat
   Animation Clip. Ulangi untuk arah lainnya. Atur Samples sesuai kebutuhan
   (misalnya 8-12 FPS).

CATATAN
- Sprite sheet sumber berisi enam pose per arah. Paket ini memisahkan keempat arah
  dan menghapus pola checkerboard latar agar PNG transparan.
- Frame mengikuti urutan kiri ke kanan dari gambar sumber.
- Untuk gerakan berjalan, periksa preview animasi dan atur FPS agar gerak terlihat
  sesuai. Tidak disertakan Animator Controller karena parameter dan sistem gerak
  bergantung pada project Unity yang digunakan.
