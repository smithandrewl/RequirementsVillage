# State Management Modularization Tasks

## Overview
Split the monolithic state management into feature-based modules for better organization and maintainability.

## Tasks

### 9. Split state management into feature modules
- [ ] Create State folder structure:
  - State/Landing/Types.fs & Update.fs
  - State/Projects/Types.fs & Update.fs
  - State/ProjectDetail/Types.fs & Update.fs
  - State/Forms/Types.fs & Update.fs
  - State/App/Types.fs & Update.fs (root composition)
- [ ] Define page-specific models
- [ ] Define page-specific message types
- [ ] Move existing state logic to appropriate modules

### 10. Create separate update modules per page/feature
- [ ] Implement update function for Landing module
- [ ] Implement update function for Projects module
- [ ] Implement update function for ProjectDetail module
- [ ] Implement update function for Forms module
- [ ] Create init functions for each module
- [ ] Handle cross-module communication patterns

### 11. Implement state composition pattern
- [ ] Create root App module that composes sub-modules
- [ ] Implement message routing from App to sub-modules
- [ ] Create helper functions for state access
- [ ] Implement state lenses or similar patterns
- [ ] Handle shared state (e.g., user, theme)
- [ ] Create subscription composition
- [ ] Document the composition pattern

## Implementation Notes
- Each module should be independent and testable
- Use discriminated unions for inter-module messages
- Consider using Elmish.Bridge pattern for complex scenarios
- Keep shared state minimal
- Make state transitions explicit and traceable