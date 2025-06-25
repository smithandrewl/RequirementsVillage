module RequirementsVillage.FSharp.Client.App

open Elmish
open Elmish.React
open Feliz
open RequirementsVillage.FSharp.Client.Types
open RequirementsVillage.FSharp.Client.State
open RequirementsVillage.FSharp.Client.Pages
open RequirementsVillage.FSharp.Client.Components

let view (model: Model) (dispatch: Msg -> unit) =
  match model.CurrentPage with
  | Landing -> 
    Landing.view dispatch
  | Dashboard -> 
    Layout.view model dispatch (Dashboard.view model dispatch)

Program.mkProgram init update view
|> Program.withReactSynchronous "app"
|> Program.run