module RequirementsVillage.Client.Tests.State.TypesTests

open Fable.Mocha
open RequirementsVillage.Client.Tests.Helpers.TestHelpers
open RequirementsVillage.Client.Tests.Helpers.TestData
open RequirementsVillage.Shared
open RequirementsVillage.Client.Routes

let tests = 
  testList "Domain Model Tests" [
    
    testList "Project Status" [
      
      test "can create all status values" {
        let statuses = [Idea; InProgress; Completed; Abandoned; OnHold]
        Assert.equal 5 statuses.Length "Should have 5 status values"
      }
      
      test "status string conversion round trips" {
        let statuses = [Idea; InProgress; Completed; Abandoned; OnHold]
        
        for status in statuses do
          let str = ProjectStatus.toString status
          match ProjectStatus.fromString str with
          | Ok parsed -> 
              Assert.equal status parsed $"Status {status} should round trip"
          | Error msg ->
              Assert.isTrue false $"Failed to parse status: {msg}"
      }
    ]
    
    testList "Project Category" [
      
      test "can create all category values" {
        let categories = [WebApp; MobileApp; Library; Tool; Game; Other "custom"]
        Assert.equal 6 categories.Length "Should have 6 category values"
      }
      
      test "category string conversion round trips" {
        let categories = [WebApp; MobileApp; Library; Tool; Game]
        
        for category in categories do
          let str = ProjectCategory.toString category
          let parsed = ProjectCategory.fromString str
          Assert.equal category parsed $"Category {category} should round trip"
      }
      
      test "Other category with empty string converts to 'other'" {
        let category = Other ""
        let str = ProjectCategory.toString category
        Assert.equal "other" str "Empty Other should convert to 'other'"
        
        let parsed = ProjectCategory.fromString "other"
        Assert.equal (Other "") parsed "Should parse 'other' to Other with empty string"
      }
    ]
    
    testList "Project" [
      
      test "can create a project with all fields" {
        let project = Sample.testProject1
        Assert.equal "Test Project 1" project.Name "Name should match"
        Assert.equal WebApp project.Category "Category should match"
        Assert.equal InProgress project.Status "Status should match"
      }
      
      test "generated projects have unique IDs" {
        let projects = Generate.projects 10
        let ids = projects |> List.map (fun p -> p.Id)
        let uniqueIds = ids |> List.distinct
        Assert.equal ids.Length uniqueIds.Length "All project IDs should be unique"
      }
      
      test "generated projects have valid dates" {
        let projects = Generate.projects 5
        
        for project in projects do
          Assert.isTrue (project.CreatedAt <= project.UpdatedAt) 
            "CreatedAt should be <= UpdatedAt"
          Assert.isTrue (project.CreatedAt < System.DateTime.UtcNow)
            "CreatedAt should be in the past"
      }
    ]
    
    testList "Routing" [
      
      test "parseUrl correctly parses root path" {
        let route = parseUrl []
        Assert.equal Landing route "Empty segments should parse to Landing"
      }
      
      test "parseUrl correctly parses dashboard path" {
        let route = parseUrl ["dashboard"]
        Assert.equal Dashboard route "dashboard segment should parse to Dashboard"
      }
      
      test "parseUrl defaults unknown paths to Landing" {
        let unknownPaths = [
          ["unknown"]
          ["admin"]
          ["projects"; "123"]
          ["dashboard"; "extra"]
        ]
        
        for path in unknownPaths do
          let route = parseUrl path
          Assert.equal Landing route $"Unknown path {path} should default to Landing"
      }
      
      test "toUrlSegments generates correct segments" {
        let cases = [
          Landing, []
          Dashboard, ["dashboard"]
        ]
        
        for route, expectedSegments in cases do
          let segments = toUrlSegments route
          Assert.equal expectedSegments segments $"Route {route} should generate correct segments"
      }
      
      test "route roundtrip preserves identity" {
        let routes = [Landing; Dashboard]
        
        for route in routes do
          let segments = toUrlSegments route
          let parsed = parseUrl segments
          Assert.equal route parsed $"Route {route} should roundtrip correctly"
      }
    ]
  ]