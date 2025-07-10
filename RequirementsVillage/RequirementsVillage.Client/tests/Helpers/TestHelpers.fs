module RequirementsVillage.Client.Tests.Helpers.TestHelpers

open Fable.Core
open Fable.Mocha
open RequirementsVillage.Client.Presentation.State.Types
open RequirementsVillage.Client.Infrastructure.Storage.ThemeStorage
open RequirementsVillage.Shared
open Elmish

// Test assertion helpers
module Assert =
  
  let equal expected actual message =
    Expect.equal actual expected message
  
  let notEqual expected actual message =
    Expect.notEqual actual expected message
  
  let isTrue value message =
    Expect.isTrue value message
  
  let isFalse value message =
    Expect.isFalse value message
  
  let contains item list message =
    Expect.contains list item message
  
  let isEmpty list message =
    Expect.isEmpty list message
  
  let isNone option message =
    Expect.isNone option message
  
  let isSome option message =
    Expect.isSome option message
  
  let throws fn message =
    Expect.throws fn message

// State test helpers
module State =
  
  let initialModel () : Model = {
    Domain = { Projects = [] }
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
  
  let withPage page model =
    { model with UI = { model.UI with CurrentPage = page } }
  
  let withTheme theme model =
    { model with UI = { model.UI with CurrentTheme = theme } }
  
  let withError error model =
    { model with UI = { model.UI with Error = Some error } }
  
  let withLoading operation model =
    { model with 
        UI = { model.UI with 
                LoadingOperations = 
                  model.UI.LoadingOperations |> Set.add operation } }
  
  let withFilter status model =
    { model with UI = { model.UI with FilteredStatus = status } }

// Elmish test helpers
module Elmish =
  
  type TestCmd<'msg> = 
    | None
    | Batch of TestCmd<'msg> list
    | OfFunc of (unit -> 'msg)
    | OfAsync of Async<'msg>
    | OfPromise of JS.Promise<'msg>
  
  // Convert Elmish Cmd to TestCmd for easier testing
  let rec cmdToTestCmd (cmd: Cmd<'msg>) : TestCmd<'msg> =
    match cmd with
    | [] -> None
    | [x] -> 
        match x with
        | Elmish.Dispatch msg -> OfFunc (fun () -> msg)
        | _ -> None // Simplified for testing
    | multiple -> 
        Batch (multiple |> List.map (fun c -> cmdToTestCmd [c]))
  
  // Extract messages from commands
  let rec extractMessages (cmd: Cmd<'msg>) : 'msg list =
    match cmd with
    | [] -> []
    | cmds ->
        cmds |> List.collect (function
          | Elmish.Dispatch msg -> [msg]
          | _ -> []
        )
  
  // Test that a command contains a specific message
  let cmdContainsMessage (msg: 'msg) (cmd: Cmd<'msg>) =
    extractMessages cmd |> List.contains msg
  
  // Test that a command is empty
  let cmdIsEmpty (cmd: Cmd<'msg>) =
    match cmd with
    | [] -> true
    | _ -> false

// Component test helpers
module Component =
  
  open Feliz
  
  // Helper to render component and extract text content
  let renderToText (element: ReactElement) : string =
    // In a real test environment, this would use React test renderer
    // For now, we'll use a placeholder
    "rendered component"
  
  // Helper to check if element contains class
  let hasClass className (element: ReactElement) : bool =
    // Placeholder implementation
    false
  
  // Helper to simulate click events
  let simulateClick (element: ReactElement) : unit =
    // Placeholder implementation
    ()

// Async test helpers
module Async =
  
  open Fable.Core.JS
  
  let runSync (computation: Async<'a>) : 'a =
    let mutable result = None
    let mutable error = None
    
    computation
    |> Async.StartImmediate (fun res ->
      result <- Some res
    )
    
    match result, error with
    | Some value, _ -> value
    | None, Some e -> raise e
    | None, None -> failwith "Async computation did not complete"
  
  let delay ms = 
    Async.AwaitPromise (Promise.create (fun resolve _ ->
      setTimeout resolve ms |> ignore
    ))