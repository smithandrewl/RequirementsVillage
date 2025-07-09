module RequirementsVillage.Client.Domain.Project

open System
open System.Text.RegularExpressions

// Utility function for display text conversion
let duCaseToDisplayText (caseValue: obj) =
  let caseStr = caseValue.ToString()
  
  Regex.Replace(caseStr, "([A-Z])", " $1").Trim()

// Project status domain
type ProjectStatus =
  | Idea
  | InProgress
  | Completed
  | Abandoned
  | OnHold

// Project category domain
type ProjectCategory =
  | WebApp
  | MobileApp
  | Library
  | Tool
  | Game
  | Other of string

// Display text utilities
module ProjectStatus =
  let toDisplayText status = duCaseToDisplayText status

module ProjectCategory =
  let toDisplayText = function
    | Other s  -> s
    | category -> duCaseToDisplayText category

// Main project entity
type Project = {
  Id:          Guid
  Name:        string
  Description: string
  Category:    ProjectCategory
  Status:      ProjectStatus
  CreatedAt:   DateTime
  UpdatedAt:   DateTime
}
