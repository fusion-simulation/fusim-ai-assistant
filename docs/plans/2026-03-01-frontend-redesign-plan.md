# Frontend Redesign Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Redesign the FusimAiAssiant Blazor frontend to a highly polished, modern "Corporate/Refined Dark Theme" with orange-yellow accents, removing Bootstrap entirely.

**Architecture:** We will replace the default Bootstrap styling with a custom CSS Variable architecture in `app.css`. Components will use scoped CSS (`.razor.css`) or global utility classes defined in `app.css`. The layout will feature a floating sidebar and frosted glass headers. 

**Tech Stack:** Blazor Server (C#, HTML, pure CSS), Google Fonts (Plus Jakarta Sans).

---

### Task 1: Setup Foundation & Remove Bootstrap

**Files:**
- Modify: `Pages/_Host.cshtml`
- Modify: `wwwroot/app.css`
- Delete: `wwwroot/bootstrap/`

**Step 1: Update `_Host.cshtml` to remove Bootstrap and add fonts**
Modify `Pages/_Host.cshtml` to remove `<link rel="stylesheet" href="css/bootstrap/bootstrap.min.css" />` (or similar) and inject the Google Fonts link for `Plus Jakarta Sans`.

**Step 2: Define CSS Variables in `app.css`**
Clear the existing `wwwroot/app.css` and define the `:root` variables for the new dark theme:
```css
:root {
  --bg-base: #18181b; /* zinc-900 */
  --bg-surface: #27272a; /* zinc-800 */
  --bg-surface-hover: #3f3f46; /* zinc-700 */
  --accent-primary: #f5a623; /* striking orange-yellow */
  --accent-primary-hover: #eab308;
  --text-primary: #f4f4f5; /* zinc-100 */
  --text-secondary: #a1a1aa; /* zinc-400 */
  --border-subtle: rgba(255, 255, 255, 0.05);
  --border-focus: rgba(245, 166, 35, 0.5);
  --shadow-elevated: 0 4px 6px -1px rgba(0, 0, 0, 0.5), 0 2px 4px -2px rgba(0, 0, 0, 0.5);
  --font-sans: 'Plus Jakarta Sans', system-ui, sans-serif;
}

body {
  background-color: var(--bg-base);
  color: var(--text-primary);
  font-family: var(--font-sans);
  margin: 0;
  -webkit-font-smoothing: antialiased;
}

/* Global typography resets */
h1, h2, h3, h4, h5, h6 { color: var(--text-primary); margin-top: 0; font-weight: 600; }
p { color: var(--text-secondary); line-height: 1.6; }
a { color: var(--accent-primary); text-decoration: none; transition: color 0.2s; }
a:hover { color: var(--accent-primary-hover); }
```

**Step 3: Clean up Bootstrap**
Run `rm -rf wwwroot/bootstrap` to ensure no lingering styles.

**Step 4: Commit Foundation**
```bash
git add Pages/_Host.cshtml wwwroot/app.css wwwroot/bootstrap
git commit -m "chore(style): remove bootstrap, setup dark theme variables and fonts"
```

---

### Task 2: Redesign Layout (`MainLayout` & `NavMenu`)

**Files:**
- Modify: `Layout/MainLayout.razor`
- Modify: `Layout/MainLayout.razor.css`
- Modify: `Layout/NavMenu.razor`
- Modify: `Layout/NavMenu.razor.css`

**Step 1: Rewrite `MainLayout.razor` structure**
```html
<div class="page-container">
    <div class="sidebar-wrapper">
        <NavMenu />
    </div>
    <main class="main-content">
        <div class="top-row">
            <span class="brand-text">Fusim AI Assistant</span>
            <a href="https://docs.microsoft.com/aspnet/" target="_blank">About</a>
        </div>
        <article class="content-body">
            @Body
        </article>
    </main>
</div>
```

**Step 2: Style `MainLayout.razor.css`**
Implement the CSS for `.page-container` (flexbox), `.sidebar-wrapper` (fixed/floating width), `.main-content` (flex-grow), `.top-row` (glassmorphism: `background: rgba(24, 24, 27, 0.8); backdrop-filter: blur(12px); border-bottom: 1px solid var(--border-subtle);`), and `.content-body` (padding).

**Step 3: Rewrite `NavMenu.razor` & `NavMenu.razor.css`**
Update the navigation to feature a sleek dark panel. Remove Bootstrap `.navbar`, `.nav-item` classes. Use standard `<ul>` and `<li>` with custom scoped CSS.
The `.nav-link.active` state should have `background-color: var(--bg-surface); border-left: 3px solid var(--accent-primary); color: var(--accent-primary);`.

**Step 4: Commit Layout**
```bash
git add Layout/
git commit -m "feat(ui): implement modern workspace dark layout and navigation"
```

---

### Task 3: Redesign Login Page (`Login.razor`)

**Files:**
- Modify: `Pages/Login.razor`
- Create/Modify: `Pages/Login.razor.css` (if exists, else scoped block in `app.css` or create file)

**Step 1: Rewrite Login HTML**
Wrap the login form in a `.login-container` and a `.login-card`. Add a decorative `.ambient-glow` div behind the card.

**Step 2: Style Login**
Create `Pages/Login.razor.css`.
`.login-container`: Flex center, min-height 100vh.
`.login-card`: `background: var(--bg-surface); padding: 3rem; border-radius: 16px; border: 1px solid var(--border-subtle); box-shadow: var(--shadow-elevated); z-index: 10;`.
`.ambient-glow`: Absolute positioning, large blur `filter: blur(100px); background: radial-gradient(circle, var(--accent-primary) 0%, transparent 70%); opacity: 0.15;`.
Style inputs to have `background: var(--bg-base); border: 1px solid var(--border-subtle); color: var(--text-primary);` and on focus: `outline: none; border-color: var(--accent-primary); box-shadow: 0 0 0 3px var(--border-focus);`.

**Step 3: Commit Login Page**
```bash
git add Pages/Login.razor Pages/Login.razor.css
git commit -m "feat(ui): redesign login page with floating card and ambient glow"
```

---

### Task 4: Redesign Home & Cases Data Pages

**Files:**
- Modify: `Pages/Home.razor`
- Modify: `Pages/Cases.razor`
- Create/Modify: relevant scoped `.css` files.

**Step 1: Rewrite Home Page**
Update `Home.razor` to feature a large, welcoming hero section.
```html
<div class="hero-section">
    <h1 class="hero-title">Welcome to <span class="text-accent">Fusim AI</span></h1>
    <p class="hero-subtitle">Your intelligent corporate assistant.</p>
</div>
```
Style it in `Pages/Home.razor.css` with large typography and `.text-accent { color: var(--accent-primary); }`.

**Step 2: Rewrite Cases Page**
Update `Cases.razor` table to be a modern list. Remove standard `<table class="table">`.
Use a custom `div` grid or a deeply stylized table.
```css
.cases-table { width: 100%; border-collapse: collapse; }
.cases-table th { text-align: left; padding: 1rem; color: var(--text-secondary); font-weight: 500; border-bottom: 1px solid var(--border-subtle); }
.cases-table td { padding: 1rem; border-bottom: 1px solid var(--border-subtle); }
.cases-table tbody tr:hover { background-color: var(--bg-surface); cursor: pointer; transition: background-color 0.2s; }
```

**Step 3: Commit Home & Cases**
```bash
git add Pages/Home* Pages/Cases*
git commit -m "feat(ui): modernize home and cases pages with high-contrast data styling"
```

---

### Task 5: Redesign Submit (Forms)

**Files:**
- Modify: `Pages/Submit.razor`
- Create/Modify: `Pages/Submit.razor.css`

**Step 1: Rewrite Submit Form**
Update `Submit.razor` to use the new input styling (from Login). Wrap inputs in `.form-group` with generous margin-bottom (`1.5rem`).
Ensure the "Submit" button uses primary accent styling: `background: var(--accent-primary); color: #000; font-weight: 600; border: none; padding: 0.75rem 1.5rem; border-radius: 8px; cursor: pointer; transition: transform 0.1s, filter 0.2s;`. Hover: `filter: brightness(1.1);`. Active: `transform: scale(0.98);`.

**Step 2: Commit Submit Page**
```bash
git add Pages/Submit*
git commit -m "feat(ui): redesign submit form with premium input interactions"
```
