# Form Handling Tasks

## Overview
Implement comprehensive form handling with validation to enable creating and editing projects in the Requirements Village application. Currently, the app is read-only from the client side, despite having full CRUD API endpoints ready on the backend.

## Current State
- No forms implemented in the client
- Backend has full CRUD endpoints with validation
- API client only implements GET requests
- No form state management patterns established
- "Add Project" button exists but is non-functional

## Tasks

### 1. Define form state types and validation
- [ ] Create form state types:
  ```fsharp
  type FormField<'T> = {
    Value: 'T
    Error: string option
    Touched: bool
  }
  
  type ProjectForm = {
    Name:        FormField<string>
    Description: FormField<string> 
    Category:    FormField<ProjectCategory option>
    Status:      FormField<ProjectStatus>
    IsSubmitting: bool
  }
  ```
- [ ] Create validation error types:
  ```fsharp
  type ValidationError =
    | Required of fieldName: string
    | TooShort of fieldName: string * minLength: int
    | TooLong of fieldName: string * maxLength: int
  ```
- [ ] Port validation rules from backend to client:
  - Name: Required, max 100 characters
  - Description: Required, max 1000 characters
  - Category: Required selection

### 2. Add form messages and state to Model
- [ ] Create form-related messages:
  ```fsharp
  type FormMsg =
    | UpdateName of string
    | UpdateDescription of string
    | UpdateCategory of ProjectCategory option
    | UpdateStatus of ProjectStatus
    | ValidateField of fieldName: string
    | SubmitForm
    | FormSubmitted of Result<Project, ApiError>
    | ResetForm
    | CancelForm
  ```
- [ ] Add form state to the Model:
  - Option<ProjectForm> for create/edit mode
  - Track whether creating new or editing existing
  - Store original project for edit mode

### 3. Implement form update logic
- [ ] Handle field updates with validation
- [ ] Implement on-change and on-blur validation
- [ ] Create form submission logic:
  - Validate all fields
  - Show field errors if invalid
  - Submit if valid
  - Handle loading state during submission
- [ ] Reset form after successful submission
- [ ] Handle API errors appropriately

### 4. Create form UI components
- [ ] Create reusable form field component:
  - Input with label
  - Error message display
  - Bulma styling (help text, danger state)
- [ ] Create ProjectForm component:
  - Name input (text)
  - Description textarea
  - Category dropdown
  - Status radio buttons or select
  - Submit/Cancel buttons
  - Loading spinner during submission
- [ ] Implement proper keyboard support (Enter to submit, Escape to cancel)

### 5. Integrate create functionality
- [ ] Make "Add Project" button functional
- [ ] Show form in modal or slide-out panel
- [ ] Implement POST API call:
  ```fsharp
  let createProject (project: CreateProjectRequest) =
    promise {
      // POST to /api/projects
    }
  ```
- [ ] Update project list after successful creation
- [ ] Show success notification
- [ ] Handle and display API errors

### 6. Add edit functionality
- [ ] Add edit button to project cards
- [ ] Pre-populate form with existing data
- [ ] Implement PUT API call:
  ```fsharp
  let updateProject (id: Guid) (project: UpdateProjectRequest) =
    promise {
      // PUT to /api/projects/{id}
    }
  ```
- [ ] Update project in list after successful edit
- [ ] Handle optimistic updates (optional)

### 7. Add quick status update
- [ ] Create inline status editor
- [ ] Implement PATCH API call:
  ```fsharp
  let updateProjectStatus (id: Guid) (status: ProjectStatus) =
    promise {
      // PATCH to /api/projects/{id}/status
    }
  ```
- [ ] Update UI immediately (optimistic)
- [ ] Rollback on error

### 8. Form UX enhancements
- [ ] Add unsaved changes warning
- [ ] Implement form dirty checking
- [ ] Add confirmation for cancel with unsaved changes
- [ ] Focus first field when form opens
- [ ] Show character count for limited fields
- [ ] Add client-side debouncing for validation

## Implementation Guidelines

### Validation Strategy
- Share validation logic between client and server where possible
- Validate on blur for better UX
- Show errors only after field is touched
- Validate all fields on submit attempt

### State Management
- Keep form state separate from domain state
- Use Option types for edit vs create mode
- Track form dirty state for navigation guards

### API Integration
- Complete the API client module with all HTTP verbs
- Handle all possible API errors gracefully
- Consider optimistic updates for better perceived performance

### Testing Approach
- Unit test validation functions
- Test form state transitions
- Test API error handling
- Consider property-based testing for form inputs

## Expected Outcome
After implementing these tasks, users will be able to:
- Create new projects with validated input
- Edit existing projects
- Quickly update project status
- See helpful validation messages
- Have a smooth form experience with proper loading states

The application will transition from read-only to a full CRUD interface with robust form handling.