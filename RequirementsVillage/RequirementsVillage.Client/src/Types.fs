module RequirementsVillage.Client.Types

open System
open System.Text.RegularExpressions

let duCaseToDisplayText (caseValue: obj) =
  let caseStr = caseValue.ToString()
  Regex.Replace(caseStr, "([A-Z])", " $1").Trim()

type ProjectStatus =
  | Idea
  | InProgress
  | Completed
  | Abandoned
  | OnHold

type ProjectCategory =
  | WebApp
  | MobileApp
  | Library
  | Tool
  | Game
  | Other of string

module ProjectStatus =
  let toDisplayText status = duCaseToDisplayText status

module ProjectCategory =
  let toDisplayText = function
    | Other s -> s
    | category -> duCaseToDisplayText category

type Project = {
  Id:          Guid
  Name:        string
  Description: string
  Category:    ProjectCategory
  Status:      ProjectStatus
  CreatedAt:   DateTime
  UpdatedAt:   DateTime
}

type Theme =
  | Light
  | Dark

type Page =
  | Landing
  | Dashboard

type Model = {
  CurrentPage:    Page
  CurrentTheme:   Theme
  Projects:       Project list
  FilteredStatus: ProjectStatus option
  IsLoading:      bool
  Error:          string option
}

type ApiError =
  | NetworkError  of string
  | DecodingError of string
  | ServerError   of int * string

type Msg =
  | NavigateTo     of Page
  | SetTheme       of Theme
  | ProjectsLoaded of Result<Project list, ApiError>
  | FilterByStatus of ProjectStatus option
  | ClearError
  | LoadProjects
