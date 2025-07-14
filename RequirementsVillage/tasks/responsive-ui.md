# Responsive UI Tasks

## Overview
Enhance the application's responsive design using Bulma's CSS classes and utilities. All responsive behavior should be handled through CSS classes, not JavaScript/F# code.

## Current State
- ✅ Viewport meta tag configured
- ✅ Bulma CSS framework with responsive grid
- ✅ Project cards use responsive columns (3 on desktop, 2 on tablet, 1 on mobile)
- ❌ No mobile navigation using Bulma navbar component
- ❌ No responsive utility classes for hiding/showing elements
- ❌ Fixed font sizes that may be too large on mobile (6rem hero text)

## Tasks

### 1. Implement Bulma navbar for responsive navigation
- [ ] Replace current header with Bulma navbar component
- [ ] Add navbar-burger for mobile menu (pure CSS toggle)
- [ ] Structure navbar with proper Bulma classes:
  ```fsharp
  Bulma.navbar [
    Bulma.navbarBrand.div [
      Bulma.navbarItem.div [ (* logo/title *) ]
      Bulma.navbarBurger [ (* burger icon *) ]
    ]
    Bulma.navbarMenu [
      Bulma.navbarEnd.div [
        Bulma.navbarItem.div [ (* theme selector *) ]
      ]
    ]
  ]
  ```
- [ ] Use Bulma's built-in navbar responsive behavior
- [ ] Ensure theme selector works in mobile menu

### 2. Apply responsive typography classes
- [ ] Use Bulma's responsive text size modifiers:
  - `is-size-1-desktop is-size-3-mobile` for hero text
  - `is-size-4-desktop is-size-5-mobile` for headings
- [ ] Apply responsive spacing helpers:
  - `py-6 py-3-mobile` for vertical padding
  - `px-6 px-3-mobile` for horizontal padding
- [ ] Use `has-text-centered-mobile` where appropriate

### 3. Enhance responsive grid usage
- [ ] Apply more specific Bulma column classes:
  ```fsharp
  Bulma.column [
    column.is4Desktop
    column.is6Tablet  
    column.is12Mobile
  ]
  ```
- [ ] Use `is-hidden-mobile` and `is-hidden-desktop` for selective display
- [ ] Apply `is-flex-direction-column-mobile` for mobile layouts
- [ ] Use `is-fullwidth` on buttons for mobile

### 4. Optimize component layouts with utility classes
- [ ] Apply Bulma spacing utilities:
  - `mb-5 mb-3-mobile` for responsive margins
  - `p-5 p-3-mobile` for responsive padding
- [ ] Use level component for responsive layouts:
  ```fsharp
  Bulma.level [
    level.isMobile  // Maintains horizontal layout on mobile
  ]
  ```
- [ ] Apply `is-flex-wrap-wrap` for wrapping content

### 5. Improve card responsiveness
- [ ] Use Bulma's card footer for actions (auto-stacks on mobile)
- [ ] Apply responsive padding classes to card content
- [ ] Use `is-clipped` modifier to prevent overflow
- [ ] Ensure media objects in cards are responsive

### 6. Add responsive helpers for common patterns
- [ ] Stack horizontal layouts on mobile:
  ```fsharp
  Bulma.columns [
    columns.isMobile  // Prevents stacking
    columns.isMultiline  // Or allows wrapping
  ]
  ```
- [ ] Use Bulma's responsive table classes if needed
- [ ] Apply `is-fullwidth` to form elements on mobile

### 7. Create responsive empty states
- [ ] Use responsive image sizes:
  ```fsharp
  Bulma.image [
    image.is128x128
    prop.className "is-256x256-desktop"
  ]
  ```
- [ ] Apply responsive text alignment
- [ ] Use responsive spacing around illustrations

## Implementation Guidelines

### Bulma Responsive Modifiers
- `-mobile`: up to 768px
- `-tablet`: 769px to 1023px
- `-desktop`: 1024px and above
- `-widescreen`: 1216px and above

### Common Responsive Patterns
```fsharp
// Hide on mobile, show on desktop
prop.className "is-hidden-mobile"

// Different sizes per viewport
prop.className "is-size-1-desktop is-size-3-mobile"

// Responsive spacing
prop.className "p-6 p-3-mobile"

// Responsive columns
column.is4Desktop
column.is6Tablet
column.is12Mobile
```

### CSS-Only Approach
- No viewport width checks in F# code
- No conditional rendering based on screen size
- Use Bulma's CSS classes exclusively
- Let CSS handle all responsive behavior

## Expected Outcome
After implementing these tasks:
- Fully responsive layout using only CSS classes
- Bulma navbar with built-in mobile menu
- Proper text scaling across devices
- Elements that hide/show based on viewport
- No JavaScript-based responsive logic
- Clean, maintainable responsive design