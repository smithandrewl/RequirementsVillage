# Client-Side Routing Tasks

## Overview
Implement proper client-side routing with browser history support to enable back button functionality, bookmarkable URLs, and deep linking in the Requirements Village SPA. This is entirely client-side routing - the server already serves the SPA correctly.

## Current State
The application currently uses simple state-based navigation without URL changes or browser history integration. This means:
- Browser back/forward buttons don't work
- URLs don't change when navigating between pages
- Client-side pages aren't bookmarkable
- No deep linking support

## Tasks

### 1. Choose and integrate a client-side routing solution
- [ ] Evaluate client-side routing options for Fable/Elmish:
  - **Elmish.Navigation** - Integrates with Elmish, handles browser history
  - **Elmish.UrlParser** - Type-safe URL parsing with Elmish
  - **Feliz.Router** - Simple, React-based routing
- [ ] Add chosen package to project dependencies
- [ ] Import and configure the routing module
- [ ] Set up browser history mode (not hash mode for cleaner URLs)

### 2. Define the client-side route model and parser
- [ ] Create Route discriminated union:
  ```fsharp
  type Route =
    | Landing
    | Dashboard
    | ProjectDetail of projectId: Guid
    | NotFound
  ```
- [ ] Implement URL parser using chosen library:
  ```fsharp
  let routeParser =
    oneOf [
      map Landing (s "")
      map Dashboard (s "dashboard")
      map ProjectDetail (s "project" </> guid)
    ]
  ```
- [ ] Create URL generation functions for type-safe links
- [ ] Handle parsing failures with NotFound route

### 3. Integrate routing with Elmish program
- [ ] Add Route to the application Model
- [ ] Create UrlChanged message type
- [ ] Set up subscription to browser URL changes (popstate event)
- [ ] Initialize app with current browser URL route
- [ ] Update init to handle initial route from browser location
- [ ] Ensure route changes update both browser URL and application state

### 4. Update client navigation throughout the app
- [ ] Replace `NavigateTo` message with proper pushState navigation
- [ ] Update Landing page "Enter Graveyard" button to push new URL
- [ ] Add navigation back to Landing from Dashboard
- [ ] Ensure all navigation updates the browser URL
- [ ] Test browser back/forward buttons work correctly
- [ ] Verify bookmarking and page refresh maintain correct page

### 5. Add project detail page with client routing
- [ ] Create ProjectDetail page component
- [ ] Add client route pattern `/project/{id}`
- [ ] Update project cards to navigate to detail pages
- [ ] Implement loading project by ID from route parameter
- [ ] Handle invalid project IDs gracefully (show 404 in client)
- [ ] Add breadcrumb navigation on detail page

### 6. Handle client-side edge cases
- [ ] Test direct URL access to all routes (server returns SPA, client routes)
- [ ] Handle 404/NotFound routes gracefully in the client
- [ ] Ensure proper behavior on page refresh
- [ ] Test navigation with browser navigation buttons
- [ ] Prevent navigation loops or invalid states
- [ ] Add client-side navigation analytics hooks (optional)

## Implementation Guidelines

### Client-Side URL Structure
```
/                    → Landing page
/dashboard           → Projects dashboard  
/project/{guid}      → Project detail page
/*                   → 404 Not Found (handled client-side)
```

### Code Organization
- Keep routing logic in client code only
- Use type-safe route generation functions
- Centralize route definitions in a single client module
- Make navigation functions pure and testable

### Browser APIs to Use
- `window.history.pushState()` - For navigation without page reload
- `window.history.replaceState()` - For replacing current history entry
- `window.addEventListener('popstate', ...)` - For back/forward button handling
- `window.location` - For reading current URL

### Testing Approach
- Unit test route parsing and generation
- Test navigation doesn't break application state
- Verify browser history manipulation works correctly
- Test deep linking scenarios in the client

## Expected Outcome
After implementing these tasks, the client-side application will have:
- Working browser back/forward buttons
- Bookmarkable URLs for each page (client handles routing after load)
- Direct URL access to any page (server serves SPA, client routes)
- Type-safe routing with compile-time guarantees
- Standard web navigation behavior all handled client-side

## Note
This is purely client-side routing. The server configuration (already serving the SPA correctly) doesn't need to change.