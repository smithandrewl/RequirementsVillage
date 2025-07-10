module RequirementsVillage.Client.Tests.State.TypesTests

open Fable.Mocha
open RequirementsVillage.Client.Presentation.State.Types
open RequirementsVillage.Client.Tests.Helpers.TestHelpers
open RequirementsVillage.Client.Tests.Helpers.TestData
open System

let tests = 
  testList "State.Types Tests" [
    
    testList "UIState helper functions" [
      
      test "isLoading returns true when operation is in set" {
        let uiState = {
          CurrentPage       = Dashboard
          CurrentTheme      = Light
          FilteredStatus    = None
          LoadingOperations = Set.ofList [LoadingProjects]
          Error             = None
        }
        
        let result = UIState.isLoading LoadingProjects uiState
        Assert.isTrue result "Should return true for active operation"
      }
      
      test "isLoading returns false when operation is not in set" {
        let uiState = {
          CurrentPage       = Dashboard
          CurrentTheme      = Light
          FilteredStatus    = None
          LoadingOperations = Set.ofList [CreatingProject]
          Error             = None
        }
        
        let result = UIState.isLoading LoadingProjects uiState
        Assert.isFalse result "Should return false for inactive operation"
      }
      
      test "isLoadingAny returns true when any operation is active" {
        let uiState = {
          CurrentPage       = Dashboard
          CurrentTheme      = Light
          FilteredStatus    = None
          LoadingOperations = Set.ofList [LoadingProjects; CreatingProject]
          Error             = None
        }
        
        let result = UIState.isLoadingAny uiState
        Assert.isTrue result "Should return true when operations exist"
      }
      
      test "isLoadingAny returns false when no operations active" {
        let uiState = {
          CurrentPage       = Dashboard
          CurrentTheme      = Light
          FilteredStatus    = None
          LoadingOperations = Set.empty
          Error             = None
        }
        
        let result = UIState.isLoadingAny uiState
        Assert.isFalse result "Should return false when no operations"
      }
      
      test "isLoadingProject detects all project-specific operations" {
        let projectId = Guid.NewGuid()
        let operations = [
          UpdatingProject projectId
          DeletingProject projectId
          UpdatingStatus projectId
          LoadingProjectDetail projectId
        ]
        
        for operation in operations do
          let uiState = {
            CurrentPage       = Dashboard
            CurrentTheme      = Light
            FilteredStatus    = None
            LoadingOperations = Set.ofList [operation]
            Error             = None
          }
          
          let result = UIState.isLoadingProject projectId uiState
          Assert.isTrue result $"Should detect {operation}"
      }
      
      test "isLoadingProject returns false for different project ID" {
        let projectId1 = Guid.NewGuid()
        let projectId2 = Guid.NewGuid()
        
        let uiState = {
          CurrentPage       = Dashboard
          CurrentTheme      = Light
          FilteredStatus    = None
          LoadingOperations = Set.ofList [UpdatingProject projectId1]
          Error             = None
        }
        
        let result = UIState.isLoadingProject projectId2 uiState
        Assert.isFalse result "Should return false for different project ID"
      }
    ]
    
    testList "Model initialization" [
      
      test "initial model has correct default values" {
        let model = State.initialModel()
        
        Assert.isEmpty model.Domain.Projects "Projects should be empty"
        Assert.equal Landing model.UI.CurrentPage "Should start on Landing page"
        Assert.equal Light model.UI.CurrentTheme "Should default to Light theme"
        Assert.isNone model.UI.FilteredStatus "Should have no filter"
        Assert.isEmpty model.UI.LoadingOperations "Should have no loading operations"
        Assert.isNone model.UI.Error "Should have no error"
      }
    ]
    
    testList "State builder functions" [
      
      test "withProjects updates projects list" {
        let projects = Generate.projects 3
        let model = 
          State.initialModel()
          |> State.withProjects projects
        
        Assert.equal projects model.Domain.Projects "Projects should be updated"
      }
      
      test "withPage updates current page" {
        let model = 
          State.initialModel()
          |> State.withPage Dashboard
        
        Assert.equal Dashboard model.UI.CurrentPage "Page should be updated"
      }
      
      test "withError sets error message" {
        let errorMsg = "Test error"
        let model = 
          State.initialModel()
          |> State.withError errorMsg
        
        Assert.equal (Some errorMsg) model.UI.Error "Error should be set"
      }
      
      test "withLoading adds loading operation" {
        let model = 
          State.initialModel()
          |> State.withLoading LoadingProjects
        
        Assert.isTrue 
          (model.UI.LoadingOperations |> Set.contains LoadingProjects)
          "Loading operation should be added"
      }
      
      test "multiple operations can be composed" {
        let projects = Generate.projects 2
        let model = 
          State.initialModel()
          |> State.withProjects projects
          |> State.withPage Dashboard
          |> State.withTheme Dark
          |> State.withLoading LoadingProjects
        
        Assert.equal projects model.Domain.Projects "Projects should be set"
        Assert.equal Dashboard model.UI.CurrentPage "Page should be Dashboard"
        Assert.equal Dark model.UI.CurrentTheme "Theme should be Dark"
        Assert.isTrue (UIState.isLoadingProjects model.UI) "Should be loading"
      }
    ]
  ]