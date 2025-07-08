module RequirementsVillage.Client.Elm.State

open Elmish
open RequirementsVillage.Client.Domain
open RequirementsVillage.Client.Elm.Types
open RequirementsVillage.Client.Constants
open Browser.Dom

let init () : Model * Cmd<Msg> =
  let savedTheme =
    match window.localStorage.getItem(Theme.StorageKey) with
    | null                         -> Light
    | value when value = Theme.Dark -> Dark
    | _                            -> Light

  // Apply the theme to DOM on startup
  let themeValue = 
    match savedTheme with
    | Light -> Theme.Light
    | Dark  -> Theme.Dark
  document.documentElement.setAttribute("data-theme", themeValue)

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
        RequirementsVillage.Client.Api.Projects.getProjects
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