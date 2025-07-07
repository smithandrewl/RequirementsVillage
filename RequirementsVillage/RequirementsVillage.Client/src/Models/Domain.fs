module RequirementsVillage.Client.Models.Domain

open System

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