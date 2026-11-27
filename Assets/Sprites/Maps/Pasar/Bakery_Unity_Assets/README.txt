BAKERY SHOP - UNITY SPRITE ASSETS

Isi:
- Background_Original_Reference.png : gambar utuh sebagai referensi tata letak.
- Sprites/*.png : potongan objek PNG dengan transparansi (RGBA).
- asset_positions.json : posisi dan ukuran setiap potongan pada kanvas gambar asli (1536 x 1024 px).

CATATAN:
Latar asli masih menyimpan objek-objeknya. File ini disertakan sebagai referensi komposisi,
bukan background kosong. Untuk menyusun ulang scene secara bebas, gunakan sprites PNG
di atas background/floor buatan sendiri atau Tilemap Unity. Jangan menumpuk sprite pada
Background_Original_Reference jika tidak ingin objek terlihat dobel.

CARA PAKAI DI UNITY:
1. Extract ZIP.
2. Salin folder Sprites ke Assets/Art/Bakery/Sprites.
3. Pilih PNG -> Inspector:
   Texture Type = Sprite (2D and UI)
   Sprite Mode = Single
   Alpha Is Transparency = aktif
   Pixels Per Unit = samakan untuk semua sprite (contoh 32 atau 100).
4. Klik Apply.
5. Drag sprite ke Scene atau susun melalui Sorting Layers.
6. Untuk tampilan top-down, atur Sorting Layer/Order in Layer sesuai kedalaman objek.
7. Posisi pada JSON menggunakan koordinat piksel dari kiri-atas gambar asli. Unity memakai titik origin dan skala berbeda,
   jadi JSON adalah panduan penyusunan, bukan file scene Unity siap pakai.

Objek yang dipisahkan:
- Rak dinding kiri/tengah/kanan
- Area mixer dan peralatan gantung
- Meja kasir
- Pajangan bahan kiri
- Tiga rak tengah
- Rak karung kanan
- Rak bawah kiri dan meja peralatan bawah kanan
- Pintu masuk/karpet/pilar
- Papan nama toko, papan tulis, rak baki, lampu gantung
