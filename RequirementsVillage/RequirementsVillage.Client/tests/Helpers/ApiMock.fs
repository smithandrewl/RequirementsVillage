module RequirementsVillage.Client.Tests.Helpers.ApiMock

open Fable.Core
open Fable.Core.JS
open RequirementsVillage.Shared
open RequirementsVillage.Client.Infrastructure.Api.Types
open RequirementsVillage.Client.Tests.Helpers.TestData

// Mock fetch responses
module Response =
  
  type MockResponse = {
    Status:     int
    StatusText: string
    Body:       obj
  }
  
  let ok body = {
    Status     = 200
    StatusText = "OK"
    Body       = body
  }
  
  let created body = {
    Status     = 201
    StatusText = "Created"
    Body       = body
  }
  
  let notFound () = {
    Status     = 404
    StatusText = "Not Found"
    Body       = null
  }
  
  let serverError message = {
    Status     = 500
    StatusText = "Internal Server Error"
    Body       = {| message = message |}
  }
  
  let badRequest errors = {
    Status     = 400
    StatusText = "Bad Request"
    Body       = {| errors = errors |}
  }

// Mock API client
type MockApiClient() =
  
  let mutable responses : Map<string, MockResponse> = Map.empty
  let mutable callHistory : (string * obj option) list = []
  
  member _.SetResponse(endpoint: string, response: MockResponse) =
    responses <- responses |> Map.add endpoint response
  
  member _.GetCallHistory() = callHistory
  
  member _.WasCalled(endpoint: string) =
    callHistory |> List.exists (fun (e, _) -> e = endpoint)
  
  member _.GetCallCount(endpoint: string) =
    callHistory 
    |> List.filter (fun (e, _) -> e = endpoint)
    |> List.length
  
  member _.Reset() =
    responses <- Map.empty
    callHistory <- []
  
  member this.MockFetch(endpoint: string, ?body: obj) =
    // Record the call
    callHistory <- (endpoint, body) :: callHistory
    
    // Return mock response
    match responses |> Map.tryFind endpoint with
    | Some response ->
        Promise.create (fun resolve _ ->
          resolve response.Body
        )
    | None ->
        Promise.create (fun _ reject ->
          reject (System.Exception($"No mock response set for {endpoint}"))
        )

// Pre-configured mock scenarios
module Scenarios =
  
  let successfulProjectList (client: MockApiClient) =
    client.SetResponse(
      "/api/projects",
      Response.ok (Generate.projects 5)
    )
  
  let emptyProjectList (client: MockApiClient) =
    client.SetResponse(
      "/api/projects",
      Response.ok []
    )
  
  let failedProjectList (client: MockApiClient) =
    client.SetResponse(
      "/api/projects",
      Response.serverError "Database connection failed"
    )
  
  let successfulProjectCreate (client: MockApiClient) (project: Project) =
    client.SetResponse(
      "/api/projects",
      Response.created project
    )
  
  let successfulProjectUpdate (client: MockApiClient) (project: Project) =
    client.SetResponse(
      $"/api/projects/{project.Id}",
      Response.ok project
    )
  
  let projectNotFound (client: MockApiClient) (projectId: System.Guid) =
    client.SetResponse(
      $"/api/projects/{projectId}",
      Response.notFound()
    )

// Helper to replace fetch in tests
module FetchMock =
  
  let mutable private originalFetch = None
  
  let setup (mockFn: string -> obj option -> JS.Promise<obj>) =
    // Store original if not already stored
    if originalFetch.IsNone then
      originalFetch <- Some globalThis?fetch
    
    // Replace global fetch
    globalThis?fetch <- mockFn
  
  let restore () =
    match originalFetch with
    | Some original ->
        globalThis?fetch <- original
    | None -> ()
  
  let withMock (mockFn: string -> obj option -> JS.Promise<obj>) (test: unit -> unit) =
    setup mockFn
    try
      test()
    finally
      restore()