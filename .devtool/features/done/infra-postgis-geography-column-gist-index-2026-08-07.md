---
id: "infra-postgis-geography-column-gist-index-2026-08-07"
status: "done"
priority: "high"
assignee: null
dueDate: null
created: "2026-08-07T09:00:00.000Z"
modified: "2026-08-07T20:34:19.000Z"
completedAt: "2026-08-07T20:34:19.000Z"
labels: ["company", "phase-1", "infra"]
order: "a00a"
---

# Infra: PostGIS geography column + GiST index

`company.location` as `geography(Point,4326)` via NetTopologySuite, not two decimal columns — the map does radius and bbox queries. GiST index for map performance.
