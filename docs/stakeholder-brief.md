# DMS - Simando — Ringkasan untuk PGN

**Sistem Manajemen Proses Berlangganan Gas**
Ringkasan hasil analisis dokumen dan pertemuan, serta hal-hal yang kami perlukan
dari PGN agar pembangunan dapat berjalan.

*Dokumen ini ditujukan untuk pemangku kepentingan PGN. Rincian teknis ada di
[README](README.md).*

---

## 1. Yang kami pahami

**Masalah utama** (dari notulen pertemuan):

> *"Orang perusahaan tidak bisa memonitor workflownya statusnya dah sampai mana,
> bagaimana progressnya belum jelas."*

**Sebagian besar pekerjaan di sistem ini memang entri data — dan visibilitas
status adalah alasan sistem ini dibangun.** Keduanya tidak bertentangan: entri data
adalah caranya, visibilitas adalah tujuannya.

Sales Area akan banyak mengisi formulir; Formulir KK0 saja memuat sekitar 60 isian.
Namun **pekerjaan itu bukan pekerjaan baru.** PGN sudah mengumpulkan seluruh data
tersebut — di formulir kertas sesuai Prosedur Operasi `O-001/06.02`, di berkas
Excel, dan di dokumen Word yang dikirim lewat surel. Sistem ini tidak menambah
entri data, melainkan **memindahkannya ke satu tempat**.

Yang baru adalah hasil dari pemindahan itu: satu tempat untuk melihat, kapan saja,
calon pelanggan berada di tahap mana, ada di tangan siapa, dan sudah berapa lama.

**Delapan tahap**, satu record per calon pelanggan:

| Tahap | Nama | Penanggung jawab |
|---|---|---|
| 1 | Directory | Sales Area |
| 2 | Plotting | Sales Area |
| 3 | Prospect | Sales Area |
| 4 | Survey (KK0) | Sales Area |
| 5 | A1 — Registrasi Berlangganan | Sales Area |
| 6 | Permohonan NOL | Sales Area → Area Head |
| 7 | Evaluasi NOL | Admin Regional (SOR) |
| 8 | Persetujuan NOL | Reviewer → Division Head |

**Rantai persetujuan:**

```
Sales Area → Area Head → Admin Regional → Reviewer (2–3) → Division Head → NOL / RL
```

- **Setuju** → naik satu tingkat
- **Revisi** → turun satu tingkat
- **Tolak** → langsung ke Admin Regional *(bukan kembali ke pembuat)*
- Area Head berakhir pada **Lampiran 17**; selanjutnya menjadi wewenang Regional
- Admin Regional yang **menetapkan reviewer** untuk tiap permohonan

**Keluaran akhir** adalah **NOL** (*No Objection Letter* / Surat Pernyataan Tidak
Keberatan) atau **RL** (*Refusal Letter*).

---

## 1b. Perubahan lingkup terbaru

Dua hal disepakati untuk sprint ini dan mengubah rancangan:

### Pengelolaan pengguna dilakukan di dalam sistem

Kami **tidak memperoleh akses ke direktori pengguna internal PGN**, sehingga akun
dikelola sepenuhnya di dalam aplikasi ini.

| Konsekuensi | Penjelasan |
|---|---|
| Akun dibuat oleh admin | Tidak ada pendaftaran mandiri |
| Kata sandi disimpan sistem | Menggunakan pengamanan standar ASP.NET Core Identity |
| Kata sandi sementara | Diberikan admin **secara langsung** kepada pengguna, bukan melalui sistem |
| Tidak ada "lupa kata sandi" | Perlu email; pengguna menghubungi administrator |
| ⚠️ **Pencabutan akses manual** | Karyawan yang keluar **tetap dapat masuk** sampai admin menonaktifkan akunnya |

**Butir terakhir adalah risiko operasional utama.** Dengan SSO hal ini otomatis;
sekarang menjadi tanggung jawab administrator. Kami menyarankan **peninjauan akses
setiap triwulan**, dan sistem menampilkan kolom *login terakhir* agar akun yang
sudah lama tidak dipakai mudah terlihat.

Kami juga perlu **daftar pengguna dari PGN** — nama, Area, dan peran masing-masing
— karena tidak dapat kami ambil dari direktori.

### Email ditunda

Notifikasi hanya **di dalam aplikasi** (lonceng notifikasi dan penanda jumlah tugas).

| Konsekuensi | Penjelasan |
|---|---|
| Tidak ada surel pemberitahuan | Pengguna mengetahui adanya tugas **hanya saat membuka aplikasi** |
| Tidak ada ringkasan harian | Digantikan halaman *Tugas Saya* |
| Tidak ada tautan dari surel | Alamat halaman tetap dijaga agar tautan yang dikirim kelak tetap berlaku |

⚠️ Perlu diperhatikan: keluhan awal adalah *"tidak bisa memonitor statusnya sudah
sampai mana"*. Surel adalah salah satu jawabannya. Tanpa surel, **seluruh beban itu
ditanggung tampilan di dalam aplikasi** — penanda jumlah tugas, halaman Tugas Saya,
dan laporan penuaan. Jika seseorang tidak membuka aplikasi, tidak ada yang
memberitahunya. Kami menyarankan surel diaktifkan sesegera mungkin setelah sprint
ini.

Keduanya dibangun dengan sekat (*interface*) tersendiri, sehingga integrasi
direktori dan pengaktifan surel nantinya cukup berupa perubahan konfigurasi.

---

## 2. Yang kami perlukan dari PGN

Dikelompokkan menurut dampaknya. **Tidak ada lagi hal yang menghentikan
perhitungan inti sistem** — ketiga hal yang semula ada di kelompok itu sudah
kami selesaikan sendiri di sisi kami, dengan beralih ke isian manual alih-alih
menunggu rumus atau data yang belum PGN punya.

✅ **Sudah kami selesaikan sendiri, tidak perlu ditunggu:**

- **Nilai kalor 10 jenis bahan bakar** *(dulu nomor 1, 🔴)* — daripada menunggu
  8 nilai kalor yang belum ada, `Konversi ke Gas` kami jadikan **isian manual**
  per baris peralatan pada formulir Survei, persis seperti cara pengisian di
  formulir kertas. Kebutuhan gas tetap tercatat untuk semua jenis bahan bakar
  sejak hari pertama. Nilai kalor yang sudah kami punya (Batubara 6.000, HSD
  9.000, basis gas resmi 8.750) tetap ditampilkan sebagai acuan agar pengisi
  formulir bisa mengecek angkanya sendiri — dan bila PGN belakangan melengkapi
  tabel nilai kalor, sistem sudah siap beralih ke perhitungan otomatis tanpa
  migrasi data.
- **Formula OUP / UMP** *(dulu nomor 2, 🔴)* — daripada menunggu rumus yang
  tidak ada di satu pun sumber, **penetapan harga otomatis kami hapus dari
  rancangan v1**, bukan ditunda. `Harga`, `Kode Harga`, `Segmen` dan `Basis
  Kontrak` semuanya isian manual di A1 maupun NOL, persis seperti pengisian di
  kertas — tidak ada lagi tombol pilihan Otomatis/Manual, dan tidak ada tabel
  harga yang perlu diisi PGN untuk kami mulai membangun.
- **Harga segmen Bronze 1** *(dulu bagian dari nomor 2)* — sudah dikonfirmasi
  **10**, bukan `10.000` (itu salah baca pemisah ribuan), sejalan dengan
  segmen lain (Platinum 8,78 … B2 9,66 … B1 10). Sekadar catatan sekarang,
  bukan lagi data yang perlu di-seed — tidak ada tabel harga tersimpan.
- **Kurs USD/IDR** *(dulu nomor 3, 🔴)* — ternyata IRR, NPV dan Payback Period
  **tidak dihitung oleh sistem sejak awal**; ketiganya isian manual yang diisi
  Admin Regional dari hasil perhitungan mereka sendiri di luar sistem. Jadi
  kurs tidak menghambat pembangunan. Pertanyaannya tetap relevan untuk proses
  internal PGN sendiri, tapi tidak kami tunggu untuk mulai membangun.

### 🟡 Menghentikan fitur

| # | Kebutuhan | Tanpa ini |
|---|---|---|
| 4 | **Daftar pengguna** — nama, nama pengguna, Area, dan peran. Tidak dapat kami ambil dari direktori PGN. | **Tidak ada seorang pun dapat masuk ke sistem.** |
| 5 | **Dokumen Prosedur Operasi `O-001/06.02` lengkap (211 halaman)** — berkas aslinya bernama **`Final-PO Berlangganan Gas 2023_ o.pdf`** (nama ini terbaca pada tangkapan layar yang kami terima). Kami hanya memiliki 17 halaman. | Definisi formulir kami berdasarkan potongan; ada risiko bagian yang terlewat. |
| 6 | **Template Lampiran 16** (Penerbitan NOL/RL) dalam bentuk berkas | Dokumen NOL/RL tidak dapat dihasilkan sistem. |
| 8 | **Katalog ukuran meter (G-Size)** beserta flowrate nominal dan maksimum | Pemilihan meter pada tahap Evaluasi hanya berupa ketikan bebas. |

### 🟢 Perlu dikonfirmasi

| # | Pertanyaan |
|---|---|
| 9 | **Kebijakan kata sandi PGN** — panjang minimum, masa berlaku, aturan penguncian. Kami memakai bawaan: 12 karakter, tanpa masa berlaku, terkunci 15 menit setelah 10 percobaan. |
| 10 | Siapa yang berwenang **membuat akun dan menetapkan peran**? Usulan kami: System Admin untuk semua; Admin Regional hanya untuk peran tingkat Area di regionnya sendiri. |
| 11 | Siapa yang bertanggung jawab **menonaktifkan akun karyawan yang keluar**? Proses ini kini manual. |
| 11b | Kami **tidak membuat "super admin" yang dapat melihat seluruh data transaksi.** System Admin mengelola konfigurasi dan akun, tetapi **tidak dapat membuka record, dan tidak dapat menyetujui apa pun** — data survei memuat volume produksi dan harga pelanggan. Akses darurat tersedia, terbatas satu record, 60 menit, wajib beralasan, dan tercatat. Mohon dikonfirmasi apakah pembagian ini sesuai kebijakan PGN. |
| 12 | Format `Nomor` memakai **kode BPS resmi** (Jawa Timur = 35), dengan nomor urut **global**. Mohon dikonfirmasi — nomor tidak dapat diubah setelah terbit. |
| 12b | **Format penomoran** Nota Dinas dan Formulir KK0 belum kami ketahui. Kami memakai contoh yang ada di dokumen sumber sebagai bawaan (`No. ……KK0/AREA ……/20……` dan pola `<<Nomor>>` pada Nota Dinas) — sistem tetap menerbitkan nomor sejak hari pertama, dan mengganti formatnya nanti adalah perubahan kode, bukan migrasi data. Mohon dikonfirmasi formatnya yang benar. |
| 13 | Basis kalor gas: **8.750 kkal/m³** (sesuai Daftar Peralatan Gas) atau **9.000** (sesuai worksheet)? Kami memakai 8.750 sebagai bawaan. |
| 14 | Apakah **amendemen / perpanjangan perjanjian** pelanggan eksisting termasuk lingkup? Formulir resmi mendukungnya; pertemuan belum membahasnya. |
| 15 | Apakah **basis kontrak Harian** benar dipakai? Jika ya, diperlukan tabel pemakaian per hari (Senin–Minggu). |
| 16 | **Sistem tidak membubuhkan tanda tangan.** Dokumen diunduh (.docx), ditandatangani di luar sistem — basah maupun digital, termasuk PSrE bersertifikat bila diperlukan — lalu diunggah kembali. Mohon dikonfirmasi bahwa alur ini sesuai, dan siapa yang menyediakan alat tanda tangan digital bila dipakai. |
| 16b | **Khusus KK0: kami mengasumsikan formulir kertas yang ditandatangani di lapangan itulah yang difoto/dipindai dan diunggah** — bukan hasil cetak ulang `.docx` dari sistem, karena Petugas Survei tidak punya akses sistem saat di lokasi (lihat [end-to-end-walkthrough](end-to-end-walkthrough.md), tahap Survey). Artinya `Unduh KK0 (.docx)` di sistem hanya menghasilkan salinan bersih untuk arsip internal, bukan dokumen yang perlu ditandatangani ulang oleh pelanggan. Mohon dikonfirmasi apakah asumsi ini sesuai praktik lapangan, atau apakah ada kunjungan kedua ke pelanggan untuk menandatangani salinan dari sistem. |
| 17 | Apakah PGN memiliki **ketentuan penetapan segmen** (Bronze/Silver/Gold/Platinum) berdasarkan volume atau nilai? |
| 18 | Apakah **Posisi Pelanggan** dan **Jalur Pipa** memang satu hal yang sama? Keduanya bernilai *Pengembangan / Jalur Existing*. |
| 19 | Pada anotasi Resume Evaluasi terdapat tulisan **`01/0RBB/300626`** tanpa keterangan. Apakah ini contoh **format nomor Nota Dinas**? Kami belum memakainya sampai ada konfirmasi. |
| 20 | **Lampiran 17 §8 (*Spread sheet Peralatan Gas*, profil beban 24 jam) akan dibuat sebagai formulir digital**, sesuai anotasi **`Entry Data`** dari PGN — bukan unggahan gambar seperti §9–§11 yang beranotasi `Upload Gambar`. Ini berarti 20 baris × 24 kolom diisi manual (tersedia tempel dari Excel). Mohon dikonfirmasi, karena inilah yang menentukan ukuran meter. |
| 21 | **Penyimpanan berkas — OneDrive.** Sistem mendukung MinIO (S3) dan OneDrive; pilihan cukup lewat konfigurasi, sehingga peralihan nanti **bukan migrasi data**. Untuk OneDrive kami memerlukan: (a) konfirmasi bahwa yang dipakai adalah **SharePoint / OneDrive for Business milik organisasi**, bukan OneDrive pribadi — berkas pada drive pribadi akan ikut hilang bila orangnya keluar; (b) **app registration** di Entra ID (tenant id, client id, client secret); (c) **admin consent** untuk izin aplikasi `Files.ReadWrite.All`; (d) **id document library** tujuan; (e) **kebijakan retensi** library tersebut — bila tenant menghapus atau mengarsipkan otomatis, NOL bertanda tangan ikut terkena. |

---

## 3. Temuan yang perlu perhatian

Beberapa hal yang kami temukan saat menelaah dokumen sumber:

**Nilai harga Bronze 1 sudah dikonfirmasi: 10.** Pada worksheet tertulis
`10.000`, ternyata salah baca pemisah ribuan — segmen lain berkisar 8,78–9,66
USD/MMBtu dan 10 sejalan dengan pola itu. Sudah kami masukkan sebagai data
awal.

**Dua basis kalor gas yang berbeda.** Worksheet memakai 9.000 kkal/m³; dokumen
resmi *Daftar Peralatan Gas* menyebut 8.750 kkal/m³. Kami memakai angka resmi
sebagai bawaan, dan nilainya dapat diubah administrator. Sejak `Konversi ke
Gas` menjadi isian manual, kedua angka ini murni acuan pembanding bagi
pengisi formulir — bukan lagi masukan untuk sebuah rumus.

**Sekitar 19 field pada formulir resmi belum ada di worksheet** — antara lain
NPWP dan salinannya (wajib menurut Lampiran 11), Nama Pimpinan Perusahaan, Jangka
Waktu Kontrak, Titik Koordinat, Willingness To Pay, dan tabel profil beban 24 jam
yang menentukan kapasitas meter. Semuanya sudah kami masukkan ke rancangan.

**Persetujuan dapat mengubah ketentuan.** Lampiran 16 memuat *Isi Persetujuan*
yang bisa berbeda dari *Isi Permohonan*, beserta *Kontrak Bersyarat*. Sistem
menyimpan keduanya secara terpisah.

**Unit organisasi (Region/Area) belum ada yang mengelola.** Pengguna dilekatkan
pada Area, dan Area berada di bawah Region, tetapi tidak ada dokumen sumber yang
menyebut siapa yang membuat unit tersebut. Harus disiapkan sebelum pengguna
pertama dapat diberi peran.

---

## 4. Yang tidak termasuk lingkup

Agar tidak ada salah paham, hal berikut **di luar** sistem ini:

- Setelah NOL terbit: pembayaran biaya penyambungan, penandatanganan **PJBG**,
  pemasangan infrastruktur, *gas in*, metering, penagihan
  *(langkah 3d–5 pada Diagram Alir 6.1)*
- Integrasi dengan ERP / sistem penagihan PGN
- Integrasi dengan direktori pengguna PGN *(ditunda — dikelola sendiri)*
- Notifikasi surel *(ditunda)*
- Portal untuk calon pelanggan — seluruh pengguna adalah internal PGN
- Sistem **Gate Review** itu sendiri (datanya diisi manual)
- Proses **FEED** (hanya dicatat statusnya)
- Peta jaringan pipa — PGN menyatakan data geometri tidak tersedia

---

## 5. Kesiapan sebelum sistem dapat dipakai

12 kelompok data harus disiapkan sebelum *go-live*. **Tiga di antaranya
menunggu masukan PGN** dan sudah dirinci pada bagian 2 di atas.

Urutan ketergantungan (yang paling awal harus lebih dulu):

1. Region (SOR I–IV) dan Area → *pengguna tidak dapat diberi peran tanpa ini*
2. Wilayah administratif + kode BPS
3. **Akun pengguna, peran, dan kata sandi sementara** 🔴
4. Satuan, jenis bahan bakar
5. Jenis industri, negara, segmen
6. Format penomoran dokumen

Tidak ada lagi "template alur kerja" untuk disiapkan — Admin Regional memilih
2–3 reviewer **per kasus**, saat kasus itu sampai ke mejanya, persis seperti
diminta pada notulen. Bukan jadwal yang disiapkan di muka.

Tidak ada lagi kelompok "faktor konversi", "matriks harga" atau "kurs" —
`Konversi ke Gas`, harga gas, dan IRR/NPV/Payback Period semuanya isian
manual, jadi ketiganya tidak menunggu data apa pun dari PGN untuk bisa
dipakai.

Tidak ada lagi "alokasi gas" atau "Gas Balance" untuk disiapkan atau
dikelola di sistem — PGN sudah mencatatnya sendiri di spreadsheet mereka.
Yang dibutuhkan alur kerja hanyalah satu angka manual (`Ketersediaan
Pasokan`) yang diisi Admin Regional saat evaluasi, sama seperti harga gas
dan IRR/NPV/Payback Period di atas.

---

## 6. Tahapan pembangunan yang diusulkan

| Tahap | Isi |
|---|---|
| 0 | Organisasi, **pengelolaan akun pengguna**, hak akses |
| 1 | Directory, Plotting, **peta** |
| 2 | **Lini masa status + laporan penuaan** ← menjawab masalah utama |
| 3 | Survei KK0 (isian manual) |
| 4 | A1 + pembuatan dokumen + unggah dokumen tertandatangan |
| 5 | Permohonan NOL + alur persetujuan + notifikasi |
| 6 | Evaluasi, Resume Evaluasi, penerbitan NOL/RL |
| 7 | Laporan, ekspor Excel |

Tahap 2 sengaja diletakkan sebelum formulir-formulir besar: keluhan utama adalah
soal visibilitas, sehingga lini masa di atas data tiga tahap lebih bernilai
daripada formulir KK0 sempurna tanpa tampilan status.

---

## 7. Ringkasan sumber

| Sumber | Peran |
|---|---|
| `1. Worksheet Sistem Sales Tools (1).xlsx` | Spesifikasi field dari PGN — 8 tahap, 54 catatan sel, 9 rumus |
| `Form Output Data.docx` | 17 tangkapan layar Prosedur `O-001/06.02` + **19 anotasi kotak teks** di atas gambar (alur persetujuan, otomasi, entri vs unggah) |
| `notulen.txt` | Notulen pertemuan — **acuan utama untuk alur kerja** |

Seluruh berkas sumber diarsipkan di [`source/`](source/) agar setiap pernyataan
dalam dokumen rancangan dapat ditelusuri kembali.

---

*Pertanyaan pada bagian 2 dapat dijawab bertahap — tidak ada lagi yang
menghentikan perhitungan inti. Nomor 1, 2 dan 3 (dulu 🔴, di bagian "sudah
kami selesaikan sendiri") sudah tidak menunggu jawaban PGN untuk kami mulai
membangun.*
