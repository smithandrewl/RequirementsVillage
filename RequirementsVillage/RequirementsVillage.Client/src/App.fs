module RequirementsVillage.Client.App

open Elmish
open Elmish.React
open Elmish.Navigation
open Browser.Types
open Feliz
open RequirementsVillage.Client.Presentation.State.Types
open RequirementsVillage.Client.Presentation.State.Update
open RequirementsVillage.Client.Presentation.Pages
open RequirementsVillage.Client.Presentation.Components
open RequirementsVillage.Client.Routes

let view (model: Model) (dispatch: Msg -> unit) =
  match model.UI.CurrentPage with
  | Landing   -> Layout.landingView model dispatch (Landing.view dispatch)
  | Dashboard -> Layout.appView model dispatch (Dashboard.view model dispatch)

// URL update for navigation
let urlUpdate (result: Page option) model =
  match result with
  | Some page ->
    model, Cmd.ofMsg (UrlChanged page)
  | None ->
    // Default to landing for invalid URLs
    model, Cmd.ofMsg (UrlChanged Landing)

// Parse location to route  
let route (location: Location) : Page option =
  let segments = 
    location.pathname.Split([|'/'|], System.StringSplitOptions.RemoveEmptyEntries)
    |> Array.toList
  parseUrl segments |> Some

let program =
  Program.mkProgram init update view
  |> Program.toNavigable route urlUpdate
  |> Program.withReactSynchronous "app"

program |> Program.run
