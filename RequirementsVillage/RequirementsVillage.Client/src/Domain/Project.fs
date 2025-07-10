module RequirementsVillage.Client.Domain.Project

// Import all shared domain types
open RequirementsVillage.Shared

// Re-export for backward compatibility
type Project         = RequirementsVillage.Shared.Project
type ProjectStatus   = RequirementsVillage.Shared.ProjectStatus
type ProjectCategory = RequirementsVillage.Shared.ProjectCategory

// Re-export utility functions
let duCaseToDisplayText = Utils.duCaseToDisplayText

// Re-export display text functions
module ProjectStatus =
  let toDisplayText = ProjectStatus.toDisplayText

module ProjectCategory =
  let toDisplayText = ProjectCategory.toDisplayText