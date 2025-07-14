module RequirementsVillage.Client.Presentation.State.Types

open RequirementsVillage.Shared
open RequirementsVillage.Client.Infrastructure.Api.Types
open RequirementsVillage.Client.Infrastructure.Storage.ThemeStorage
open RequirementsVillage.Client.Routes

// For backward compatibility, alias Page to Route
type Page = Route

// Loading operations for granular tracking
type LoadingOperation =
  | LoadingProjects
  | CreatingProject
  | UpdatingProject   of projectId: System.Guid
  | DeletingProject   of projectId: System.Guid
  | UpdatingStatus    of projectId: System.Guid
  | LoadingProjectDetail of projectId: System.Guid

// Domain state - business data
type DomainState = {
  Projects: Project list
}

// UI state - presentation concerns
type UIState = {
  CurrentPage:       Page
  CurrentTheme:      Theme
  FilteredStatus:    ProjectStatus option
  LoadingOperations: Set<LoadingOperation>
  Error:             string option
}

// Combined model
type Model = {
  Domain: DomainState
  UI:     UIState
}

type Msg =
  | NavigateTo     of Page
  | UrlChanged     of Route  // New message for browser URL changes
  | SetTheme       of Theme
  | ProjectsLoaded of Result<Project list, ApiError>
  | FilterByStatus of ProjectStatus option
  | ClearError
  | LoadProjects
  | StartLoading   of LoadingOperation
  | StopLoading    of LoadingOperation

// UI State helper functions
module UIState =
  let isLoading (operation: LoadingOperation) (uiState: UIState) =
    uiState.LoadingOperations |> Set.contains operation
  
  let isLoadingAny (uiState: UIState) =
    not (Set.isEmpty uiState.LoadingOperations)
  
  let isLoadingProject (projectId: System.Guid) (uiState: UIState) =
    uiState.LoadingOperations 
    |> Set.exists (function
      | UpdatingProject id 
      | DeletingProject id 
      | UpdatingStatus id
      | LoadingProjectDetail id -> id = projectId
      | _ -> false
    )
  
  let isCreatingProject (uiState: UIState) =
    isLoading CreatingProject uiState
  
  let isLoadingProjects (uiState: UIState) =
    isLoading LoadingProjects uiState