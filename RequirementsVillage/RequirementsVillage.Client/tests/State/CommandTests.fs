module RequirementsVillage.Client.Tests.State.CommandTests

open Fable.Mocha
open Elmish
open RequirementsVillage.Client.Presentation.State.Types
open RequirementsVillage.Client.Presentation.State.Update
open RequirementsVillage.Client.Infrastructure.Api.Types
open RequirementsVillage.Client.Tests.Helpers.TestHelpers
open RequirementsVillage.Client.Tests.Helpers.TestData
open RequirementsVillage.Client.Domain.Project

let tests =
  testList "State Command Tests" [
    
    testList "LoadProjects command behavior" [
      
      test "LoadProjects generates correct command sequence" {
        let model = State.initialModel()
        let _, cmd = update LoadProjects model
        
        // Extract messages from the batch command
        let messages = Elmish.extractMessages cmd
        
        // Should contain StartLoading message
        Assert.isTrue 
          (messages |> List.exists (function StartLoading LoadingProjects -> true | _ -> false))
          "Should contain StartLoading LoadingProjects"
        
        // The command should also trigger the async API call
        // which will eventually dispatch ProjectsLoaded
        Assert.equal 1 (messages |> List.length) "Should have exactly one synchronous message"
      }
      
      test "LoadProjects does not modify model state directly" {
        let model = 
          State.initialModel()
          |> State.withProjects (Generate.projects 3)
          |> State.withPage Dashboard
          |> State.withTheme Dark
        
        let newModel, _ = update LoadProjects model
        
        // Model should remain unchanged
        Assert.equal model.Domain.Projects newModel.Domain.Projects "Projects unchanged"
        Assert.equal model.UI.CurrentPage newModel.UI.CurrentPage "Page unchanged"
        Assert.equal model.UI.CurrentTheme newModel.UI.CurrentTheme "Theme unchanged"
        Assert.equal model.UI.LoadingOperations newModel.UI.LoadingOperations "Loading unchanged"
      }
    ]
    
    testList "NavigateTo command generation" [
      
      test "NavigateTo Dashboard with empty projects generates LoadProjects command" {
        let model = State.initialModel()
        let _, cmd = update (NavigateTo Dashboard) model
        
        let messages = Elmish.extractMessages cmd
        Assert.isTrue 
          (messages |> List.contains LoadProjects)
          "Should dispatch LoadProjects"
      }
      
      test "NavigateTo Dashboard with existing projects generates no commands" {
        let model = 
          State.initialModel()
          |> State.withProjects (Generate.projects 1)
        
        let _, cmd = update (NavigateTo Dashboard) model
        
        Assert.isTrue (Elmish.cmdIsEmpty cmd) "Should generate no commands"
      }
      
      test "NavigateTo Landing always generates no commands" {
        // Test from various states
        let models = [
          State.initialModel()
          State.initialModel() |> State.withPage Dashboard
          State.initialModel() |> State.withProjects (Generate.projects 5)
          State.initialModel() |> State.withLoading LoadingProjects
        ]
        
        for model in models do
          let _, cmd = update (NavigateTo Landing) model
          Assert.isTrue (Elmish.cmdIsEmpty cmd) 
            "NavigateTo Landing should never generate commands"
      }
    ]
    
    testList "Command batching" [
      
      test "LoadProjects creates proper batch command" {
        let model = State.initialModel()
        let _, cmd = update LoadProjects model
        
        // The command should be a batch containing:
        // 1. Immediate dispatch of StartLoading
        // 2. Async command for API call
        match cmd with
        | [] -> Assert.isTrue false "Should not be empty"
        | [_] -> Assert.isTrue false "Should be a batch"
        | multiple -> 
            Assert.isTrue (multiple.Length >= 2) "Should have at least 2 commands"
      }
    ]
    
    testList "Error handling commands" [
      
      test "ProjectsLoaded Error does not generate additional commands" {
        let model = State.initialModel()
        let errors = [
          NetworkError "Timeout"
          DecodingError "Invalid JSON"
          ServerError (400, "Bad Request")
          ServerError (500, "Internal Error")
        ]
        
        for error in errors do
          let _, cmd = update (ProjectsLoaded (Error error)) model
          Assert.isTrue (Elmish.cmdIsEmpty cmd) 
            $"Error {error} should not generate commands"
      }
      
      test "ProjectsLoaded Success does not generate additional commands" {
        let model = State.initialModel()
        let projects = Generate.projects 10
        let _, cmd = update (ProjectsLoaded (Ok projects)) model
        
        Assert.isTrue (Elmish.cmdIsEmpty cmd) 
          "Success should not generate additional commands"
      }
    ]
    
    testList "Pure messages never generate commands" [
      
      test "all pure state updates return Cmd.none" {
        let model = State.initialModel()
        
        let pureMessages = [
          SetTheme Dark
          FilterByStatus (Some InProgress)
          ClearError
          StartLoading LoadingProjects
          StopLoading LoadingProjects
        ]
        
        for msg in pureMessages do
          let _, cmd = update msg model
          Assert.isTrue (Elmish.cmdIsEmpty cmd) 
            $"Message {msg} should not generate commands"
      }
    ]
    
    testList "Command message extraction" [
      
      test "extractMessages handles empty commands" {
        let messages = Elmish.extractMessages Cmd.none
        Assert.isEmpty messages "Empty command should extract no messages"
      }
      
      test "extractMessages handles single dispatch" {
        let cmd = Cmd.ofMsg LoadProjects
        let messages = Elmish.extractMessages cmd
        
        Assert.equal 1 messages.Length "Should extract one message"
        Assert.equal LoadProjects messages.[0] "Should extract correct message"
      }
      
      test "extractMessages handles batch commands" {
        let cmd = Cmd.batch [
          Cmd.ofMsg (StartLoading LoadingProjects)
          Cmd.ofMsg ClearError
          Cmd.ofMsg (SetTheme Dark)
        ]
        
        let messages = Elmish.extractMessages cmd
        Assert.equal 3 messages.Length "Should extract all messages"
        Assert.contains (StartLoading LoadingProjects) messages "Should contain StartLoading"
        Assert.contains ClearError messages "Should contain ClearError"
        Assert.contains (SetTheme Dark) messages "Should contain SetTheme"
      }
    ]
  ]