# CLAUDE.md - Client

## 🎯 Project Context

This is the frontend client for Requirements Village, built with SvelteKit, Tailwind CSS, and DaisyUI. The application provides a modern, responsive interface for managing project ideas.

## 🧰 Tech Stack

- **Framework:** SvelteKit
- **Language:** TypeScript (no JavaScript files)
- **Styling:** Tailwind CSS + DaisyUI
- **Testing:** Vitest (unit) + Playwright (integration)
- **Code Quality:** ESLint + Prettier

## 📁 Key Files & Structure

```
src/
├── app.html              # HTML template with DaisyUI theme support
├── app.css              # Global Tailwind imports
├── routes/
│   ├── +layout.svelte   # Root layout with CSS imports
│   └── +page.svelte     # Home page with hero section
└── lib/
    └── components/      # Reusable Svelte components
```

## 🛠️ Development Commands

```bash
npm run dev              # Development server
npm run build           # Production build
npm test               # Run all tests
npm run check          # TypeScript checking
npm run lint           # ESLint + Prettier
```

## 🎨 Styling Guidelines

- Use DaisyUI components for UI elements
- Leverage Tailwind utilities for custom styling
- Follow responsive-first design approach
- Maintain consistent spacing using Tailwind scale

## 🧪 Testing Strategy

- **Unit Tests:** Component logic with Vitest
- **Integration Tests:** Full user flows with Playwright
- Test files follow `*.test.ts` convention

## 📝 Code Standards

- TypeScript everywhere (no .js files)
- ESLint + Prettier for code formatting
- Component props should be typed
- Use `lang="ts"` in all script blocks