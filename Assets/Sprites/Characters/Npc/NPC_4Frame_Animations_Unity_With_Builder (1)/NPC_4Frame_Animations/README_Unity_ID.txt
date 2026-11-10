NPC 4-FRAME ANIMATION PACK — THE LITTLE MATCHA BAKERY

Isi: 64 PNG individual (4 aksi x 4 arah x 4 frame).
Aksi: Idle, Walk, Run, Sit.
Arah: Down, Up, Left, Right.

CARA IMPORT
1. Ekstrak folder NPC_4Frame_Animations ke dalam folder Assets project Unity.
2. Pilih seluruh PNG di folder NPC_4Frame_Animations.
3. Inspector: Texture Type = Sprite (2D and UI), Sprite Mode = Single.
4. Filter Mode = Point (no filter), Compression = None. Klik Apply.
5. Pastikan semua frame satu karakter memiliki ukuran dan posisi pivot yang konsisten.

MEMBUAT CLIPS + CONTROLLER OTOMATIS
1. Pastikan file BuildNPCAnimations.cs berada di Assets/NPC_4Frame_Animations/Editor/.
2. Tunggu Unity selesai compile.
3. Klik menu Tools > NPC Animations > Generate Clips and Controller.
4. Hasil ada di Assets/NPC_4Frame_Animations/Generated/.
5. Tambahkan komponen Animator pada GameObject NPC dan pasang NPC_Animator.controller ke field Controller.
6. Pastikan SpriteRenderer dan Animator berada pada GameObject yang sama (atau path animasi disesuaikan).

PARAMETER CONTROLLER
Action (Int): 0 Idle, 1 Walk, 2 Run, 3 Sit.
Direction (Int): 0 Down, 1 Up, 2 Left, 3 Right.

CONTOH KODE
animator.SetInteger("Action", 1);    // Walk
animator.SetInteger("Direction", 2); // Left

animator.SetInteger("Action", 0);    // Idle
animator.SetInteger("Direction", 0); // Down

FPS default: Idle 4, Walk 8, Run 12, Sit 4.
Sit dibuat tidak looping agar bisa digunakan sebagai pose duduk. Jika ingin gerakan duduk berulang, ubah loopTime di script.
