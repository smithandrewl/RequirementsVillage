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
      
      test "withFilter updates filter status" {
        let model = 
          State.initialModel()
          |> State.withFilter (Some InProgress)
        
        Assert.equal (Some InProgress) model.UI.FilteredStatus "Filter should be set"
      }
      
      test "clearing error with None" {
        let model = 
          State.initialModel()
          |> State.withError "Some error"
        
        Assert.isSome model.UI.Error "Error should be set initially"
        
        let clearedModel = { model with UI = { model.UI with Error = None } }
        Assert.isNone clearedModel.UI.Error "Error should be cleared"
      }
    ]
    
    testList "LoadingOperation discriminated union" [
      
      test "LoadingOperation equality works correctly" {
        let id1 = Guid.NewGuid()
        let id2 = Guid.NewGuid()
        
        Assert.equal LoadingProjects LoadingProjects "Same operations should be equal"
        Assert.notEqual (UpdatingProject id1) (UpdatingProject id2) "Different IDs should not be equal"
        Assert.equal (UpdatingProject id1) (UpdatingProject id1) "Same project ID should be equal"
      }
      
      test "LoadingOperation can be added to Set correctly" {
        let id = Guid.NewGuid()
        let operations = 
          Set.empty
          |> Set.add LoadingProjects
          |> Set.add CreatingProject
          |> Set.add (UpdatingProject id)
          |> Set.add (DeletingProject id)
        
        Assert.equal 4 operations.Count "Should have 4 unique operations"
        
        // Adding same operation should not increase count
        let operations2 = operations |> Set.add LoadingProjects
        Assert.equal 4 operations2.Count "Adding duplicate should not increase count"
      }
    ]
    
    testList "Page type" [
      
      test "Page values are distinct" {
        Assert.notEqual Landing Dashboard "Landing and Dashboard should be different"
      }
    ]
    
    testList "DomainState" [
      
      test "empty DomainState has no projects" {
        let domain = { Projects = [] }
        Assert.isEmpty domain.Projects "Should have no projects"
      }
      
      test "DomainState with projects maintains order" {
        let projects = Generate.projects 5
        let domain = { Projects = projects }
        Assert.equal projects domain.Projects "Projects should maintain order"
      }
    ]
    
    testList "UIState edge cases" [
      
      test "isLoadingProject handles empty operations set" {
        let uiState = {
          CurrentPage       = Dashboard
          CurrentTheme      = Light
          FilteredStatus    = None
          LoadingOperations = Set.empty
          Error             = None
        }
        let projectId = Guid.NewGuid()
        
        let result = UIState.isLoadingProject projectId uiState
        Assert.isFalse result "Should return false for empty operations"
      }
      
      test "multiple loading operations of same type but different IDs" {
        let id1 = Guid.NewGuid()
        let id2 = Guid.NewGuid()
        let id3 = Guid.NewGuid()
        
        let uiState = {
          CurrentPage       = Dashboard
          CurrentTheme      = Light
          FilteredStatus    = None
          LoadingOperations = Set.ofList [
            UpdatingProject id1
            UpdatingProject id2
            DeletingProject id3
          ]
          Error             = None
        }
        
        Assert.isTrue (UIState.isLoadingProject id1 uiState) "Should detect id1"
        Assert.isTrue (UIState.isLoadingProject id2 uiState) "Should detect id2"
        Assert.isTrue (UIState.isLoadingProject id3 uiState) "Should detect id3"
        Assert.equal 3 uiState.LoadingOperations.Count "Should have 3 operations"
      }
    ]
  ]