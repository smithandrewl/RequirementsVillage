module RequirementsVillage.Client.Presentation.State.Types

open RequirementsVillage.Client.Domain.Project
open RequirementsVillage.Client.Infrastructure.Api.Types
open RequirementsVillage.Client.Infrastructure.Storage.ThemeStorage

// UI pages
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

type Msg =
  | NavigateTo     of Page
  | SetTheme       of Theme
  | ProjectsLoaded of Result<Project list, ApiError>
  | FilterByStatus of ProjectStatus option
  | ClearError
  | LoadProjects