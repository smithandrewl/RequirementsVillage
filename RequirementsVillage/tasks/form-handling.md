# Form Handling Tasks

## Overview
Implement form handling with validation to demonstrate proper Elmish patterns for form state management without using API calls.

## Tasks

### 1. Create form component with local state validation
- [ ] Create a new "Create Project" form component
- [ ] Implement local state for form fields (name, description, category, status)
- [ ] Add form state to the Elmish model
- [ ] Handle input changes with proper message types
- [ ] Show form in a modal or dedicated page

### 2. Add form validation rules and error display
- [ ] Implement validation functions for each field
  - Name: required, min/max length
  - Description: required, min length
  - Category: required selection
- [ ] Add validation state to the model
- [ ] Display field-level error messages
- [ ] Implement form submission with validation check
- [ ] Show success/error feedback after submission
- [ ] Clear form on successful submission

## Implementation Notes
- Use discriminated unions for validation errors
- Keep validation pure and testable
- Consider real-time vs on-blur validation
- Use Bulma's form validation CSS classes