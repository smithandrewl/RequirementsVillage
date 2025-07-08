module RequirementsVillage.Client.Models.Domain

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

type ApiError =
  | NetworkError  of string
  | DecodingError of string
  | ServerError   of int * string
