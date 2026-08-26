---
name: form-design-and-validation
description: >
  Playbook and rules for form design, layout spacing, mandatory field asterisk indicators,
  and DataAnnotations validation architecture in Simando DMS. Use when building or updating
  Blazor forms, inputs, select fields, date pickers, or modal dialog forms.
---

# Form Design & Validation Playbook

This playbook defines mandatory rules and architectural patterns for all form controls, layout grids, validation logic, and modal dialog forms across Simando DMS.

---

## 1. Mandatory Field Asterisk (`*`) Rule

- **Rule**: Every mandatory/required form field label **MUST explicitly include an asterisk (`*`)** at the end of the label text (e.g. `Label="Nama *"`).
- **Required Property Wiring**: Always pair the label asterisk with `Required="true"` on BlazorBlueprint form field components so ARIA accessibility flags match visual indicators:
  ```razor
  <BbFormFieldInput TValue="string" @bind-Value="_form.Name" Label="Nama *" Required="true" />
  <BbFormFieldSelect TValue="Guid?" @bind-Value="_form.AreaId" Label="Area *" Required="true" Options="_areaOptions" />
  <BbFormFieldDatePicker @bind-Value="_form.Tanggal" Label="Tanggal Survei *" Required="true" />
  ```
- **Optional Fields**: Optional fields **MUST NOT** include an asterisk in their label text.

---

## 2. Form Layout & Grid Spacing Guidelines

- **Grid Separation**: Multi-column form layouts MUST use clean grid spacing (`gap-4` or `gap-6`) to prevent controls from touching or overlapping horizontally.
- **Full Width Parameter Wiring (`Class` & `InputClass`)**:
  - `Class="w-full"` sets the outer field container width.
  - `InputClass="w-full"` sets the inner control width (trigger button, text box, select element).
  - Always provide both `Class="w-full" InputClass="w-full"` on form fields inside grid columns to prevent controls from snapping together or rendering auto-width:
    ```razor
    <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
        <BbFormFieldDatePicker @bind-Value="_date" Label="Tanggal *" Required="true" Class="w-full" InputClass="w-full" />
        <BbFormFieldSelect TValue="Guid?" @bind-Value="_selectId" Label="Surveyor *" Required="true" Options="_options" Class="w-full" InputClass="w-full" />
    </div>
    ```

---

## 3. DataAnnotations Validation Architecture

- **Standard Form Architecture**: Wrap forms in `<EditForm Model="model" OnValidSubmit="...">` and include `<DataAnnotationsValidator />`:
  ```razor
  <EditForm Model="_form" OnValidSubmit="SaveAsync">
      <DataAnnotationsValidator />
      <BbDialogHeader>
          <BbDialogTitle>@(_editingId is null ? "Tambah Data" : "Ubah Data")</BbDialogTitle>
      </BbDialogHeader>
      <div class="space-y-4 py-4">
          <BbFormFieldInput TValue="string" @bind-Value="_form.Code" Label="Kode *" Required="true" />
          <BbFormFieldInput TValue="string" @bind-Value="_form.Name" Label="Nama *" Required="true" />
          @if (_error is not null)
          {
              <BbAlert Variant="AlertVariant.Danger">
                  <BbAlertDescription>@_error</BbAlertDescription>
              </BbAlert>
          }
      </div>
      <BbDialogFooter>
          <BbButton Variant="ButtonVariant.Outline" Type="ButtonType.Button" OnClick="CloseDialog">Batal</BbButton>
          <BbButton Type="ButtonType.Submit">Simpan</BbButton>
      </BbDialogFooter>
  </EditForm>
  ```
- **Form Model Classes**: Define nested `sealed class FormModel` inside `@code` blocks with DataAnnotation attributes:
  ```csharp
  private sealed class FormModel
  {
      [Required(ErrorMessage = "Kode wajib diisi")]
      [StringLength(20, ErrorMessage = "Kode maksimal 20 karakter")]
      public string Code { get; set; } = "";

      [Required(ErrorMessage = "Nama wajib diisi")]
      [StringLength(100, ErrorMessage = "Nama maksimal 100 karakter")]
      public string Name { get; set; } = "";
  }
  ```

---

## 4. Modal Dialogs vs. Inline Table Row Editing

- **Avoid Inline Table Editing**: Do not squeeze form inputs into narrow data table cells (`BbTableCell`). Squeezing multiple inputs into table cells causes horizontal squishing, truncated placeholders (`Nama peralat...`), and action button collisions on narrow viewports.
- **Use Dedicated Dialog Modals (`<BbDialog>`)**:
  - Render data tables as clean read-only display rows with compact action icons (`pencil` edit, `trash-2` delete).
  - Open a spacious `<BbDialog>` modal dialog for creating or editing row items.

---

## 5. Metric Summaries & Calculated Totals

- **Single Prominent Display Card**: Derived calculations and calculated totals (e.g. `Jumlah Kebutuhan Energi`) MUST be rendered in a single prominent metric card near the source dataset (e.g. right below the equipment table).
- **No Duplication**: Calculated summary metrics MUST NOT be duplicated across multiple section cards or rendered as editable form input fields.

---

## 6. Toast Notification Feedback

- **Inject ToastService**: Always inject `@inject ToastService ToastService` in form components.
- **Success & Error Feedback**: Upon successful form submission, trigger a success toast (e.g. `ToastService.Success("Formulir berhasil disimpan.", "Tersimpan");`). On error or failure, trigger an error toast (e.g. `ToastService.Error(result.Error ?? "Gagal menyimpan formulir.", "Gagal");`).

