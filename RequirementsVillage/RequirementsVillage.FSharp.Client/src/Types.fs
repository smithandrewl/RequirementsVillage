module RequirementsVillage.FSharp.Client.Types

open System

type ProjectStatus =
    | Someday
    | Current
    | Archive

type Project = {
    Id: Guid
    Title: string
    Description: string
    TechStack: string list
    Status: ProjectStatus
}

type Theme =
    | Light
    | Dark

type Page =
    | Landing
    | Dashboard

type Model = {
    CurrentPage: Page
    CurrentTheme: Theme
    Projects: Project list
    FilteredStatus: ProjectStatus option
    IsLoading: bool
    Error: string option
}

type ApiError =
    | NetworkError of string
    | DecodingError of string
    | ServerError of int * string

type Msg =
    | NavigateTo of Page
    | SetTheme of Theme
    | LoadProjects
    | ProjectsLoaded of Result<Project list, ApiError>
    | FilterByStatus of ProjectStatus option
    | ClearError