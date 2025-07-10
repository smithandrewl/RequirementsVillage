namespace RequirementsVillage.Shared

open System
open System.Text.RegularExpressions

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

// Shared error types that both client and API use
type ProjectError =
  | NotFound of
        projectId:     Guid
      * searchContext: string
  | ValidationFailed of
        field:          string
      * reason:         string
      * attemptedValue: obj
  | DatabaseError of
        operation:  string
      * tableName:  string
      * innerError: exn
  | UnknownError of message: string

// Utility functions
module Utils =
  let duCaseToDisplayText (caseValue: obj) =
    let caseStr = caseValue.ToString()
    Regex.Replace(caseStr, "([A-Z])", " $1").Trim()

// Display text utilities
module ProjectStatus =
  let toDisplayText status = Utils.duCaseToDisplayText status
  
  let toString = function
    | Idea       -> "idea"
    | InProgress -> "inProgress"
    | Completed  -> "completed"
    | Abandoned  -> "abandoned"
    | OnHold     -> "onHold"
  
  let fromString = function
    | "idea"       -> Ok Idea
    | "inProgress" -> Ok InProgress
    | "completed"  -> Ok Completed
    | "abandoned"  -> Ok Abandoned
    | "onHold"     -> Ok OnHold
    | s            -> Error (sprintf "Unknown ProjectStatus: %s" s)

module ProjectCategory =
  let toDisplayText = function
    | Other s  -> s
    | category -> Utils.duCaseToDisplayText category
    
  let toString = function
    | WebApp    -> "webApp"
    | MobileApp -> "mobileApp"
    | Library   -> "library"
    | Tool      -> "tool"
    | Game      -> "game"
    | Other s   -> s
    
  let fromString = function
    | "webApp"    -> WebApp
    | "mobileApp" -> MobileApp
    | "library"   -> Library
    | "tool"      -> Tool
    | "game"      -> Game
    | s           -> Other s