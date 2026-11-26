THE LITTLE MATCHA BAKERY — WALK SIDE LEFT/RIGHT

Aset ini berisi dua arah berjalan untuk NPC laki-laki yang sama:
- Walk_Right: 4 frame, karakter menghadap kanan.
- Walk_Left: 4 frame, karakter menghadap kiri.
- SpriteSheets: spritesheet 4 frame per arah dan gabungan 4x2.

STRUKTUR:
Walk_Right/Walk_Right_01.png ... Walk_Right_04.png
Walk_Left/Walk_Left_01.png ... Walk_Left_04.png
SpriteSheets/Walk_Right_4Frames.png
SpriteSheets/Walk_Left_4Frames.png
SpriteSheets/Walk_LeftRight_4x2.png

UKURAN:
- Setiap frame PNG: 320 x 420 px.
- Sprite sheet per arah: 1280 x 420 px.
- Sprite sheet gabungan: 1280 x 840 px.
- PNG RGBA transparan.

CARA PAKAI DI UNITY 2D (DISARANKAN: PNG TERPISAH):
1. Extract folder ini ke dalam Assets project Unity.
2. Pilih seluruh PNG di folder Walk_Right dan Walk_Left.
3. Inspector: Texture Type = Sprite (2D and UI), Sprite Mode = Single.
4. Filter Mode = Point (no filter), Compression = None, Generate Mip Maps = Off.
5. Samakan Pixels Per Unit dengan sprite idle NPC lain. Gunakan nilai yang sama untuk semua frame.
6. Pilih 4 file dalam Walk_Right di Project window, lalu drag ke Scene atau Animation window.
   Simpan sebagai Walk_Right.anim.
7. Ulangi 4 file Walk_Left, simpan sebagai Walk_Left.anim.
8. Pada setiap Animation Clip, aktifkan Loop Time dan set Samples sekitar 8–10 FPS.
9. Pastikan urutan frame 01, 02, 03, 04.

CATATAN PENTING:
- Sprite ini sudah dipisah menjadi file per frame agar tidak perlu Slice.
- Frame sudah diletakkan pada canvas ukuran sama dan baseline bawah yang sama untuk mengurangi efek karakter naik-turun.
- Animator Controller dan script pergerakan tidak termasuk ZIP; clip perlu dihubungkan ke parameter Animator pada project.
