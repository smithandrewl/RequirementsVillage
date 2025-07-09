module RequirementsVillage.Client.State.Types

open RequirementsVillage.Client.Models.Domain

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