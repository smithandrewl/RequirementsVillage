module RequirementsVillage.Client.App

open Elmish
open Elmish.React
open Feliz
open RequirementsVillage.Client.Types
open RequirementsVillage.Client.State
open RequirementsVillage.Client.Pages
open RequirementsVillage.Client.Components

let view (model: Model) (dispatch: Msg -> unit) =
  match model.CurrentPage with
  | Landing ->
    Landing.view dispatch
  | Dashboard ->
    Layout.view model dispatch (Dashboard.view model dispatch)

Program.mkProgram init update view
|> Program.withReactSynchronous "app"
|> Program.run
