# UI Redesign Plan — StadiumTracker

## Context

The current layout uses a 250px left sidebar with a dark blue-to-purple gradient. The user wants:
1. A red/white/blue color scheme (matching the sports league aesthetic)
2. A light/dark mode toggle
3. The three admin links consolidated in one place (currently scattered in the sidebar)
4. The left sidebar removed entirely — replaced with a horizontal top navbar

## Approach

Replace the sidebar layout with a **sticky horizontal top navbar**. Use CSS custom properties for all theme colors so that a single `data-bs-theme` attribute swap on `<html>` toggles between light and dark modes (leveraging Bootstrap 5.3's native dark mode support). Add R/W/B brand colors as CSS variable overrides. Consolidate admin links into a single Bootstrap dropdown in the navbar (admin-only, via `<AuthorizeView>`). Add Google Fonts for a more distinctive sports aesthetic.

---

## Critical Files to Modify

| File | Change |
|------|--------|
| `StadiumTracker/Components/App.razor` | Add Google Fonts link; add inline theme-init script (reads localStorage, sets `data-bs-theme` on `<html>` before paint to prevent flash) |
| `StadiumTracker/Components/Layout/MainLayout.razor` | Remove `<div class="sidebar"><NavMenu /></div>`; replace top-row with NavMenu spanning full width; remove the MS docs "About" link |
| `StadiumTracker/Components/Layout/MainLayout.razor.css` | Full rewrite — remove sidebar styles, keep only content/article/error-ui styles |
| `StadiumTracker/Components/Layout/NavMenu.razor` | Full rewrite as horizontal Bootstrap navbar: brand left, nav links center/left, Admin dropdown (admin-only), dark/light toggle + user icon right |
| `StadiumTracker/Components/Layout/NavMenu.razor.css` | Full rewrite — horizontal nav styling, theme-aware colors, active link indicator |
| `StadiumTracker/wwwroot/app.css` | Add CSS custom properties for R/W/B theme colors (light and dark variants); update `btn-primary`, link colors, etc. to use the new variables |

---

## Design Details

### Typography
- **Display/Brand**: [Bebas Neue](https://fonts.google.com/specimen/Bebas+Neue) — bold, athletic, zero-fuss sports feel
- **Body**: [DM Sans](https://fonts.google.com/specimen/DM+Sans) — clean, modern, readable

### Navbar Layout

```
[🏟 STADIUM TRACKER]  [Map]  [My Visits]  ···  [Admin ▾]  [☀/🌙]  [👤 Account]
```

- Brand on far left using Bebas Neue
- Nav links immediately after
- Admin dropdown (AuthorizeView Roles="Admin") with Leagues / Stadiums / Users inside
- Dark/light toggle button (sun/moon Bootstrap icon, calls JS to flip `data-bs-theme` and write to localStorage)
- User/account icon on far right

### Color Palette

**Light mode:**
| Variable | Value | Usage |
|---|---|---|
| `--color-primary` | `#B22234` | Buttons, active accents, highlights |
| `--color-secondary` | `#002868` | Navbar background, headings |
| `--color-bg` | `#ffffff` | Page background |
| `--color-surface` | `#f4f6f9` | Cards, panels |
| `--color-text` | `#1a1a2e` | Body text |
| `--color-nav-bg` | `#002868` | Navbar background (navy) |
| `--color-nav-text` | `#ffffff` | Navbar link text |
| `--color-nav-accent` | `#B22234` | Active link indicator / underline |

**Dark mode** (toggled via `data-bs-theme="dark"` on `<html>`):
| Variable | Value | Usage |
|---|---|---|
| `--color-primary` | `#e84055` | Brighter red for dark backgrounds |
| `--color-secondary` | `#4a90d9` | Lighter blue for dark backgrounds |
| `--color-bg` | `#0d1117` | Page background |
| `--color-surface` | `#161b22` | Cards, panels |
| `--color-text` | `#e6edf3` | Body text |
| `--color-nav-bg` | `#0d1117` | Navbar background (near-black) |
| `--color-nav-text` | `#e6edf3` | Navbar link text |
| `--color-nav-accent` | `#e84055` | Active link indicator |

### Theme Toggle Implementation
1. **Inline script in `App.razor`** — runs before the body renders; reads `localStorage.getItem('theme')` and sets `document.documentElement.setAttribute('data-bs-theme', theme)`. Prevents flash of wrong theme on page load.
2. **Toggle button in NavMenu.razor** — calls `window.toggleTheme()`, a small JS function that flips the attribute and writes the new value to localStorage.

### Mobile Responsive
Bootstrap's `navbar-toggler` / `navbar-collapse` handles mobile collapse. Admin dropdown degrades gracefully on mobile (expands inline). Breakpoint: Bootstrap default (992px).

---

## What Is NOT Changing
- All page content components (`Home.razor`, `StadiumList.razor`, `Admin/*.razor`) — untouched
- Leaflet map integration — untouched
- Auth/Identity pages — untouched
- No new routes or services

---

## Verification

1. `dotnet run --project StadiumTracker/StadiumTracker.csproj` — confirm app starts, no build errors
2. Visit `/` — map renders, top navbar visible (no sidebar)
3. Toggle dark/light mode — theme switches instantly, persists on page refresh
4. Log in as admin — "Admin" dropdown appears in navbar with all 3 links working
5. Log in as regular user — "Admin" dropdown not visible
6. Mobile: resize to narrow viewport — hamburger menu appears, all links accessible
7. `dotnet test` — all existing tests still pass
