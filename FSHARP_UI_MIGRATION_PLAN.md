# F# UI Migration Plan: SvelteKit to Fable/Elmish/Feliz

## Overview
This document outlines the complete migration plan for converting the Requirements Village frontend from SvelteKit/TypeScript to a full F# stack using Fable, Elmish, and Feliz.

## Phase 1: Setup F# Frontend Project
1. Create new F# project structure under `RequirementsVillage.FSharp.Client/`
2. Initialize Fable project with Feliz template
3. Configure webpack/vite for F# to JS compilation
4. Set up Elmish for state management
5. Install fp-ts F# equivalents (FSharpPlus or native F# types)

## Phase 2: Component Migration

### Landing Page (`+page.svelte` → `App.fs`)
- Convert hero section with gothic background
- Implement theme switching with Elmish state
- Replace Svelte lifecycle with React hooks via Feliz

### Dashboard (`dashboard/+page.svelte` → `Dashboard.fs`)
- Convert project filtering logic to Elmish model
- Replace Svelte reactive statements with F# computed values
- Implement project grid layout with Feliz.Bulma

### Components
- **ProjectCard** → `ProjectCard.fs` with discriminated union for status
- **DashboardLayout** → `Layout.fs` with header/content composition
- **ThemeSelector** → `ThemeSelector.fs` with localStorage effect

## Phase 3: Styling Migration
- Replace Tailwind/DaisyUI with Feliz.Bulma components
- Create F# DSL for consistent styling
- Implement custom Gothic theme using Bulma variables
- Port responsive design patterns

## Phase 4: State Management
- Define domain models as F# records
- Implement Elmish program with:
  - `Model`: Application state
  - `Msg`: Discriminated unions for all events
  - `Update`: Pure state transitions
  - `View`: Feliz components

## Phase 5: API Integration
- Replace fetch calls with F# async workflows
- Implement Railway-Oriented Programming for error handling
- Create type providers or DTOs for API contracts
- Handle all errors with discriminated unions

## Phase 6: Build Integration
- Configure Fable to output to wwwroot
- Update .NET project to serve F# compiled output
- Integrate with existing API project
- Set up hot reload for development

## Phase 7: Testing
- Port existing tests to F# with Expecto
- Implement property-based testing with FsCheck
- Set up browser testing with Canopy

## File Structure:
```
RequirementsVillage.FSharp.Client/
├── src/
│   ├── App.fs              # Main entry point
│   ├── Types.fs            # Domain models
│   ├── State.fs            # Elmish model/update
│   ├── Pages/
│   │   ├── Landing.fs      # Landing page
│   │   └── Dashboard.fs    # Dashboard page
│   ├── Components/
│   │   ├── ProjectCard.fs
│   │   ├── Layout.fs
│   │   └── ThemeSelector.fs
│   └── Api/
│       └── Projects.fs     # API client
├── public/                 # Static assets
├── webpack.config.js       # Fable build config
├── package.json
└── paket.dependencies      # F# dependencies
```

## Key Technologies
- **Fable**: F# to JavaScript compiler
- **Elmish**: Elm-like state management for F#
- **Feliz**: Type-safe React DSL for F#
- **Feliz.Bulma**: Bulma CSS framework bindings
- **FSharpPlus**: Functional programming abstractions

## Migration Benefits
- Type safety throughout the entire stack
- Functional programming paradigm consistency
- Better refactoring support with F# compiler
- Reduced runtime errors through discriminated unions
- Unified language across frontend and backend