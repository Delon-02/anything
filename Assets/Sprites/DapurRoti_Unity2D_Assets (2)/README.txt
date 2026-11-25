ASET UNITY 2D - DAPUR ROTI
===========================

Resolusi sumber: 1536 x 1024 px
Grid acuan: 32 x 32 px (48 x 32 tile)
PPU acuan: 32

ISI FOLDER
1_Wall.png              : latar dinding (bagian selain dinding transparan)
2_Floor.png             : lantai kayu (bagian selain lantai transparan)
3_Decoration.png        : dekorasi dinding/lampu/tanda dan elemen dekoratif
4_Appliances.png        : kulkas, kompor, oven, lemari display
5_Furniture.png         : meja, kabinet, island, rak display
6_Kitchen_Props_Food.png: perlengkapan dapur dan makanan
7_Plants_Misc.png       : tanaman dan beberapa objek kecil

CATATAN PENTING
- Semua PNG berukuran sama (1536 x 1024) agar posisi antar-layer tetap sejajar saat ditumpuk di Unity.
- Layer objek memiliki transparansi. Karena sumbernya satu gambar datar, pemisahan objek otomatis merupakan hasil perkiraan; periksa tepi objek dan lakukan koreksi kecil di Sprite Editor bila diperlukan.
- Layer Wall dan Floor dipisah pada batas horizontal sekitar y=378 px.
- PNG ini adalah layer komposit transparan, bukan setiap objek sebagai file terpisah.
- Untuk mengimpor: Texture Type = Sprite (2D and UI), Sprite Mode = Multiple untuk layer yang ingin di-slice, Pixels Per Unit = 32, Filter Mode = Point (no filter) bila ingin tampilan pixel-art tajam.
- Gunakan Sprite Editor > Slice untuk memotong sprite. Grid 32x32 cocok sebagai acuan tile, tetapi furnitur/mesin besar perlu dipotong manual berdasarkan batas objek, bukan dipaksa menjadi satu tile.
