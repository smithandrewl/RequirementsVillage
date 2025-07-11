module RequirementsVillage.Api.Endpoints

open System
open Microsoft.AspNetCore.Http
open Giraffe
open FSharp.Control.Tasks
open RequirementsVillage.Shared
open RequirementsVillage.Api.Services

// Request/Response DTOs
type CreateProjectRequest = {
  Name:        string
  Description: string
  Category:    string
}

type UpdateProjectRequest = {
  Name:        string
  Description: string
  Category:    string
  Status:      string
}

type UpdateStatusRequest = {
  Status: string
}

// Helper functions for error handling
module ErrorHandlers =
  let handleProjectError (error: ProjectError) : HttpHandler =
    match error with
    | NotFound (id, context) ->
      RequestErrors.NOT_FOUND (
        json {|
          error = sprintf "Project %A not found in %s" id context
        |}
      )
    | ValidationFailed (field, reason, value) ->
      RequestErrors.BAD_REQUEST (
        json {|
          error = sprintf "%s validation failed: %s" field reason
          field = field
          value = value
        |}
      )
    | DatabaseError (operation, table, ex) ->
      ServerErrors.INTERNAL_ERROR (
        json {|
          error = sprintf "Database error in %s on %s"
            operation table
        |}
      )
    | UnknownError message ->
      ServerErrors.INTERNAL_ERROR (json {| error = message |})

// Project endpoints
module ProjectEndpoints =
  open ErrorHandlers

  // Parse category from string
  let parseCategory (str: string) : ProjectCategory =
    match str.ToLowerInvariant() with
    | "webapp"    -> WebApp
    | "mobileapp" -> MobileApp
    | "library"   -> Library
    | "tool"      -> Tool
    | "game"      -> Game
    | other       -> Other other

  // Parse status from string
  let parseStatus (str: string) : Result<ProjectStatus, string> =
    match str.ToLowerInvariant() with
    | "idea"       -> Ok Idea
    | "inprogress" -> Ok InProgress
    | "completed"  -> Ok Completed
    | "abandoned"  -> Ok Abandoned
    | "onhold"     -> Ok OnHold
    | _            -> Error (sprintf "Invalid status: %s" str)

  let getProjects : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
      task {
        let service = ctx.GetService<IProjectService>()
        let! result =
          service.GetAllProjectsAsync() |> Async.StartAsTask

        match result with
        | Ok projects ->
          return! json projects next ctx
        | Error error ->
          return! handleProjectError error next ctx
      }

  let getProject (id: string) : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
      task {
        match Guid.TryParse(id) with
        | true, guid ->
          let service = ctx.GetService<IProjectService>()
          let! result =
            service.GetProjectByIdAsync(guid) |> Async.StartAsTask

          match result with
          | Ok (Some project) ->
            return! json project next ctx
          | Ok None ->
            return! RequestErrors.NOT_FOUND (
              json {| error = sprintf "Project %s not found" id |}
            ) next ctx
          | Error error ->
            return! handleProjectError error next ctx
        | false, _ ->
          return! RequestErrors.BAD_REQUEST (
            json {| error = "Invalid project ID format" |}
          ) next ctx
      }

  let createProject : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
      task {
        try
          let! request = ctx.BindJsonAsync<CreateProjectRequest>()

          // Handle null values from JSON deserialization
          let name = if isNull request.Name then "" else request.Name
          let description = if isNull request.Description then "" else request.Description
          let categoryStr = if isNull request.Category then "" else request.Category

          let service  = ctx.GetService<IProjectService>()
          let category = parseCategory categoryStr

          let! result =
            service.CreateProjectAsync(name, description, category)
            |> Async.StartAsTask

          match result with
          | Ok project ->
            ctx.SetStatusCode 201
            ctx.SetHttpHeader("Location", sprintf "/api/projects/%A" project.Id)
            return! json project next ctx
          | Error error ->
            return! handleProjectError error next ctx
        with
        | :? System.Text.Json.JsonException ->
          return! RequestErrors.BAD_REQUEST (
            json {| error = "Invalid JSON format" |}
          ) next ctx
        | :? System.NullReferenceException ->
          return! RequestErrors.BAD_REQUEST (
            json {| error = "Missing required fields" |}
          ) next ctx
        | ex ->
          // Log the actual exception for debugging
          printfn "Unexpected error in createProject: %A" ex
          return! ServerErrors.INTERNAL_ERROR (
            json {| error = "An unexpected error occurred" |}
          ) next ctx
      }

  let updateProject (id: string) : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
      task {
        match Guid.TryParse(id) with
        | true, guid ->
          let! request = ctx.BindJsonAsync<UpdateProjectRequest>()

          // Handle null values from JSON deserialization
          let name = if isNull request.Name then "" else request.Name
          let description = if isNull request.Description then "" else request.Description
          let categoryStr = if isNull request.Category then "" else request.Category
          let statusStr = if isNull request.Status then "" else request.Status

          let service = ctx.GetService<IProjectService>()

          match parseStatus statusStr with
          | Ok status ->
            let project = {
              Id          = guid
              Name        = name
              Description = description
              Category    = parseCategory categoryStr
              Status      = status
              CreatedAt   = DateTime.MinValue // Ignored by service
              UpdatedAt   = DateTime.UtcNow
            }

            let! result =
              service.UpdateProjectAsync(project)
              |> Async.StartAsTask

            match result with
            | Ok () ->
              return! Successful.NO_CONTENT next ctx
            | Error error ->
              return! handleProjectError error next ctx
          | Error msg ->
            return! RequestErrors.BAD_REQUEST (
              json {| error = msg |}
            ) next ctx
        | false, _ ->
          return! RequestErrors.BAD_REQUEST (
            json {| error = "Invalid project ID format" |}
          ) next ctx
      }

  let updateProjectStatus (id: string) : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
      task {
        match Guid.TryParse(id) with
        | true, guid ->
          let! request = ctx.BindJsonAsync<UpdateStatusRequest>()

          // Handle null values from JSON deserialization
          let statusStr = if isNull request.Status then "" else request.Status

          let service = ctx.GetService<IProjectService>()

          match parseStatus statusStr with
          | Ok status ->
            let! result =
              service.UpdateProjectStatusAsync(guid, status)
              |> Async.StartAsTask

            match result with
            | Ok () ->
              return! Successful.NO_CONTENT next ctx
            | Error error ->
              return! handleProjectError error next ctx
          | Error msg ->
            return! RequestErrors.BAD_REQUEST (
              json {| error = msg |}
            ) next ctx
        | false, _ ->
          return! RequestErrors.BAD_REQUEST (
            json {| error = "Invalid project ID format" |}
          ) next ctx
      }

  let deleteProject (id: string) : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
      task {
        match Guid.TryParse(id) with
        | true, guid ->
          let service = ctx.GetService<IProjectService>()
          let! result =
            service.DeleteProjectAsync(guid) |> Async.StartAsTask

          match result with
          | Ok () ->
            return! Successful.NO_CONTENT next ctx
          | Error error ->
            return! handleProjectError error next ctx
        | false, _ ->
          return! RequestErrors.BAD_REQUEST (
            json {| error = "Invalid project ID format" |}
          ) next ctx
      }

// Health check endpoint
let healthCheck : HttpHandler =
  fun (next: HttpFunc) (ctx: HttpContext) ->
    let response = {|
      status    = "healthy"
      timestamp = DateTime.UtcNow.ToString("O")
    |}
    json response next ctx

// Main router
let apiRouter : HttpHandler =
  choose [
    GET_HEAD >=> route "/api/health" >=> healthCheck
    subRoute "/api/projects" (
      choose [
        GET >=> choose [
          route "" >=> ProjectEndpoints.getProjects
          routef "/%s" ProjectEndpoints.getProject
        ]
        POST >=> route "" >=> ProjectEndpoints.createProject
        PUT >=> routef "/%s" ProjectEndpoints.updateProject
        PATCH >=>
          routef "/%s/status" ProjectEndpoints.updateProjectStatus
        DELETE >=> routef "/%s" ProjectEndpoints.deleteProject
      ]
    )
    RequestErrors.NOT_FOUND "Not Found"
  ]
