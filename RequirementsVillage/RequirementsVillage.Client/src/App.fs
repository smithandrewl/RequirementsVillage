module RequirementsVillage.Client.App

open Elmish
open Elmish.React
open Feliz
open RequirementsVillage.Client.Models.Domain
open RequirementsVillage.Client.State.Types
open RequirementsVillage.Client.State.Update
open RequirementsVillage.Client.Pages
open RequirementsVillage.Client.Components

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
