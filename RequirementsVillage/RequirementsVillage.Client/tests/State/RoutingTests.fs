module RequirementsVillage.Client.Tests.State.RoutingTests

open Fable.Mocha
open Elmish
open RequirementsVillage.Client.Presentation.State.Types
open RequirementsVillage.Client.Presentation.State.Update
open RequirementsVillage.Client.Routes
open RequirementsVillage.Client.Tests.Helpers.TestHelpers
open RequirementsVillage.Client.Tests.Helpers.TestData

let tests =
  testList "Routing Tests" [
    
    testList "Route parsing" [
      
      test "parses empty path as Landing" {
        let route = parseUrl []
        Assert.equal Landing route "Empty path should be Landing"
      }
      
      test "parses dashboard path correctly" {
        let route = parseUrl ["dashboard"]
        Assert.equal Dashboard route "dashboard path should be Dashboard"
      }
      
      test "parses unknown paths as Landing" {
        let testCases = [
          ["unknown"]
          ["projects"; "123"]
          ["admin"; "settings"]
          ["foo"; "bar"; "baz"]
        ]
        
        for path in testCases do
          let route = parseUrl path
          Assert.equal Landing route $"Path {path} should default to Landing"
      }
    ]
    
    testList "URL generation" [
      
      test "generates correct URL segments for Landing" {
        let segments = toUrlSegments Landing
        Assert.equal [] segments "Landing should have no segments"
      }
      
      test "generates correct URL segments for Dashboard" {
        let segments = toUrlSegments Dashboard
        Assert.equal ["dashboard"] segments "Dashboard should have dashboard segment"
      }
    ]
    
    testList "Navigation commands" [
      
      test "NavigateTo generates navigation command" {
        let model = State.initialModel()
        let _, cmd = update (NavigateTo Dashboard) model
        
        // The command should contain both navigation and potentially LoadProjects
        let hasNavCmd = 
          cmd 
          |> Elmish.cmdToList 
          |> List.exists (fun subcmd ->
            // Check if this is a navigation command
            match subcmd with
            | Elmish.Cmd.OfFunc _ -> true
            | _ -> false
          )
        
        Assert.isTrue hasNavCmd "Should have navigation command"
      }
      
      test "NavigateTo updates current page immediately" {
        let model = State.initialModel()
        let newModel, _ = update (NavigateTo Dashboard) model
        
        Assert.equal Dashboard newModel.UI.CurrentPage "Page should update immediately"
      }
    ]
    
    testList "UrlChanged message" [
      
      test "UrlChanged updates current page" {
        let model = State.initialModel()
        let newModel, _ = update (UrlChanged Dashboard) model
        
        Assert.equal Dashboard newModel.UI.CurrentPage "Page should update from URL change"
      }
      
      test "UrlChanged to Dashboard with no projects triggers load" {
        let model = State.initialModel()
        let _, cmd = update (UrlChanged Dashboard) model
        
        Assert.isTrue 
          (Elmish.cmdContainsMessage LoadProjects cmd)
          "Should trigger LoadProjects when navigating to Dashboard via URL"
      }
      
      test "UrlChanged to Dashboard with existing projects does not load" {
        let model = 
          State.initialModel() 
          |> State.withProjects (Generate.projects 3)
        
        let _, cmd = update (UrlChanged Dashboard) model
        
        Assert.isFalse
          (Elmish.cmdContainsMessage LoadProjects cmd)
          "Should not load projects when they already exist"
      }
      
      test "UrlChanged to Landing never triggers commands" {
        let model = 
          State.initialModel()
          |> State.withPage Dashboard
        
        let _, cmd = update (UrlChanged Landing) model
        
        Assert.isTrue (Elmish.cmdIsEmpty cmd) "Landing should not trigger commands"
      }
    ]
    
    testList "Navigation behavior" [
      
      test "NavigateTo and UrlChanged have consistent behavior for Dashboard" {
        let model1 = State.initialModel()
        let model2 = State.initialModel()
        
        let newModel1, cmd1 = update (NavigateTo Dashboard) model1
        let newModel2, cmd2 = update (UrlChanged Dashboard) model2
        
        // Both should update the page
        Assert.equal Dashboard newModel1.UI.CurrentPage "NavigateTo should set page"
        Assert.equal Dashboard newModel2.UI.CurrentPage "UrlChanged should set page"
        
        // Both should trigger LoadProjects when no projects exist
        Assert.isTrue 
          (Elmish.cmdContainsMessage LoadProjects cmd1)
          "NavigateTo should load projects"
        Assert.isTrue 
          (Elmish.cmdContainsMessage LoadProjects cmd2)
          "UrlChanged should load projects"
      }
      
      test "Navigation preserves other UI state" {
        let model = 
          State.initialModel()
          |> State.withTheme Dark
          |> State.withFilter (Some InProgress)
          |> State.withError "Test error"
        
        let newModel, _ = update (NavigateTo Dashboard) model
        
        Assert.equal Dark newModel.UI.CurrentTheme "Theme should be preserved"
        Assert.equal (Some InProgress) newModel.UI.FilteredStatus "Filter should be preserved"
        Assert.equal (Some "Test error") newModel.UI.Error "Error should be preserved"
      }
    ]
    
    testList "Route roundtrip" [
      
      test "route to URL to route preserves identity" {
        let routes = [Landing; Dashboard]
        
        for route in routes do
          let segments = toUrlSegments route
          let parsed = parseUrl segments
          Assert.equal route parsed $"Route {route} should roundtrip correctly"
      }
    ]
    
    testList "Navigation edge cases" [
      
      test "rapid navigation updates page correctly" {
        let model = State.initialModel()
        
        // Simulate rapid navigation
        let model1, _ = update (NavigateTo Dashboard) model
        let model2, _ = update (NavigateTo Landing) model1
        let model3, _ = update (NavigateTo Dashboard) model2
        
        Assert.equal Dashboard model3.UI.CurrentPage "Final page should be Dashboard"
      }
      
      test "navigating to current page is idempotent" {
        let model = 
          State.initialModel()
          |> State.withPage Dashboard
          |> State.withProjects (Generate.projects 3)
        
        let newModel, cmd = update (NavigateTo Dashboard) model
        
        Assert.equal model.UI.CurrentPage newModel.UI.CurrentPage "Page should not change"
        Assert.isTrue (Elmish.cmdIsEmpty cmd) "Should not trigger commands when already on page with data"
      }
    ]
  ]