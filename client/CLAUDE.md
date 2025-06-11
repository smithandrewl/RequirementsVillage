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

## 🎨 Code Formatting Guidelines

### General Formatting
- **Indentation:** 2 spaces for all web files
- **Quotes:** Single quotes for TypeScript/JavaScript strings
- **Semicolons:** Always required for TS/JS statements
- **Line Length:** 80 characters maximum (hard limit)
- **Exception:** Long URLs and string attributes are exempt from 80-column limit and should not be wrapped

### Import Organization
- Group imports by type with blank lines between groups:
  ```typescript
  // Third-party libraries (separate group per library)
  import { writable } from 'svelte/store';
  
  import type { ComponentProps } from 'svelte';
  
  // Local imports
  import MyComponent from '$lib/components/MyComponent.svelte';
  ```

### Svelte Components & HTML
- **File Naming:** Follow idiomatic conventions (PascalCase recommended)
- **HTML Attributes:** If more than 2 attributes, place each on its own line with aligned equals signs:
  ```svelte
  <!-- 2 or fewer attributes - single line -->
  <button class="btn" type="submit">Click</button>
  
  <!-- 3+ attributes - multi-line with aligned formatting -->
  <button
    class     = "btn btn-primary"
    type      = "submit"
    disabled  = { loading     }
    on:click  = { handleClick }
  >
    Submit
  </button>
  ```
- **Attribute Alignment Rules:**
  - Each tag's attributes are aligned independently
  - Alignment is based on the longest attribute name in that tag
  - Lone attributes (like `crossorigin`) don't affect the `=` alignment of other attributes
  - Closing braces `}` should align consistently within the same attribute block
- **Section Order:** Follow idiomatic Svelte conventions for `<script>`, `<style>`, and HTML placement

### Status
- **✅ Applied:** All formatting guidelines have been applied to the entire codebase
- **📁 Scope:** All `.svelte`, `.ts`, and `.html` files in the client folder