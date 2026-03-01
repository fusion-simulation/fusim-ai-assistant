# Frontend Redesign: "The Modern Workspace" (Dark Theme)

## Context
The user requested a modern redesign of the frontend pages for their Blazor application `FusimAiAssiant`. The current application uses generic Bootstrap styling (`app.css`, `_Host.cshtml`), which will be replaced with a bespoke, striking aesthetic.

## Concept
The chosen design direction is "The Modern Workspace" tailored specifically as a **Corporate / Refined Dark Theme**. It emphasizes highly legible geometric typography, generous negative space, subtle frosted glass layers (glassmorphism), and a high-contrast dark color palette with a vibrant amber/orange accent.

## Aesthetic Details
- **Tone**: Premium SaaS product, elegant, trustworthy, and modern.
- **Typography**: `Plus Jakarta Sans` or `Inter` (geometric sans-serif) for clean, highly legible structure.
- **Color Palette**:
  - **Base Background**: Deep black-gray (`#121212` or `#18181B`).
  - **Surfaces**: Elevated gray (`#27272A`) for cards and panels.
  - **Primary Accent**: Striking orange-yellow (`#F5A623`).
  - **Text Primary**: Crisp off-white (`#F4F4F5`).
  - **Text Secondary**: Muted slate gray (`#A1A1AA`).
  - **Borders/Dividers**: Subtle white transparency (`rgba(255, 255, 255, 0.05)`).
- **Motion**: Soft hover states, gentle fades, and subtle glows on active elements.

## Architecture & Components
1. **CSS Architecture**: Bootstrap will be removed. A custom CSS variable structure will be built in `app.css` to manage theming efficiently.
2. **Layout (`MainLayout.razor` & `NavMenu.razor`)**:
   - **Sidebar**: Dark floating panel that detaches slightly from the screen edge. Active navigation items glow subtly with the orange-yellow accent.
   - **Header**: Sticky top bar with a dark frosted glass effect (`backdrop-filter: blur(12px)`).
3. **Core Pages**:
   - **Login**: A centralized floating card positioned over a subtle orange-yellow ambient gradient mesh background.
   - **Home**: A grand, welcoming header with oversized greeting typography and a clean dashboard layout.
   - **Submit (Forms)**: Custom input fields with soft orange-yellow focus rings. Harsh borders replaced with subtle background elevations.
   - **Cases (Data)**: Padded table/list view with row-hover highlights, removing heavy grid lines for an airy data presentation.

## Success Criteria
- The application looks distinctive, bespoke, and extremely high-quality.
- Generic "Bootstrap" aesthetics are fully eradicated.
- The UI is highly functional but feels like a luxury enterprise tool.
