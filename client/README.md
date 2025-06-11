# Requirements Village - Client

## Overview

The frontend application for Requirements Village built with SvelteKit, Tailwind CSS, and DaisyUI.

## Tech Stack

- **Framework:** SvelteKit
- **Language:** TypeScript
- **Styling:** Tailwind CSS + DaisyUI
- **Testing:** Vitest (unit) + Playwright (integration)
- **Code Quality:** ESLint only (NO prettier)

## Development

### Prerequisites

- Node.js 18+
- npm

### Setup

```bash
npm install
```

### Available Scripts

```bash
# Development server
npm run dev

# Build for production
npm run build

# Preview production build
npm run preview

# Run all tests
npm test

# Run unit tests
npm run test:unit

# Run integration tests  
npm run test:integration

# Type checking
npm run check

# Linting
npm run lint
```

### Project Structure

```
src/
├── app.html           # HTML template
├── app.css           # Global styles
├── app.d.ts          # TypeScript declarations
├── lib/              # Reusable components and utilities
│   └── components/   # Svelte components
└── routes/           # File-based routing
    ├── +layout.svelte # Root layout
    └── +page.svelte   # Home page
```

## Features

- ✅ TypeScript everywhere
- ✅ Tailwind CSS + DaisyUI components
- ✅ Unit testing with Vitest
- ✅ Integration testing with Playwright
- ✅ ESLint configured (NO prettier - prettier is banned)
- ✅ Responsive design ready