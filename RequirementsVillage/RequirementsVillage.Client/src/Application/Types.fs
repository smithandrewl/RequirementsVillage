module RequirementsVillage.Client.Application.Types

open RequirementsVillage.Client.Domain.Project
open RequirementsVillage.Client.Infrastructure.Api.Types
open RequirementsVillage.Client.Infrastructure.Storage.ThemeStorage

// Application pages
type Page =
  | Landing
  | Dashboard

// Application commands
type Command =
  | LoadProjects
  | SaveProject    of Project
  | UpdateProject  of Project
  | DeleteProject  of System.Guid
  | SaveTheme      of Theme
  | LoadTheme

// Application queries
type Query =
  | GetAllProjects
  | GetProjectById     of System.Guid
  | GetProjectsByStatus of ProjectStatus
  | GetCurrentTheme

// Application events
type Event =
  | ProjectsLoaded       of Result<Project list, ApiError>
  | ProjectSaved         of Result<Project, ApiError>
  | ProjectUpdated       of Result<Project, ApiError>
  | ProjectDeleted       of Result<System.Guid, ApiError>
  | ThemeChanged         of Theme
  | NavigationRequested  of Page
  | ErrorOccurred        of string