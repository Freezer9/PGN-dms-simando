---
id: "add-pic-contact-person-entity-supporting-multiple-2026-08-05"
status: "done"
priority: "high"
assignee: null
dueDate: null
created: "2026-08-05T09:00:00.000Z"
modified: "2026-08-30T00:00:00.000Z"
completedAt: "2026-08-30T00:00:00.000Z"
labels: ["epic-pic", "backend", "data-model"]
order: "a6"
---
# Add PIC (contact person) entity supporting multiple contacts per company

Add a dedicated PIC (Person In Charge / contact person) entity so each subscription can record its company contacts, instead of having no contact data at all.

## Context
Form A1 requires two PIC contact blocks per company: Nama, Jabatan, Email, No HP, LinkedIn, IG, FB. None of this exists in `Api/Data/Entities.cs` today — `Subscription` has no contact fields whatsoever.

## Acceptance Criteria
- [x] `PicContact` entity added (Nama, Jabatan, Email, NoHp, Linkedin, Instagram, Facebook, SubscriptionId FK)
- [x] Supports 1-N contacts per subscription (spec shows 2, but don't hardcode a max of 2 in the schema)
- [x] EF Core migration created and applied
- [x] DTOs added to `Shared/Models.cs`
