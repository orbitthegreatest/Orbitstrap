# Orbitstrap Publishing Guide

A step-by-step guide for shipping a new Orbitstrap release and keeping the website + GitHub repo in sync with it.

---

## 1. Versioning

Orbitstrap follows plain semantic versioning: `vMAJOR.MINOR.PATCH` (e.g. `v1.4.2`).

- **MAJOR** — breaking changes (settings format changes, a mod removed, etc.)
- **MINOR** — new features (a new mod, a new tab, a new integration)
- **PATCH** — bug fixes only

Tag names always start with a lowercase `v` (`v1.4.2`, not `1.4.2`) — the website's version
display and GitHub's own UI both expect this format.

---

## 2. Build the release exe

From the repo root, on a Windows machine with the .NET 8+ SDK installed:

```
build.bat
```

This runs `dotnet restore` then `dotnet publish` against `Orbitstrap\Orbitstrap.csproj` as a
self-contained, single-file `win-x64` release. (The old `orbitstrap_modified` duplicate folder was
deleted and this is now the only source folder — see `MASTER_PLAN.md` for the resolution.)

When it finishes, the exe is at:

```
publish_output\Orbitstrap.exe
```

Sanity-check before publishing:
- Launch it once on a clean-ish machine (or a VM) and confirm the app opens without a missing
  `.dll` error — self-contained publishes occasionally miss a native dependency if a new
  NuGet package was added without a `runtimes\` entry.
- Confirm the version shown in the app's About page matches the tag you're about to create.

---

## 3. Create the GitHub release

### Option A — GitHub CLI (fastest)

```
gh auth login
gh release create v1.4.2 "publish_output\Orbitstrap.exe" ^
    --repo orbitthegreatest/Orbitstrap ^
    --title "Orbitstrap v1.4.2" ^
    --notes "Describe what changed in this release."
```

**Important:** the asset must be uploaded with the exact filename `Orbitstrap.exe` — the
website's Download buttons link directly to
`github.com/orbitthegreatest/Orbitstrap/releases/latest/download/Orbitstrap.exe`, which is
GitHub's built-in "always the newest release's asset with this exact name" redirect. If the file
is renamed on upload (e.g. `Orbitstrap-v1.4.2.exe`), the website's download buttons will break
until the site is updated — so don't rename it on upload.

### Option B — GitHub web UI

1. Go to `github.com/orbitthegreatest/Orbitstrap/releases` → **Draft a new release**.
2. Tag: `v1.4.2` (create the tag from this screen if it doesn't exist yet).
3. Title: `Orbitstrap v1.4.2`.
4. Release notes: a short changelog — what was added/fixed, not a wall of commit messages.
5. Drag `publish_output\Orbitstrap.exe` into the assets box — **don't rename it**.
6. Publish release.

---

## 4. Automating builds with GitHub Actions (optional, not yet set up)

Once you're comfortable with the manual flow above, the next step is a workflow that builds and
attaches the exe automatically whenever you push a `v*` tag, so you never have to run `build.bat`
locally. A minimal version of that workflow:

```yaml
# .github/workflows/release.yml
name: Build and Release

on:
  push:
    tags:
      - 'v*'

jobs:
  build:
    runs-on: windows-latest
    steps:
      - uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'

      - name: Restore
        run: dotnet restore Orbitstrap.sln

      - name: Publish
        run: >
          dotnet publish Orbitstrap/Orbitstrap.csproj
          -c Release -r win-x64 --self-contained true
          -p:PublishSingleFile=true -p:IncludeAllContentForSelfExtract=true
          -o publish_output

      - name: Create Release
        uses: softprops/action-gh-release@v2
        with:
          files: publish_output/Orbitstrap.exe
          generate_release_notes: true
```

This isn't in the repo yet — add it under `.github/workflows/release.yml` when you're ready, then
publishing a new version becomes: bump the version, `git tag v1.4.2`, `git push --tags`, and let
the workflow build + attach the exe for you.

---

## 5. Deploying the website

The website lives at `website/index.html` (+ `website/assets/`, `website/sitemap.xml`,
`website/robots.txt`) in this repo — pure HTML/CSS/JS, no build step.

1. Go to [vercel.com](https://vercel.com) and sign in with GitHub.
2. **Add New Project** → import `orbitthegreatest/Orbitstrap`.
3. Set **Root Directory** to `website` (Vercel will otherwise try to build from the repo root).
4. Framework preset: **Other** (it's static HTML, no framework to detect).
5. Deploy. Vercel gives you a `orbitstrap.vercel.app` URL (or your own subdomain if that one's
   taken) — this is the URL already baked into the site's meta tags, JSON-LD, `sitemap.xml`, and
   `robots.txt`, so no further edits are needed if you get that exact subdomain.
6. Every future push to the repo's default branch redeploys the site automatically — no separate
   "publish" step for website changes.

If you end up with a different subdomain or a custom domain, update these files to match (search
for `orbitstrap.vercel.app`):
- `website/index.html` (canonical URL, OG/Twitter tags, JSON-LD `url`)
- `website/sitemap.xml`
- `website/robots.txt`

---

## 6. Submitting to Google Search Console

1. Deploy the site first (step 5) so you have a live URL.
2. Go to [search.google.com/search-console](https://search.google.com/search-console/).
3. **Add property** → **URL prefix** → enter your Vercel URL (e.g. `https://orbitstrap.vercel.app`).
4. Verify ownership. The simplest method on Vercel: the **HTML file** verification option — Google
   gives you a file like `google1234567890abcdef.html`; drop it into `website/` and push, Vercel
   redeploys it automatically, then click Verify.
5. Once verified, go to **Sitemaps** in the left nav → submit `sitemap.xml`
   (`https://orbitstrap.vercel.app/sitemap.xml`).
6. Use **URL Inspection** on the homepage URL → **Request Indexing**.

Google typically indexes a new, verified site within 1–4 days of submission, sometimes longer for
a brand-new domain with no existing backlinks.

---

## 7. README badges

The README already includes GitHub release/download/star badges via shields.io, pointed at
`orbitthegreatest/Orbitstrap` — these update automatically as soon as a new release/tag exists, no
manual edits needed. Nothing to do here unless the repo is ever renamed or transferred.

---

## 8. Release checklist (copy this each time)

- [ ] Version bumped, follows `vMAJOR.MINOR.PATCH`
- [ ] `build.bat` run on Windows, exe launches cleanly
- [ ] About page version matches the tag
- [ ] Release created with asset named exactly `Orbitstrap.exe`
- [ ] Release notes describe what changed, not raw commit log
- [ ] `MASTER_PLAN.md` conversation log updated if this release includes work tracked there
