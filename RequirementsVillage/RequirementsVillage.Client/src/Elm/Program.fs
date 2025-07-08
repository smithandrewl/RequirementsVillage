module RequirementsVillage.Client.Elm.Program

open Elmish
open Elmish.React
open Feliz
open RequirementsVillage.Client.Elm.Types
open RequirementsVillage.Client.Elm.State
open RequirementsVillage.Client.Pages
open RequirementsVillage.Client.Components

let view (model: Model) (dispatch: Msg -> unit) =
  match model.CurrentPage with
  | Landing ->
    Landing.view dispatch
  | Dashboard ->
    Layout.view model dispatch (Dashboard.view model dispatch)

let program =
  Program.mkProgram init update view
  |> Program.withReactSynchronous "app"