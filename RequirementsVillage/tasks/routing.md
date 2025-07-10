# Routing Tasks

## Overview
Implement client-side routing with parameters to demonstrate navigation patterns in a Fable SPA.

## Tasks

### 3. Implement routing infrastructure with parameters
- [ ] Add Feliz.Router package to the project
- [ ] Define Route discriminated union type
  - Landing
  - Projects
  - ProjectDetail of Guid
- [ ] Create URL parsing functions
- [ ] Create URL generation functions
- [ ] Add current route to the application model
- [ ] Handle route change messages

### 4. Add project detail page with route parameters
- [ ] Create ProjectDetail.fs page component
- [ ] Parse project ID from route parameters
- [ ] Fetch and display project details
- [ ] Handle loading and error states
- [ ] Add breadcrumb navigation
- [ ] Implement "Back to Projects" link

### 5. Update navigation to use proper routing
- [ ] Replace onClick handlers with proper route navigation
- [ ] Update project cards to link to detail page
- [ ] Add active route highlighting in navigation
- [ ] Implement route guards (e.g., redirect to landing if needed)
- [ ] Handle browser back/forward buttons
- [ ] Update page titles based on current route

## Implementation Notes
- Use Feliz.Router for type-safe routing
- Consider SEO-friendly URLs
- Handle invalid project IDs gracefully
- Preserve scroll position on navigation