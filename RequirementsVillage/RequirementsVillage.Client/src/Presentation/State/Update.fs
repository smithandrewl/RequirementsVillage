module RequirementsVillage.Client.Presentation.State.Update

open Elmish
open RequirementsVillage.Shared
open RequirementsVillage.Client.Presentation.State.Types
open RequirementsVillage.Client.Infrastructure.Api.Project
open RequirementsVillage.Client.Infrastructure.Storage.ThemeStorage
open RequirementsVillage.Client.Infrastructure.Api.Types

let init () : Model * Cmd<Msg> =
  let savedTheme = Theme.load()
  
  // Apply the theme to DOM on startup
  Theme.applyToDom savedTheme

  let initialModel = {
    Domain = {
      Projects = []
    }
    UI = {
      CurrentPage       = Landing
      CurrentTheme      = savedTheme
      FilteredStatus    = None
      LoadingOperations = Set.empty
      Error             = None
    }
  }

  initialModel, Cmd.none

let update (msg: Msg) (model: Model) : Model * Cmd<Msg> =
  match msg with
  | NavigateTo page ->
    let cmd =
      match page with
      | Dashboard when List.isEmpty model.Domain.Projects ->
        Cmd.ofMsg LoadProjects
      | _ -> Cmd.none
    { model with
        UI = {
          model.UI with
            CurrentPage = page
        }
    }, cmd

  | SetTheme theme ->
    { model with
        UI = {
          model.UI with
            CurrentTheme = theme
        }
    }, Cmd.none

  | LoadProjects ->
    let loadCmd =
      Cmd.batch [
        Cmd.ofMsg (StartLoading LoadingProjects)
        Cmd.OfPromise.perform
          getProjects
          ()
          ProjectsLoaded
      ]
    model, loadCmd

  | ProjectsLoaded (Ok projects) ->
    { model with
        Domain = {
          model.Domain with
            Projects = projects
        }
        UI = {
          model.UI with
            LoadingOperations = model.UI.LoadingOperations |> Set.remove LoadingProjects
            Error             = None
        }
    }, Cmd.none

  | ProjectsLoaded (Error error) ->
    let errorMsg =
      match error with
      | NetworkError msg        -> $"Network error: {msg}"
      | DecodingError msg       -> $"Data error: {msg}"
      | ServerError (code, msg) -> $"Server error ({code}): {msg}"
    { model with
        UI = {
          model.UI with
            LoadingOperations = model.UI.LoadingOperations |> Set.remove LoadingProjects
            Error             = Some errorMsg
        }
    }, Cmd.none

  | FilterByStatus status ->
    { model with
        UI = {
          model.UI with
            FilteredStatus = status
        }
    }, Cmd.none

  | ClearError ->
    { model with
        UI = {
          model.UI with
            Error = None
        }
    }, Cmd.none

  | StartLoading operation ->
    { model with
        UI = {
          model.UI with
            LoadingOperations = model.UI.LoadingOperations |> Set.add operation
        }
    }, Cmd.none

  | StopLoading operation ->
    { model with
        UI = {
          model.UI with
            LoadingOperations = model.UI.LoadingOperations |> Set.remove operation
        }
    }, Cmd.none

