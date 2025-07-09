module RequirementsVillage.Client.App

open Elmish
open Elmish.React
open Feliz
open RequirementsVillage.Client.Presentation.State.Types
open RequirementsVillage.Client.Presentation.State.Update
open RequirementsVillage.Client.Presentation.Pages
open RequirementsVillage.Client.Presentation.Components

let view (model: Model) (dispatch: Msg -> unit) =
  match model.UI.CurrentPage with
  | Landing   -> Layout.landingView model dispatch (Landing.view dispatch)
  | Dashboard -> Layout.appView model dispatch (Dashboard.view model dispatch)

let program =
  Program.mkProgram init update view
  |> Program.withReactSynchronous "app"

program |> Program.run
