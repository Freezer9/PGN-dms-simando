---
name: form-design-and-validation
description: >
  Playbook and rules for form design, layout spacing, mandatory field asterisk indicators,
  TanStack Form architecture, and Zod validation in Simando DMS. Use when building or updating
  React forms, inputs, select fields, date pickers, or modal dialog forms.
---

# Form Design & Validation Playbook

This playbook defines mandatory rules and architectural patterns for all form controls, layout grids, validation logic, and modal dialog forms across Simando DMS using **React 19**, **@tanstack/react-form**, **Zod**, and **shadcn/ui**.

---

## 1. Mandatory Field Asterisk (`*`) Rule

- **Rule**: Every mandatory/required form field label **MUST explicitly include an asterisk (`*`)** formatted with danger/destructive styling:
  ```tsx
  <Label htmlFor="name">
    Nama Perusahaan <span className="text-destructive">*</span>
  </Label>
  ```
- **Optional Fields**: Optional fields **MUST NOT** include an asterisk in their label text.

---

## 2. Form Layout & Grid Spacing Guidelines

- **Grid Separation**: Multi-column form layouts MUST use clean grid spacing (`gap-4` or `gap-6`) to prevent controls from touching or overlapping horizontally:
  ```tsx
  <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
    {/* Form Fields */}
  </div>
  ```
- **Full Width Wiring**: Ensure form inputs span their container width (`w-full`) so responsiveness is maintained across desktop and tablet viewports.

---

## 3. TanStack Form & Zod Validation Architecture

- **Schema Definition**: Define Zod validation schemas matching backend OpenAPI contracts:
  ```typescript
  import { z } from 'zod';

  export const createCompanySchema = z.object({
    name: z.string().min(1, 'Nama perusahaan wajib diisi').max(200),
    areaId: z.string().uuid('Area wajib dipilih'),
    segmentId: z.string().uuid('Segmen wajib dipilih'),
    picName: z.string().min(1, 'Nama PIC wajib diisi'),
    picPhone: z.string().min(8, 'Nomor telepon minimal 8 digit'),
    estimatedGasDemand: z.number().positive('Perkiraan kebutuhan gas harus lebih dari 0'),
  });

  export type CreateCompanyFormValues = z.infer<typeof createCompanySchema>;
  ```

- **Standard Form Hook Setup**:
  ```tsx
  import { useForm } from '@tanstack/react-form';
  import { zodValidator } from '@tanstack/zod-form-adapter';
  import { toast } from 'sonner';
  import { $api } from '@/api/client';
  import { Button } from '@/components/ui/button';
  import { Input } from '@/components/ui/input';
  import { Label } from '@/components/ui/label';

  export function CreateCompanyDialog({ open, onOpenChange, onSuccess }: Props) {
    const createMutation = $api.useMutation('post', '/api/companies');

    const form = useForm({
      defaultValues: {
        name: '',
        areaId: '',
        segmentId: '',
        picName: '',
        picPhone: '',
        estimatedGasDemand: 0,
      } as CreateCompanyFormValues,
      validatorAdapter: zodValidator(),
      validators: {
        onChange: createCompanySchema,
      },
      onSubmit: async ({ value }) => {
        try {
          await createMutation.mutateAsync({ body: value });
          toast.success('Perusahaan berhasil ditambahkan', { description: value.name });
          onSuccess();
          onOpenChange(false);
        } catch (error) {
          toast.error('Gagal menambahkan perusahaan', {
            description: (error as Error).message ?? 'Terjadi kesalahan pada server',
          });
        }
      },
    });

    return (
      <form
        onSubmit={(e) => {
          e.preventDefault();
          e.stopPropagation();
          form.handleSubmit();
        }}
        className="space-y-4"
      >
        <form.Field
          name="name"
          children={(field) => (
            <div className="space-y-1">
              <Label htmlFor={field.name}>
                Nama Perusahaan <span className="text-destructive">*</span>
              </Label>
              <Input
                id={field.name}
                value={field.state.value}
                onBlur={field.handleBlur}
                onChange={(e) => field.handleChange(e.target.value)}
                placeholder="PT Contoh Energi Nusantara"
              />
              {field.state.meta.errors.length > 0 && (
                <p className="text-xs text-destructive">{field.state.meta.errors[0]}</p>
              )}
            </div>
          )}
        />

        <div className="flex justify-end gap-2 pt-4">
          <Button variant="outline" type="button" onClick={() => onOpenChange(false)}>
            Batal
          </Button>
          <Button type="submit" disabled={createMutation.isPending}>
            {createMutation.isPending ? 'Menyimpan...' : 'Simpan'}
          </Button>
        </div>
      </form>
    );
  }
  ```

- **Dynamic Field Arrays (e.g. Survey Equipment)**: Use TanStack Form field array helpers (`field.pushValue`, `field.removeValue`) to manage repeating tabular rows cleanly.

---

## 4. Modal Dialogs vs. Inline Table Row Editing

- **Avoid Inline Table Editing**: Do not squeeze text inputs and buttons directly into narrow data table cells. Squeezing multiple inputs into table rows causes severe horizontal squishing, truncated placeholders, and UI breakage on narrow screens.
- **Use Dedicated Dialog Modals (`<Dialog>`)**:
  - Render data tables as clean read-only display rows with compact action buttons (`Pencil` edit, `Trash2` delete).
  - Open a dedicated `<Dialog>` or `<Sheet>` for creating or editing row items.

---

## 5. Metric Summaries & Calculated Totals

- **Single Prominent Display Card**: Derived calculations and calculated totals (e.g. `Jumlah Kebutuhan Energi`) MUST be computed with `useMemo` from form state and rendered in a prominent metric card near the source dataset (e.g. right below the equipment table).
- **No Duplication**: Calculated summary metrics MUST NOT be duplicated across multiple section cards or rendered as editable form input fields.

---

## 6. Toast Notification Feedback

- **Toast System**: Use `sonner` (`toast.success`, `toast.error`, `toast.info`).
- **Success Feedback**: Upon successful form submission, show `toast.success("Perusahaan berhasil disimpan.")`.
- **Error Feedback**: On validation or mutation failure, show `toast.error("Gagal menyimpan data.", { description: err.message })`.
