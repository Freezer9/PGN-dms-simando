# Geography seed data

`provinces.csv` / `regencies.csv` / `districts.csv` / `villages.csv` --
`code|name` per line, one file per level of the Kemendagri administrative
hierarchy. `code` is the dotted Kemendagri code (e.g. `35.78.13.1001`); a
row's parent is always that same code with its last `.` segment removed, so
`GeographySeeder` (`src/Simando.Infrastructure/Geography/GeographySeeder.cs`)
walks the hierarchy from the codes alone, no separate parent column needed.

Source: [`db/wilayah.sql`](https://github.com/cahyadsn/wilayah/blob/master/db/wilayah.sql)
from [cahyadsn/wilayah](https://github.com/cahyadsn/wilayah) (MIT licensed) --
the canonical dataset the wilayah.id API itself is built from, current as of
Kepmendagri No 300.2.2-2138 Tahun 2025. It's a single `wilayah(kode, nama)`
table covering all four levels in one dump; these four files are that dump
split by code depth (1 segment = province, 2 = regency, 3 = district, 4 =
village) and re-encoded as `|`-delimited text instead of SQL `INSERT`
statements.

## Regenerating

```bash
curl -sL https://raw.githubusercontent.com/cahyadsn/wilayah/master/db/wilayah.sql -o wilayah.sql
python3 - wilayah.sql <<'EOF'
import re, sys
with open(sys.argv[1] if len(sys.argv) > 1 else "wilayah.sql", encoding="utf-8") as f:
    text = f.read()
# nama can contain SQL's doubled-quote escape ('' -> literal '), e.g. ('12.04.28','Ma''u').
pattern = re.compile(r"\('([0-9.]+)','((?:[^']|'')*)'\)")
rows = [(code, name.replace("''", "'")) for code, name in pattern.findall(text)]
levels = {1: [], 2: [], 3: [], 4: []}
for code, name in rows:
    levels[code.count(".") + 1].append((code, name))
names = {1: "provinces.csv", 2: "regencies.csv", 3: "districts.csv", 4: "villages.csv"}
for depth, filename in names.items():
    with open(filename, "w", encoding="utf-8") as out:
        for code, name in levels[depth]:
            out.write(f"{code}|{name}\n")
EOF
```

Then copy the four output files over the ones in this folder.
`GeographySeeder` only runs when the `province` table is empty, so a
reorganisation should retire superseded rows directly against the database
(`deleted_at`, never a hard delete or a rerun of the seeder) rather than
regenerating and reseeding -- see docs/domain/master-data.md §4.
