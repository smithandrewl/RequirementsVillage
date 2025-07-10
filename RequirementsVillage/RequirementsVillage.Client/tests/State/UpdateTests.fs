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
    
    testList "State transitions" [
      
      test "full loading cycle - success path" {
        // Start with initial state
        let model0 = State.initialModel()
        
        // Navigate to Dashboard (triggers LoadProjects)
        let model1, cmd1 = update (NavigateTo Dashboard) model0
        Assert.equal Dashboard model1.UI.CurrentPage "Should be on Dashboard"
        Assert.isTrue (Elmish.cmdContainsMessage LoadProjects cmd1) "Should trigger load"
        
        // LoadProjects dispatches StartLoading
        let model2, cmd2 = update LoadProjects model1
        let messages = Elmish.extractMessages cmd2
        Assert.isTrue 
          (messages |> List.contains (StartLoading LoadingProjects))
          "Should start loading"
        
        // Apply StartLoading
        let model3, _ = update (StartLoading LoadingProjects) model2
        Assert.isTrue (UIState.isLoadingProjects model3.UI) "Should be loading"
        
        // Projects loaded successfully
        let projects = Generate.projects 5
        let model4, _ = update (ProjectsLoaded (Ok projects)) model3
        Assert.equal projects model4.Domain.Projects "Projects should be loaded"
        Assert.isFalse (UIState.isLoadingProjects model4.UI) "Should stop loading"
        Assert.isNone model4.UI.Error "Should have no error"
      }
      
      test "full loading cycle - error path" {
        // Start loading
        let model = 
          State.initialModel()
          |> State.withPage Dashboard
          |> State.withLoading LoadingProjects
        
        // Error occurs
        let error = NetworkError "Connection timeout"
        let model2, _ = update (ProjectsLoaded (Error error)) model
        
        Assert.isEmpty model2.Domain.Projects "Projects should remain empty"
        Assert.isFalse (UIState.isLoadingProjects model2.UI) "Should stop loading"
        Assert.isSome model2.UI.Error "Should have error"
        
        // Clear error
        let model3, _ = update ClearError model2
        Assert.isNone model3.UI.Error "Error should be cleared"
      }
    ]
    
    testList "Edge cases and error scenarios" [
      
      test "multiple StartLoading messages maintain all operations" {
        let projectId = Guid.NewGuid()
        let model = State.initialModel()
        
        // Add multiple loading operations
        let model1, _ = update (StartLoading LoadingProjects) model
        let model2, _ = update (StartLoading CreatingProject) model1
        let model3, _ = update (StartLoading (UpdatingProject projectId)) model2
        
        Assert.equal 3 model3.UI.LoadingOperations.Count "Should have 3 operations"
        Assert.isTrue (UIState.isLoadingProjects model3.UI) "Should be loading projects"
        Assert.isTrue (UIState.isCreatingProject model3.UI) "Should be creating project"
        Assert.isTrue (UIState.isLoadingProject projectId model3.UI) "Should be loading specific project"
      }
      
      test "StopLoading only removes specific operation" {
        let projectId1 = Guid.NewGuid()
        let projectId2 = Guid.NewGuid()
        
        let model = 
          State.initialModel()
          |> State.withLoading (UpdatingProject projectId1)
          |> State.withLoading (UpdatingProject projectId2)
          |> State.withLoading LoadingProjects
        
        let model2, _ = update (StopLoading (UpdatingProject projectId1)) model
        
        Assert.isFalse (UIState.isLoadingProject projectId1 model2.UI) "Project 1 should not be loading"
        Assert.isTrue (UIState.isLoadingProject projectId2 model2.UI) "Project 2 should still be loading"
        Assert.isTrue (UIState.isLoadingProjects model2.UI) "Should still be loading projects"
        Assert.equal 2 model2.UI.LoadingOperations.Count "Should have 2 operations remaining"
      }
      
      test "navigating away from Dashboard preserves state" {
        let projects = Generate.projects 3
        let model = 
          State.initialModel()
          |> State.withProjects projects
          |> State.withPage Dashboard
          |> State.withFilter (Some InProgress)
        
        let model2, cmd = update (NavigateTo Landing) model
        
        Assert.equal Landing model2.UI.CurrentPage "Page should change"
        Assert.equal projects model2.Domain.Projects "Projects should be preserved"
        Assert.equal (Some InProgress) model2.UI.FilteredStatus "Filter should be preserved"
        Assert.isTrue (Elmish.cmdIsEmpty cmd) "Should have no commands"
      }
      
      test "error messages preserve other UI state" {
        let model = 
          State.initialModel()
          |> State.withPage Dashboard
          |> State.withTheme Dark
          |> State.withFilter (Some Completed)
          |> State.withLoading LoadingProjects
        
        let error = ServerError (404, "Not found")
        let model2, _ = update (ProjectsLoaded (Error error)) model
        
        // Error should be set but other state preserved
        Assert.isSome model2.UI.Error "Should have error"
        Assert.equal Dashboard model2.UI.CurrentPage "Page should be preserved"
        Assert.equal Dark model2.UI.CurrentTheme "Theme should be preserved"
        Assert.equal (Some Completed) model2.UI.FilteredStatus "Filter should be preserved"
        Assert.isFalse (UIState.isLoadingProjects model2.UI) "Loading should be cleared"
      }
    ]
    
    testList "init function edge cases" [
      
      test "init returns consistent state on multiple calls" {
        let model1, cmd1 = init()
        let model2, cmd2 = init()
        
        // Models should have same structure (not same instance)
        Assert.equal model1.UI.CurrentPage model2.UI.CurrentPage "Pages should match"
        Assert.equal model1.UI.CurrentTheme model2.UI.CurrentTheme "Themes should match"
        Assert.isEmpty model1.Domain.Projects "Should have no projects"
        Assert.isEmpty model2.Domain.Projects "Should have no projects"
        
        // Commands should be empty
        Assert.isTrue (Elmish.cmdIsEmpty cmd1) "First init should have no commands"
        Assert.isTrue (Elmish.cmdIsEmpty cmd2) "Second init should have no commands"
      }
    ]
  ]