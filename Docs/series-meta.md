# Series meta — FileUploadProtector / Skrift article series

This file is **not published**. It serves as the single source of truth for tone,
terminology, facts, and editorial conventions used across the article series.
Copy-paste from here when writing a new article to stay consistent.

---

## Series identity

| Field | Value |
|---|---|
| Publication | [Skrift.io](https://skrift.io) |
| Series working title | *File Upload Protector* |
| Repository | https://github.com/PragmaticIT/PragmaticIT.Umbraco.FileUploadProtector |
| NuGet package | `PragmaticIT.Umbraco.FileUploadProtector` |
| License | MIT |

---

## Author bio (use verbatim or adapt lightly)

> Hubert is a consultant, software architect, developer and entrepreneur who has been
> building software since the Y2K era. A long-time C# / .NET enthusiast, he focuses on
> maintaining and evolving business-critical .NET applications, with a recent emphasis on
> Umbraco-based portals and DMS platforms. He enjoys translating between business
> stakeholders and development teams, and outside of work he's a husband, father of two
> boys, and an occasional djembe player.

---

## Package facts (versions as of writing)

| Item | Value |
|---|---|
| Package version | *(TBD)* |
| Target frameworks | net9.0, net10.0 |
| Minimum Umbraco | 15.0.0 |
| Maximum Umbraco (exclusive) | 18.0.0 |
| Key NuGet dependency | `Umbraco.Cms.Web.Common` |
| Umbraco version used in examples | 17.x |

---

## Core concept — one-liner

> File Upload Protector closes the gap between Umbraco's node-level Public Access and the physical files behind it — ensuring that a protected page's attachments are just as protected as the page itself.

---

## Motivation (use in introductions)

The story starts with a handover. A client asked me to take over support for an existing Umbraco 7 DMS — a system built and delivered by someone else, running in production for years. Before touching anything, I ran a set of routine security checks. It's what you do when you inherit someone else's code.

The site used Public Access to restrict content pages to logged-in members — contracts, purchase invoices, financial commitments, NDA-covered documents. Sensitive commercial material, not marketing brochures. The previous team had done that part right.

Then came the finding that nobody wanted to hear: those routine handover checks revealed that every file attached to those protected pages was freely downloadable by anyone who knew — or could guess — the URL.

Not a subtle edge case — confidential documents, sitting in a system that had been live for years, were effectively public to anyone who thought to try a direct URL. The previous team hadn't introduced a bug; they had simply trusted Umbraco to do something Umbraco had never done.

The fix at the time was a custom `IHttpModule` hooking into the IIS integrated-pipeline event cycle — the kind of solution that required `runAllManagedModulesForAllRequests="true"` in `web.config` and a good understanding of which pipeline event fires for static files. It worked, but it was tightly coupled to IIS and the classic ASP.NET runtime. Over the following years, as the Umbraco ecosystem moved through OWIN middleware to the ASP.NET Core pipeline, the solution evolved with it. Today it lives as a clean ASP.NET Core middleware component wired into Umbraco's request pipeline — same protection, none of the brittleness.

The underlying design gap hasn't changed. Umbraco's Public Access still protects nodes, not files. File Upload Protector fills that gap.

---

## Key types (use exact names in code samples)

| Type | Namespace | Role |
|---|---|---|
| *(TBD)* | `PragmaticIT.Umbraco.FileUploadProtector` | *(TBD)* |

---

## appsettings.json — all defaults

```json
{
  "FileUploadProtector": {
  }
}
```

---

## High-level architecture (reference for diagrams)

*(TBD — ASCII diagram of the request/processing pipeline.)*

---

## Editorial conventions

- **Product name in prose:** "File Upload Protector" (three words, capital F, U, P, no trademark symbol).
- **Package name in code/paths:** `PragmaticIT.Umbraco.FileUploadProtector` (exact casing).
- **Tone:** conversational but technically precise; first-person plural ("we") for narrative, second-person ("you") for instructions.
- **Code blocks:** C# and JSON examples must compile/validate against the versions in the table above.
- **The design gap framing:** always worth stating explicitly — Umbraco's Public Access protects *nodes*, not *files*. This is the core problem the package solves.
- **Evolution narrative:** the solution has a history (IIS → OWIN → ASP.NET Core middleware); use it to build credibility but don't dwell on legacy details.
- **Not a media CDN or DRM solution:** be explicit about scope — the package secures file downloads against unauthenticated access; it does not encrypt files, manage CDN signing, or provide fine-grained per-file permissions.

---

## Published articles

| # | Title | Skrift URL | Status |
|---|---|---|---|
| *(none yet)* | | | |

---

## Ideas for future articles

| Topic | Brief description | Suggested angle |
|---|---|---|
| *(TBD)* | | |

---

## Test scenario matrix

The proof-of-concept site (`PragmaticIT.Umbraco.FileUploadProtector.Web`) is seeded with
the following content tree. Use this as the reference fixture for all manual and automated tests.

### Content tree

```
Home  (page, public)
├── Site A  (page)  →  Public Access: members of group "Org A"  →  /site-a
│   └── Document A  (document)  →  /media/stvbzipd/a.txt
├── Site B  (page)  →  Public Access: members of group "Org B"  →  /site-b
│   └── Document B  (document)  →  /media/wkxbwqhb/b.txt
├── Document C  (document, no parent protection)  →  /media/h1teii2c/c.txt
├── Login  (utilityPage, template=login)  →  /login
└── No Access  (utilityPage, template=noAccess)  →  /no-access
```

### Test users / member groups

| Username | Member group | Expected access |
|---|---|---|
| `member-a` | Org A | Site A + its files |
| `member-b` | Org B | Site B + its files |
| *(anonymous)* | — | Public pages and Document C only |

### Scenarios

Each scenario is tested **without** FileUploadProtector (expected baseline) and **with** it (expected result).

---

#### TC-01 — Anonymous user visits public page

| | |
|---|---|
| **Actor** | Anonymous |
| **Action** | `GET /` |
| **Without FUP** | `200` |
| **With FUP** | `200` |
| **Notes** | Baseline — nothing should change for unprotected pages. |

---

#### TC-02 — Anonymous user visits protected page

| | |
|---|---|
| **Actor** | Anonymous |
| **Action** | `GET /site-a` |
| **Without FUP** | `302 → /login` (Umbraco Public Access) |
| **With FUP** | `302 → /login` |
| **Notes** | FUP must not interfere with Umbraco's own page-level redirect. |

---

#### TC-03 — Anonymous user downloads file from protected page ⚠️ core scenario

| | |
|---|---|
| **Actor** | Anonymous |
| **Action** | `GET /media/stvbzipd/a.txt` (file under Site A) |
| **Without FUP** | `200` 🔴 file served freely despite page being protected |
| **With FUP** | `302 → /login` ✅ |
| **Notes** | This is the fundamental design gap FUP closes. |

---

#### TC-04 — Authorised member downloads their own file

| | |
|---|---|
| **Actor** | `member-a` (Org A) |
| **Action** | `GET /media/stvbzipd/a.txt` |
| **Without FUP** | `200` |
| **With FUP** | `200` ✅ |
| **Notes** | Legitimate access must never be blocked. |

---

#### TC-05 — Authorised member visits their own protected page

| | |
|---|---|
| **Actor** | `member-a` (Org A) |
| **Action** | `GET /site-a` |
| **Without FUP** | `200` |
| **With FUP** | `200` ✅ |
| **Notes** | Page access must remain unaffected. |

---

#### TC-06 — Wrong-group member downloads file from the other site ⚠️ cross-org isolation

| | |
|---|---|
| **Actor** | `member-b` (Org B, logged in) |
| **Action** | `GET /media/stvbzipd/a.txt` (file under Site A) |
| **Without FUP** | `200` 🔴 file served despite member belonging to wrong group |
| **With FUP** | `302 → /no-access` ✅ |
| **Notes** | Proves per-organisation file isolation. |

---

#### TC-07 — Wrong-group member visits the other site's protected page

| | |
|---|---|
| **Actor** | `member-b` (Org B, logged in) |
| **Action** | `GET /site-a` |
| **Without FUP** | `302 → /no-access` (Umbraco Public Access) |
| **With FUP** | `302 → /no-access` |
| **Notes** | FUP must not interfere with Umbraco's own no-access redirect. |

---

#### TC-08 — Anyone downloads publicly available file

| | |
|---|---|
| **Actor** | Anonymous |
| **Action** | `GET /media/h1teii2c/c.txt` (Document C, under public Home) |
| **Without FUP** | `200` |
| **With FUP** | `200` ✅ |
| **Notes** | FUP must not block files whose ancestor tree contains no Public Access rule. |

---

#### TC-09 — Authorised member downloads public file

| | |
|---|---|
| **Actor** | `member-a` (Org A) |
| **Action** | `GET /media/h1teii2c/c.txt` |
| **Without FUP** | `200` |
| **With FUP** | `200` ✅ |
| **Notes** | Logged-in users must also reach public files without friction. |

---

#### TC-10 — Wrong-group member downloads public file

| | |
|---|---|
| **Actor** | `member-b` (Org B) |
| **Action** | `GET /media/h1teii2c/c.txt` |
| **Without FUP** | `200` |
| **With FUP** | `200` ✅ |
| **Notes** | Public files stay public regardless of the caller's group membership. |

---

## Repository structure (quick reference)

```
/
├── Docs/
│   ├── USAGE.md            Step-by-step integration guide
│   ├── IMPLEMENTATION.md   Architecture & internals
│   ├── SEED.md             Auto-seed infrastructure
│   ├── README.nuget.md     NuGet.org package description
│   ├── skrift-article.md   Article 1 draft
│   └── series-meta.md      ← this file
│
├── Source/
│   ├── PragmaticIT.Umbraco.FileUploadProtector/          The NuGet library
│   └── PragmaticIT.Umbraco.FileUploadProtector.Web/      Local test host
│
└── Examples/
    └── FileUploadProtectorTest/  Minimal sample app (references NuGet package)
```
