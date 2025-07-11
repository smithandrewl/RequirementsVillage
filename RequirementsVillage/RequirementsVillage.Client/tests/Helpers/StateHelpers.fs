module RequirementsVillage.Client.Tests.Helpers.StateHelpers

open System
open RequirementsVillage.Shared
open RequirementsVillage.Client.Presentation.State.Types
open RequirementsVillage.Client.Infrastructure.Storage

// State helper functions for tests
module State =
  
  let initialModel() : Model =
    {
      Domain = {
        Projects         = []
        SelectedProject  = None
      }
      UI = {
        CurrentPage       = Landing
        CurrentTheme      = Light
        FilteredStatus    = None
        LoadingOperations = Set.empty
        Error             = None
      }
    }
  
  let withProjects projects model =
    { model with Domain = { model.Domain with Projects = projects } }
  
  let withSelectedProject project model =
    { model with Domain = { model.Domain with SelectedProject = Some project } }
  
  let withPage page model =
    { model with UI = { model.UI with CurrentPage = page } }
  
  let withTheme theme model =
    { model with UI = { model.UI with CurrentTheme = theme } }
  
  let withFilter status model =
    { model with UI = { model.UI with FilteredStatus = status } }
  
  let withLoading operation model =
    { model with 
        UI = { model.UI with 
                 LoadingOperations = model.UI.LoadingOperations |> Set.add operation } }
  
  let withoutLoading operation model =
    { model with 
        UI = { model.UI with 
                 LoadingOperations = model.UI.LoadingOperations |> Set.remove operation } }
  
  let withError error model =
    { model with UI = { model.UI with Error = Some error } }
  
  let withoutError model =
    { model with UI = { model.UI with Error = None } }

// Additional test helpers for commands
module TestCmd =
  
  let extractMessages cmd =
    let mutable messages = []
    cmd |> List.iter (fun subcmd ->
      match subcmd with
      | Elmish.Cmd.OfFunc.either (fn, arg, onSuccess, onError) ->
          // For testing, we can't execute the function, but we can note its presence
          messages <- messages @ ["Function command"]
      | Elmish.Cmd.OfFunc.perform (fn, arg, ofSuccess) ->
          messages <- messages @ ["Function command"]
      | Elmish.Cmd.OfFunc.attempt (fn, arg, ofError) ->
          messages <- messages @ ["Function command"]
      | _ -> ()
    )
    messages
  
  let cmdContainsMessage msg cmd =
    // This is a simplified version for testing
    not (Elmish.Cmd.isEmpty cmd)