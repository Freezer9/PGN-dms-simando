# Build — Blazor Server Conventions

> **Canonical.** This document owns the interactive-render-mode rules and the
> component-vs-endpoint split. [architecture](architecture.md) references it
> rather than restating it.

## Interactivity is global, not per-page

`App.razor` sets `@rendermode InteractiveServer` on `<Routes>`, not on individual
pages. A page's own `@rendermode` does not extend to its **Layout** — confirmed the
hard way building the app shell (Task 6): a plain `@onclick` placed directly in
`MainLayout` never fired until interactivity was made global, because the layout
that wraps `@Body` is a separate component the page's render mode doesn't reach.

Per-page interactivity (the alternative BlazorBlueprint's own setup docs describe)
only works cleanly when the layout itself has nothing interactive. Once the shell's
sidebar, dropdowns and collapsible groups live in the layout, global is the only
option that doesn't mean re-declaring `@rendermode` on every page *and* somehow
making the layout interactive without receiving `@Body` across a render-mode
boundary — which Blazor rejects outright (`InvalidOperationException: Cannot pass
the parameter 'Body' ... arbitrary code and cannot be serialized`).

## Anything that mutates the HTTP response belongs in a Controller, not a component

An interactive component's C# runs over the SignalR circuit, not inside the
original HTTP request. By the time its event handlers or lifecycle methods run,
the response may already be sent (prerender) or the request may not be a fresh
HTTP request at all (a same-circuit Blazor navigation, including plain `<a href>`
clicks — Blazor's enhanced navigation intercepts those too). Either way, writing
response headers at that point fails or silently does nothing:

- `SignInManager.SignOutAsync()` called from a Razor component's
  `OnInitializedAsync` worked fine when the page was static (every visit was a
  fresh request), and broke the moment the shell went interactive: the sign-out
  link became a same-circuit navigation, and the cookie write failed with
  `Headers are read-only, response has already started`.
- The sign-in and change-password forms hit this from the start: `@rendermode
  InteractiveServer` on those pages was never optional (BlazorBlueprint's own
  components need it), so their cookie-writing was routed around the component
  tree from day one.

**The rule:** any action that must write a cookie, set a response header, or
stream a file needs a plain ASP.NET Core endpoint, reached via a native `<form>`
POST or `<a href>` GET — never a Razor component's own handler. This project uses
MVC controllers under `Simando.Web/Controllers/`, routed **by resource, not under
`/api/...`** — e.g. `/account/sign-in`, `/attachments/{id}/download`. These
endpoints return redirects or file streams to the same Blazor app, not JSON to an
external client, so an `/api` prefix would misdescribe them: it invites the next
contributor to assume REST conventions (content negotiation, versioning) that
don't apply here. (`AccountController` is the reference implementation: sign-in,
change-password, sign-out, mounted at `/account`.) Antiforgery validation for
these actions is done manually
(`IAntiforgery.ValidateRequestAsync`) inside each action, since the tokens
`<AntiforgeryToken />` renders come from the same underlying service.

**Prefer constructor injection here, not minimal-API delegate parameters.**
`SignInManager<TUser>` implements ASP.NET Core Identity's
`IEndpointParameterMetadataProvider`, which auto-attaches an authorization
requirement when the type is declared as a *minimal API delegate parameter* —
one that was observed to survive `.AllowAnonymous()` on the endpoint. Controller
constructor injection doesn't go through that metadata-building path, so this
doesn't apply there; it's specifically a minimal-API pitfall, and part of why
this project uses controllers for identity-touching endpoints rather than
`app.MapPost(...)`.

## Where this applies next

Every future feature that streams a file to the browser needs the same
treatment — a controller action, not a component:

- **Attachment downloads** — already specified as "an authorised endpoint that
  re-checks scope" ([architecture §Security](architecture.md#security),
  [storage §Access control is ours](storage.md#access-control-is-ours)).
  No pre-signed URLs, so the download has to stream through the app; that stream
  is a controller action.
- **Generated `.docx` downloads** — the [Document
  Generator](architecture.md#document-generator) merges a template and hands
  the user a file; same shape as an attachment download.
- **Excel export** ([design/reporting](../design/reporting.md)) — ClosedXML
  writes a workbook to a stream; the browser needs a real HTTP response with a
  `Content-Disposition` header to save it, not a component render.

None of these are built yet. When they are, they belong alongside
`AccountController` — a `AttachmentsController`, `DocumentsController`, or similar
under `Simando.Web/Controllers/`, not a Razor component or a `MapGet` lambda in
`Program.cs`. Route each by resource, the same way: `/attachments/{id}/download`,
`/documents/{id}/download`, `/reports/export` — no `/api` prefix, for the same
reason as `AccountController` above.

## Middleware exemptions live at the endpoint, not in the middleware

`MustChangePasswordMiddleware` (and any future gate that runs for every request —
a maintenance-mode check, a terms-of-service gate) needs a small set of routes to
stay reachable regardless. The temptation is to hand-list those paths inside the
middleware's exemption check. Don't: a hand-listed path silently drifts out of
sync the moment a route is renamed or a new one needs the same exemption, and
nothing fails loudly when it does — the user is just redirect-looped instead.

Read exemptions from **endpoint metadata** instead, the same way ASP.NET Core's
own `IAllowAnonymous` already works: `context.GetEndpoint()?.Metadata`. Static
assets carry it via `MapStaticAssets().AllowAnonymous()`; app routes that need a
custom exemption declare a marker attribute at the endpoint itself —
`[AllowDuringPasswordChange]` on `ChangePassword.razor`'s `@attribute` line and on
the relevant `AccountController` actions (`Simando.Web/Security/`
`AllowDuringPasswordChangeAttribute.cs`) — so the exemption travels with the route
it protects rather than living as a string the middleware has to keep in sync by
hand.

The one exception that stays hardcoded is `/_blazor`: it's the SignalR circuit
hub, mapped internally by `AddInteractiveServerRenderMode()`, not an endpoint this
app defines — there is no attribute to attach it to.
