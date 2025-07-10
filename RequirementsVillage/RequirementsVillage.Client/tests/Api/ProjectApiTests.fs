module RequirementsVillage.Client.Tests.Api.ProjectApiTests

open Fable.Mocha
open Fable.Core
open Fable.Core.JS
open Thoth.Json
open RequirementsVillage.Client.Infrastructure.Api.Project
open RequirementsVillage.Client.Infrastructure.Api.Types
open RequirementsVillage.Client.Infrastructure.Api.Codecs
open RequirementsVillage.Client.Tests.Helpers.TestHelpers
open RequirementsVillage.Client.Tests.Helpers.TestData
open RequirementsVillage.Client.Tests.Helpers.ApiMock
open RequirementsVillage.Client.Configuration.Constants

// Mock fetch implementation for testing
let createMockFetch (responses: Map<string, Response.MockResponse>) =
  fun (url: string) (_options: obj) ->
    promise {
      match responses |> Map.tryFind url with
      | Some mockResponse ->
          let response = 
            {|
              ok         = mockResponse.Status >= 200 && mockResponse.Status < 300
              status     = mockResponse.Status
              statusText = mockResponse.StatusText
              text       = fun () -> 
                promise {
                  match mockResponse.Body with
                  | null -> return ""
                  | :? string as s -> return s
                  | body ->
                      return Encode.Auto.toString(0, body)
                }
              json       = fun () ->
                promise {
                  match mockResponse.Body with
                  | null -> return null
                  | _ as body -> return body
                }
            |}
          return unbox response
      | None ->
          return failwith $"No mock response configured for URL: {url}"
    }

let tests =
  testList "Project API Tests" [
    
    testList "getProjects" [
      
      testAsync "successfully fetches project list" {
        // Arrange
        let expectedProjects = Generate.projects 3
        let projectsJson = 
          expectedProjects
          |> List.map (fun p ->
            Encode.object [
              "id", Encode.guid p.Id
              "name", Encode.string p.Name
              "description", Encode.string p.Description
              "category", Encode.string (
                match p.Category with
                | WebApp -> "webApp"
                | MobileApp -> "mobileApp"
                | Library -> "library"
                | Tool -> "tool"
                | Game -> "game"
                | Other s -> s
              )
              "status", Encode.string (
                match p.Status with
                | Idea -> "idea"
                | InProgress -> "inProgress"
                | Completed -> "completed"
                | Abandoned -> "abandoned"
                | OnHold -> "onHold"
              )
              "createdAt", Encode.datetime p.CreatedAt
              "updatedAt", Encode.datetime p.UpdatedAt
            ]
          )
          |> Encode.list
          |> Encode.toString 0
        
        let mockResponses = 
          Map.empty 
          |> Map.add (Api.url Api.ProjectsPath) (Response.ok projectsJson)
        
        let originalFetch = globalThis?fetch
        globalThis?fetch <- createMockFetch mockResponses
        
        try
          // Act
          let! result = getProjects() |> Async.AwaitPromise
          
          // Assert
          match result with
          | Ok projects ->
              Assert.equal (List.length expectedProjects) (List.length projects) 
                "Should return correct number of projects"
              
              List.iter2 (fun expected actual ->
                Assert.equal expected.Id actual.Id "Project IDs should match"
                Assert.equal expected.Name actual.Name "Project names should match"
                Assert.equal expected.Status actual.Status "Project status should match"
              ) expectedProjects projects
          | Error err ->
              Assert.isTrue false $"Expected success but got error: {err}"
        finally
          globalThis?fetch <- originalFetch
      }
      
      testAsync "handles empty project list" {
        // Arrange
        let mockResponses = 
          Map.empty 
          |> Map.add (Api.url Api.ProjectsPath) (Response.ok "[]")
        
        let originalFetch = globalThis?fetch
        globalThis?fetch <- createMockFetch mockResponses
        
        try
          // Act
          let! result = getProjects() |> Async.AwaitPromise
          
          // Assert
          match result with
          | Ok projects ->
              Assert.isEmpty projects "Should return empty list"
          | Error err ->
              Assert.isTrue false $"Expected success but got error: {err}"
        finally
          globalThis?fetch <- originalFetch
      }
      
      testAsync "handles 404 not found error" {
        // Arrange
        let mockResponses = 
          Map.empty 
          |> Map.add (Api.url Api.ProjectsPath) {
              Status     = 404
              StatusText = "Not Found"
              Body       = "Resource not found"
            }
        
        let originalFetch = globalThis?fetch
        globalThis?fetch <- createMockFetch mockResponses
        
        try
          // Act
          let! result = getProjects() |> Async.AwaitPromise
          
          // Assert
          match result with
          | Ok _ ->
              Assert.isTrue false "Expected error but got success"
          | Error (ServerError (code, msg)) ->
              Assert.equal 404 code "Should return 404 status code"
              Assert.equal "Resource not found" msg "Should return error message"
          | Error other ->
              Assert.isTrue false $"Expected ServerError but got: {other}"
        finally
          globalThis?fetch <- originalFetch
      }
      
      testAsync "handles 500 server error" {
        // Arrange
        let errorMessage = "Internal server error: Database connection failed"
        let mockResponses = 
          Map.empty 
          |> Map.add (Api.url Api.ProjectsPath) 
            (Response.serverError errorMessage)
        
        let originalFetch = globalThis?fetch
        globalThis?fetch <- createMockFetch mockResponses
        
        try
          // Act
          let! result = getProjects() |> Async.AwaitPromise
          
          // Assert
          match result with
          | Ok _ ->
              Assert.isTrue false "Expected error but got success"
          | Error (ServerError (code, msg)) ->
              Assert.equal 500 code "Should return 500 status code"
              Assert.isTrue (msg.Contains("message")) "Should contain error details"
          | Error other ->
              Assert.isTrue false $"Expected ServerError but got: {other}"
        finally
          globalThis?fetch <- originalFetch
      }
      
      testAsync "handles network errors" {
        // Arrange
        let originalFetch = globalThis?fetch
        globalThis?fetch <- fun _ _ -> 
          promise { return failwith "Network request failed" }
        
        try
          // Act
          let! result = getProjects() |> Async.AwaitPromise
          
          // Assert
          match result with
          | Ok _ ->
              Assert.isTrue false "Expected error but got success"
          | Error (NetworkError msg) ->
              Assert.isTrue (msg.Contains("Network request failed")) 
                "Should contain network error message"
          | Error other ->
              Assert.isTrue false $"Expected NetworkError but got: {other}"
        finally
          globalThis?fetch <- originalFetch
      }
      
      testAsync "handles malformed JSON response" {
        // Arrange
        let malformedJson = "{ invalid json: true"
        let mockResponses = 
          Map.empty 
          |> Map.add (Api.url Api.ProjectsPath) (Response.ok malformedJson)
        
        let originalFetch = globalThis?fetch
        globalThis?fetch <- createMockFetch mockResponses
        
        try
          // Act
          let! result = getProjects() |> Async.AwaitPromise
          
          // Assert
          match result with
          | Ok _ ->
              Assert.isTrue false "Expected error but got success"
          | Error (DecodingError msg) ->
              Assert.isTrue (msg.Length > 0) "Should contain decoding error message"
          | Error other ->
              Assert.isTrue false $"Expected DecodingError but got: {other}"
        finally
          globalThis?fetch <- originalFetch
      }
      
      testAsync "handles response with missing fields" {
        // Arrange - Project JSON missing required fields
        let incompleteJson = """
          [{
            "id": "123e4567-e89b-12d3-a456-426614174000",
            "name": "Test Project",
            "description": "Missing other fields"
          }]
        """
        let mockResponses = 
          Map.empty 
          |> Map.add (Api.url Api.ProjectsPath) (Response.ok incompleteJson)
        
        let originalFetch = globalThis?fetch
        globalThis?fetch <- createMockFetch mockResponses
        
        try
          // Act
          let! result = getProjects() |> Async.AwaitPromise
          
          // Assert
          match result with
          | Ok _ ->
              Assert.isTrue false "Expected error but got success"
          | Error (DecodingError msg) ->
              Assert.isTrue (msg.Contains("category") || msg.Contains("status")) 
                "Should indicate missing required fields"
          | Error other ->
              Assert.isTrue false $"Expected DecodingError but got: {other}"
        finally
          globalThis?fetch <- originalFetch
      }
      
      testAsync "handles response with null values" {
        // Arrange - Project JSON with null for required string field
        let jsonWithNull = """
          [{
            "id": "123e4567-e89b-12d3-a456-426614174000",
            "name": null,
            "description": "Test",
            "category": "webApp",
            "status": "idea",
            "createdAt": "2024-01-01T00:00:00Z",
            "updatedAt": "2024-01-01T00:00:00Z"
          }]
        """
        let mockResponses = 
          Map.empty 
          |> Map.add (Api.url Api.ProjectsPath) (Response.ok jsonWithNull)
        
        let originalFetch = globalThis?fetch
        globalThis?fetch <- createMockFetch mockResponses
        
        try
          // Act
          let! result = getProjects() |> Async.AwaitPromise
          
          // Assert
          match result with
          | Ok _ ->
              Assert.isTrue false "Expected error but got success"
          | Error (DecodingError msg) ->
              Assert.isTrue (msg.Contains("name") || msg.Contains("null")) 
                "Should indicate null value error"
          | Error other ->
              Assert.isTrue false $"Expected DecodingError but got: {other}"
        finally
          globalThis?fetch <- originalFetch
      }
    ]
    
    testList "API error handling" [
      
      test "NetworkError contains message" {
        let error = NetworkError "Connection failed"
        
        match error with
        | NetworkError msg ->
            Assert.equal "Connection failed" msg "Message should match"
        | _ ->
            Assert.isTrue false "Should be NetworkError"
      }
      
      test "ServerError contains code and message" {
        let error = ServerError (404, "Not found")
        
        match error with
        | ServerError (code, msg) ->
            Assert.equal 404 code "Code should be 404"
            Assert.equal "Not found" msg "Message should match"
        | _ ->
            Assert.isTrue false "Should be ServerError"
      }
      
      test "DecodingError contains message" {
        let error = DecodingError "Invalid date format"
        
        match error with
        | DecodingError msg ->
            Assert.equal "Invalid date format" msg "Message should match"
        | _ ->
            Assert.isTrue false "Should be DecodingError"
      }
      
      test "different error types are distinct" {
        let networkErr = NetworkError "net"
        let serverErr = ServerError (500, "server")
        let decodingErr = DecodingError "decode"
        
        match networkErr, serverErr, decodingErr with
        | NetworkError _, ServerError _, DecodingError _ ->
            Assert.isTrue true "All error types are distinct"
        | _ ->
            Assert.isTrue false "Error types should be distinct"
      }
    ]
    
    testList "API configuration" [
      
      test "uses correct base URL from constants" {
        let expectedUrl = Api.url Api.ProjectsPath
        Assert.isTrue (expectedUrl.Contains("/api/projects")) 
          "Should construct correct API URL"
      }
      
      test "API paths are correctly defined" {
        match Api.ProjectsPath with
        | "/api/projects" ->
            Assert.isTrue true "Projects path is correct"
        | other ->
            Assert.isTrue false $"Unexpected projects path: {other}"
      }
    ]
  ]