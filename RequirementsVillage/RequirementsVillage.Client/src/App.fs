module RequirementsVillage.Client.App

open Elmish
open Elmish.React
open Feliz
open RequirementsVillage.Client.Presentation.State.Types
open RequirementsVillage.Client.Presentation.State.Update
open RequirementsVillage.Client.Presentation.Pages
open RequirementsVillage.Client.Presentation.Components

let view (model: Model) (dispatch: Msg -> unit) =
  Layout.view model dispatch (
    match model.CurrentPage with
    | Landing   -> Landing.view dispatch
    | Dashboard -> Dashboard.view model dispatch
  )

let program =
  Program.mkProgram init update view
  |> Program.withReactSynchronous "elmish-app"

program |> Program.run
