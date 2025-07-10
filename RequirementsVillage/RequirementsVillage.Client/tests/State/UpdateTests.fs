module RequirementsVillage.Client.Tests.State.UpdateTests

open Fable.Mocha
open Elmish
open RequirementsVillage.Client.Presentation.State.Types
open RequirementsVillage.Client.Presentation.State.Update
open RequirementsVillage.Client.Infrastructure.Api.Types
open RequirementsVillage.Client.Tests.Helpers.TestHelpers
open RequirementsVillage.Client.Tests.Helpers.TestData
open RequirementsVillage.Client.Domain.Project

let tests =
  testList "State.Update Tests" [
    
    testList "init function" [
      
      test "initializes with correct default state" {
        let model, cmd = init()
        
        Assert.isEmpty model.Domain.Projects "Should have no projects"
        Assert.equal Landing model.UI.CurrentPage "Should start on Landing"
        Assert.isNone model.UI.FilteredStatus "Should have no filter"
        Assert.isEmpty model.UI.LoadingOperations "Should have no loading ops"
        Assert.isNone model.UI.Error "Should have no error"
        Assert.isTrue (Elmish.cmdIsEmpty cmd) "Should have no commands"
      }
    ]
    
    testList "NavigateTo message" [
      
      test "navigating to Dashboard with no projects triggers LoadProjects" {
        let model = State.initialModel()
        let newModel, cmd = update (NavigateTo Dashboard) model
        
        Assert.equal Dashboard newModel.UI.CurrentPage "Page should update"
        Assert.isTrue 
          (Elmish.cmdContainsMessage LoadProjects cmd)
          "Should trigger LoadProjects"
      }
      
      test "navigating to Dashboard with existing projects does not load" {
        let model = 
          State.initialModel() 
          |> State.withProjects (Generate.projects 3)
        
        let newModel, cmd = update (NavigateTo Dashboard) model
        
        Assert.equal Dashboard newModel.UI.CurrentPage "Page should update"
        Assert.isTrue (Elmish.cmdIsEmpty cmd) "Should not trigger load"
      }
      
      test "navigating to Landing does not trigger any commands" {
        let model = 
          State.initialModel() 
          |> State.withPage Dashboard
        
        let newModel, cmd = update (NavigateTo Landing) model
        
        Assert.equal Landing newModel.UI.CurrentPage "Page should update"
        Assert.isTrue (Elmish.cmdIsEmpty cmd) "Should have no commands"
      }
    ]
    
    testList "SetTheme message" [
      
      test "updates theme from Light to Dark" {
        let model = State.initialModel()
        let newModel, cmd = update (SetTheme Dark) model
        
        Assert.equal Dark newModel.UI.CurrentTheme "Theme should be Dark"
        Assert.isTrue (Elmish.cmdIsEmpty cmd) "Should have no commands"
      }
      
      test "updates theme from Dark to Light" {
        let model = 
          State.initialModel() 
          |> State.withTheme Dark
        
        let newModel, cmd = update (SetTheme Light) model
        
        Assert.equal Light newModel.UI.CurrentTheme "Theme should be Light"
        Assert.isTrue (Elmish.cmdIsEmpty cmd) "Should have no commands"
      }
    ]
    
    testList "LoadProjects message" [
      
      test "starts loading and dispatches API call" {
        let model = State.initialModel()
        let newModel, cmd = update LoadProjects model
        
        // Model should not change for LoadProjects
        Assert.equal model newModel "Model should not change"
        
        // Should dispatch StartLoading and API call
        let messages = Elmish.extractMessages cmd
        Assert.isTrue 
          (messages |> List.contains (StartLoading LoadingProjects))
          "Should start loading"
      }
    ]
    
    testList "ProjectsLoaded message" [
      
      test "successful load updates projects and clears loading" {
        let projects = Generate.projects 5
        let model = 
          State.initialModel()
          |> State.withLoading LoadingProjects
        
        let newModel, cmd = 
          update (ProjectsLoaded (Ok projects)) model
        
        Assert.equal projects newModel.Domain.Projects "Projects should update"
        Assert.isFalse 
          (UIState.isLoadingProjects newModel.UI)
          "Should stop loading"
        Assert.isNone newModel.UI.Error "Should have no error"
        Assert.isTrue (Elmish.cmdIsEmpty cmd) "Should have no commands"
      }
      
      test "failed load sets error and clears loading" {
        let error = NetworkError "Connection failed"
        let model = 
          State.initialModel()
          |> State.withLoading LoadingProjects
        
        let newModel, cmd = 
          update (ProjectsLoaded (Error error)) model
        
        Assert.isEmpty newModel.Domain.Projects "Projects should be empty"
        Assert.isFalse 
          (UIState.isLoadingProjects newModel.UI)
          "Should stop loading"
        Assert.isSome newModel.UI.Error "Should have error"
        Assert.isTrue 
          (newModel.UI.Error.Value.Contains("Network error"))
          "Error should contain network message"
        Assert.isTrue (Elmish.cmdIsEmpty cmd) "Should have no commands"
      }
      
      test "different error types produce correct messages" {
        let testCases = [
          NetworkError "Timeout", "Network error: Timeout"
          DecodingError "Invalid JSON", "Data error: Invalid JSON"
          ServerError (500, "Internal error"), "Server error (500): Internal error"
        ]
        
        for error, expectedMsg in testCases do
          let model = State.initialModel()
          let newModel, _ = 
            update (ProjectsLoaded (Error error)) model
          
          match newModel.UI.Error with
          | Some msg ->
              Assert.equal expectedMsg msg $"Error message for {error}"
          | None ->
              Assert.isTrue false "Should have error message"
      }
    ]
    
    testList "FilterByStatus message" [
      
      test "sets status filter" {
        let model = State.initialModel()
        let newModel, cmd = 
          update (FilterByStatus (Some InProgress)) model
        
        Assert.equal 
          (Some InProgress) 
          newModel.UI.FilteredStatus 
          "Filter should be set"
        Assert.isTrue (Elmish.cmdIsEmpty cmd) "Should have no commands"
      }
      
      test "clears status filter with None" {
        let model = 
          State.initialModel()
          |> State.withFilter (Some Completed)
        
        let newModel, cmd = update (FilterByStatus None) model
        
        Assert.isNone newModel.UI.FilteredStatus "Filter should be cleared"
        Assert.isTrue (Elmish.cmdIsEmpty cmd) "Should have no commands"
      }
    ]
    
    testList "Loading operations" [
      
      test "StartLoading adds operation to set" {
        let model = State.initialModel()
        let newModel, cmd = 
          update (StartLoading CreatingProject) model
        
        Assert.isTrue 
          (UIState.isCreatingProject newModel.UI)
          "Should be creating project"
        Assert.isTrue (Elmish.cmdIsEmpty cmd) "Should have no commands"
      }
      
      test "StopLoading removes operation from set" {
        let model = 
          State.initialModel()
          |> State.withLoading CreatingProject
        
        let newModel, cmd = 
          update (StopLoading CreatingProject) model
        
        Assert.isFalse 
          (UIState.isCreatingProject newModel.UI)
          "Should not be creating project"
        Assert.isTrue (Elmish.cmdIsEmpty cmd) "Should have no commands"
      }
      
      test "multiple loading operations can coexist" {
        let model = 
          State.initialModel()
          |> State.withLoading LoadingProjects
        
        let newModel, _ = 
          update (StartLoading CreatingProject) model
        
        Assert.isTrue 
          (UIState.isLoadingProjects newModel.UI)
          "Should still be loading projects"
        Assert.isTrue 
          (UIState.isCreatingProject newModel.UI)
          "Should be creating project"
        Assert.equal 2 newModel.UI.LoadingOperations.Count "Should have 2 ops"
      }
    ]
    
    testList "ClearError message" [
      
      test "clears existing error" {
        let model = 
          State.initialModel()
          |> State.withError "Test error"
        
        let newModel, cmd = update ClearError model
        
        Assert.isNone newModel.UI.Error "Error should be cleared"
        Assert.isTrue (Elmish.cmdIsEmpty cmd) "Should have no commands"
      }
      
      test "clearing when no error does nothing" {
        let model = State.initialModel()
        let newModel, cmd = update ClearError model
        
        Assert.equal model newModel "Model should not change"
        Assert.isTrue (Elmish.cmdIsEmpty cmd) "Should have no commands"
      }
    ]
  ]