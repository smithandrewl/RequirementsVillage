module RequirementsVillage.Client.Presentation.State.Update

open Elmish
open RequirementsVillage.Client.Domain.Project
open RequirementsVillage.Client.Presentation.State.Types
open RequirementsVillage.Client.Infrastructure.Api.Projects
open RequirementsVillage.Client.Infrastructure.Storage.ThemeStorage
open RequirementsVillage.Client.Infrastructure.Api.Types

let init () : Model * Cmd<Msg> =
  let savedTheme = Theme.load()
  
  // Apply the theme to DOM on startup
  Theme.applyToDom savedTheme

  let initialModel = {
    CurrentPage    = Landing
    CurrentTheme   = savedTheme
    Projects       = []
    FilteredStatus = None
    IsLoading      = false
    Error          = None
  }

  initialModel, Cmd.none

let update (msg: Msg) (model: Model) : Model * Cmd<Msg> =
  match msg with
  | NavigateTo page ->
    let cmd =
      match page with
      | Dashboard when List.isEmpty model.Projects ->
        Cmd.ofMsg LoadProjects
      | _ -> Cmd.none
    { model with CurrentPage = page }, cmd

  | SetTheme theme ->
    { model with CurrentTheme = theme }, Cmd.none

  | LoadProjects ->
    let loadCmd =
      Cmd.OfPromise.perform
        getProjects
        ()
        ProjectsLoaded
    { model with IsLoading = true }, loadCmd

  | ProjectsLoaded (Ok projects) ->
    { model with
        Projects = projects
        IsLoading = false
        Error = None
    }, Cmd.none

  | ProjectsLoaded (Error error) ->
    let errorMsg =
      match error with
      | NetworkError msg        -> $"Network error: {msg}"
      | DecodingError msg       -> $"Data error: {msg}"
      | ServerError (code, msg) -> $"Server error ({code}): {msg}"
    { model with
        IsLoading = false
        Error = Some errorMsg
    }, Cmd.none

  | FilterByStatus status ->
    { model with FilteredStatus = status }, Cmd.none

  | ClearError ->
    { model with Error = None }, Cmd.none