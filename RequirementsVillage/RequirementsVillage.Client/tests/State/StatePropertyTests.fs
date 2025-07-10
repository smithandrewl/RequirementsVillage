module RequirementsVillage.Client.Tests.State.StatePropertyTests

open Fable.Mocha
open RequirementsVillage.Client.Presentation.State.Types
open RequirementsVillage.Client.Presentation.State.Update
open RequirementsVillage.Client.Infrastructure.Api.Types
open RequirementsVillage.Shared
open RequirementsVillage.Client.Tests.Helpers.TestHelpers
open RequirementsVillage.Client.Tests.Helpers.TestData
open System

// Property-based test patterns without FsCheck
let tests =
  testList "State Property Tests" [
    
    testList "State invariants" [
      
      test "init always produces valid initial state" {
        // Run init multiple times to simulate property
        for _ in 1..10 do
          let model, cmd = init()
          
          // Verify invariants
          Assert.isEmpty model.Domain.Projects "Projects should always be empty"
          Assert.equal Landing model.UI.CurrentPage "Should always start on Landing"
          Assert.isTrue 
            (model.UI.CurrentTheme = Light || model.UI.CurrentTheme = Dark)
            "Theme should be valid"
          Assert.isEmpty model.UI.LoadingOperations "Should have no loading operations"
          Assert.isNone model.UI.Error "Should have no error"
      }
      
      test "any sequence of loading operations maintains consistency" {
        // Generate random sequences of loading operations
        let random = System.Random()
        let projectIds = [ for _ in 1..5 -> Guid.NewGuid() ]
        
        let randomOperation() =
          match random.Next(0, 6) with
          | 0 -> LoadingProjects
          | 1 -> CreatingProject
          | 2 -> UpdatingProject (projectIds.[random.Next(0, projectIds.Length)])
          | 3 -> DeletingProject (projectIds.[random.Next(0, projectIds.Length)])
          | 4 -> UpdatingStatus (projectIds.[random.Next(0, projectIds.Length)])
          | _ -> LoadingProjectDetail (projectIds.[random.Next(0, projectIds.Length)])
        
        // Test multiple random sequences
        for _ in 1..20 do
          let operationCount = random.Next(1, 10)
          let operations = [ for _ in 1..operationCount -> randomOperation() ]
          
          // Apply all operations
          let mutable model = State.initialModel()
          for op in operations do
            let newModel, _ = update (StartLoading op) model
            model <- newModel
          
          // Verify consistency
          Assert.equal operations.Length model.UI.LoadingOperations.Count 
            "Operation count should match"
          
          // Remove all operations
          for op in operations do
            let newModel, _ = update (StopLoading op) model
            model <- newModel
          
          Assert.isEmpty model.UI.LoadingOperations "All operations should be removed"
      }
      
      test "error state never affects domain data" {
        // Generate various error scenarios
        let errors = [
          NetworkError "Connection failed"
          DecodingError "Invalid JSON"
          ServerError (404, "Not found")
          ServerError (500, "Internal error")
          NetworkError ""
        ]
        
        let projects = Generate.projects 5
        let baseModel = 
          State.initialModel() 
          |> State.withProjects projects
        
        for error in errors do
          let model, _ = update (ProjectsLoaded (Error error)) baseModel
          
          // Domain data should remain unchanged
          Assert.equal projects model.Domain.Projects 
            "Projects should not change on error"
          Assert.isSome model.UI.Error "Error should be set"
          
          // Other UI state should be preserved
          Assert.equal baseModel.UI.CurrentPage model.UI.CurrentPage
          Assert.equal baseModel.UI.CurrentTheme model.UI.CurrentTheme
          Assert.equal baseModel.UI.FilteredStatus model.UI.FilteredStatus
      }
    ]
    
    testList "Message ordering properties" [
      
      test "multiple SetTheme messages result in last theme winning" {
        let themes = [Light; Dark; Light; Dark; Light]
        let mutable model = State.initialModel()
        
        for theme in themes do
          let newModel, _ = update (SetTheme theme) model
          model <- newModel
        
        Assert.equal Light model.UI.CurrentTheme "Last theme should win"
      }
      
      test "StartLoading followed by StopLoading is idempotent" {
        let operations = [
          LoadingProjects
          CreatingProject
          UpdatingProject (Guid.NewGuid())
          DeletingProject (Guid.NewGuid())
        ]
        
        for op in operations do
          let model0 = State.initialModel()
          
          // Start then stop
          let model1, _ = update (StartLoading op) model0
          let model2, _ = update (StopLoading op) model1
          
          // Should be back to original state
          Assert.equal model0.UI.LoadingOperations model2.UI.LoadingOperations
            $"Start/Stop {op} should be idempotent"
      }
      
      test "duplicate StartLoading messages are idempotent" {
        let op = LoadingProjects
        let model0 = State.initialModel()
        
        // Add same operation multiple times
        let model1, _ = update (StartLoading op) model0
        let model2, _ = update (StartLoading op) model1
        let model3, _ = update (StartLoading op) model2
        
        Assert.equal 1 model3.UI.LoadingOperations.Count 
          "Should only have one operation"
        Assert.equal model1.UI.LoadingOperations model3.UI.LoadingOperations
          "Multiple starts should be idempotent"
      }
    ]
    
    testList "State transition properties" [
      
      test "navigating to Dashboard always ensures consistent state" {
        // Test from various starting states
        let startingStates = [
          State.initialModel()
          State.initialModel() |> State.withError "Some error"
          State.initialModel() |> State.withTheme Dark
          State.initialModel() |> State.withLoading CreatingProject
          State.initialModel() |> State.withProjects (Generate.projects 3)
        ]
        
        for startModel in startingStates do
          let model, cmd = update (NavigateTo Dashboard) startModel
          
          Assert.equal Dashboard model.UI.CurrentPage "Should be on Dashboard"
          
          // If no projects, should trigger load
          if List.isEmpty startModel.Domain.Projects then
            Assert.isTrue (Elmish.cmdContainsMessage LoadProjects cmd)
              "Empty projects should trigger load"
          else
            Assert.isTrue (Elmish.cmdIsEmpty cmd)
              "Existing projects should not trigger load"
      }
      
      test "ProjectsLoaded always clears LoadingProjects regardless of result" {
        let model = 
          State.initialModel()
          |> State.withLoading LoadingProjects
          |> State.withLoading CreatingProject // Add another to ensure only LoadingProjects is cleared
        
        // Test success case
        let successModel, _ = 
          update (ProjectsLoaded (Ok (Generate.projects 3))) model
        Assert.isFalse (UIState.isLoadingProjects successModel.UI)
          "Success should clear LoadingProjects"
        Assert.isTrue (UIState.isCreatingProject successModel.UI)
          "Other operations should remain"
        
        // Test failure case
        let failureModel, _ = 
          update (ProjectsLoaded (Error (NetworkError "Failed"))) model
        Assert.isFalse (UIState.isLoadingProjects failureModel.UI)
          "Failure should clear LoadingProjects"
        Assert.isTrue (UIState.isCreatingProject failureModel.UI)
          "Other operations should remain"
      }
    ]
    
    testList "Complex scenarios" [
      
      test "rapid filter changes maintain consistency" {
        let statuses = [
          None
          Some Idea
          Some InProgress
          None
          Some Completed
          Some Abandoned
          None
          Some OnHold
        ]
        
        let mutable model = 
          State.initialModel() 
          |> State.withProjects (Generate.projectsWithMixedStatuses())
        
        for status in statuses do
          let newModel, cmd = update (FilterByStatus status) model
          model <- newModel
          
          Assert.equal status model.UI.FilteredStatus "Filter should match"
          Assert.isTrue (Elmish.cmdIsEmpty cmd) "Should have no commands"
        
        // Final state should match last filter
        Assert.equal (Some OnHold) model.UI.FilteredStatus "Should have last filter"
      }
      
      test "concurrent operations on same project maintain consistency" {
        let projectId = Guid.NewGuid()
        let model = State.initialModel()
        
        // Start multiple operations on same project
        let operations = [
          UpdatingProject projectId
          UpdatingStatus projectId
          LoadingProjectDetail projectId
        ]
        
        let mutable currentModel = model
        for op in operations do
          let newModel, _ = update (StartLoading op) currentModel
          currentModel <- newModel
        
        Assert.equal 3 currentModel.UI.LoadingOperations.Count
          "Should have all operations"
        Assert.isTrue (UIState.isLoadingProject projectId currentModel.UI)
          "Project should be loading"
        
        // Stop one operation
        let model2, _ = update (StopLoading (UpdatingProject projectId)) currentModel
        Assert.equal 2 model2.UI.LoadingOperations.Count
          "Should have 2 operations left"
        Assert.isTrue (UIState.isLoadingProject projectId model2.UI)
          "Project should still be loading"
        
        // Stop remaining operations
        let model3, _ = update (StopLoading (UpdatingStatus projectId)) model2
        let model4, _ = update (StopLoading (LoadingProjectDetail projectId)) model3
        
        Assert.isEmpty model4.UI.LoadingOperations "All operations should be cleared"
        Assert.isFalse (UIState.isLoadingProject projectId model4.UI)
          "Project should not be loading"
      }
      
      test "error recovery workflow maintains state integrity" {
        // Simulate a full error recovery workflow
        let model0 = 
          State.initialModel() 
          |> State.withPage Dashboard
        
        // 1. Start loading
        let model1, _ = update (StartLoading LoadingProjects) model0
        Assert.isTrue (UIState.isLoadingProjects model1.UI) "Should be loading"
        
        // 2. Error occurs
        let model2, _ = 
          update (ProjectsLoaded (Error (NetworkError "Timeout"))) model1
        Assert.isFalse (UIState.isLoadingProjects model2.UI) "Should stop loading"
        Assert.isSome model2.UI.Error "Should have error"
        
        // 3. User clears error
        let model3, _ = update ClearError model2
        Assert.isNone model3.UI.Error "Error should be cleared"
        
        // 4. Retry loading
        let model4, _ = update LoadProjects model3
        let messages = Elmish.extractMessages (snd (update LoadProjects model3))
        Assert.isTrue 
          (messages |> List.contains (StartLoading LoadingProjects))
          "Should start loading again"
        
        // 5. Success this time
        let projects = Generate.projects 5
        let model5, _ = 
          update (StartLoading LoadingProjects) model3
        let model6, _ = 
          update (ProjectsLoaded (Ok projects)) model5
        
        Assert.equal projects model6.Domain.Projects "Projects should be loaded"
        Assert.isFalse (UIState.isLoadingProjects model6.UI) "Should stop loading"
        Assert.isNone model6.UI.Error "Should have no error"
        Assert.equal Dashboard model6.UI.CurrentPage "Should still be on Dashboard"
      }
    ]
  ]