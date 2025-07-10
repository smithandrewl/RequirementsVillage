module RequirementsVillage.Shared.Codecs

open Thoth.Json.Net
open RequirementsVillage.Shared

// Decoders
module Decode =
  let projectStatus: Decoder<ProjectStatus> =
    Decode.string
    |> Decode.andThen (fun s ->
      match ProjectStatus.fromString s with
      | Ok status -> Decode.succeed status
      | Error msg -> Decode.fail msg
    )

  let projectCategory: Decoder<ProjectCategory> =
    Decode.string
    |> Decode.map ProjectCategory.fromString

  let project: Decoder<Project> =
    Decode.object (fun get ->
      {
        Id          = get.Required.Field "id"          Decode.guid
        Name        = get.Required.Field "name"        Decode.string
        Description = get.Required.Field "description" Decode.string
        Category    = get.Required.Field "category"    projectCategory
        Status      = get.Required.Field "status"      projectStatus
        CreatedAt   = get.Required.Field "createdAt"   Decode.datetimeUtc
        UpdatedAt   = get.Required.Field "updatedAt"   Decode.datetimeUtc
      }
    )

  let projectError: Decoder<ProjectError> =
    Decode.field "type" Decode.string
    |> Decode.andThen (fun errorType ->
      match errorType with
      | "NotFound" ->
        Decode.map2 (fun id ctx -> NotFound(id, ctx))
          (Decode.field "projectId" Decode.guid)
          (Decode.field "searchContext" Decode.string)
      
      | "ValidationFailed" ->
        Decode.map3 (fun field reason value -> 
          ValidationFailed(field, reason, value :> obj))
          (Decode.field "field" Decode.string)
          (Decode.field "reason" Decode.string)
          (Decode.field "attemptedValue" Decode.value)
      
      | "DatabaseError" ->
        Decode.map3 (fun op table msg -> 
          DatabaseError(op, table, exn msg))
          (Decode.field "operation" Decode.string)
          (Decode.field "tableName" Decode.string)
          (Decode.field "error" Decode.string)
      
      | "UnknownError" ->
        Decode.map UnknownError
          (Decode.field "message" Decode.string)
      
      | _ -> Decode.fail (sprintf "Unknown error type: %s" errorType)
    )

// Encoders
module Encode =
  let projectStatus (status: ProjectStatus) : JsonValue =
    Encode.string (ProjectStatus.toString status)

  let projectCategory (category: ProjectCategory) : JsonValue =
    Encode.string (ProjectCategory.toString category)

  let project (project: Project) : JsonValue =
    Encode.object [
      "id",          Encode.guid project.Id
      "name",        Encode.string project.Name
      "description", Encode.string project.Description
      "category",    projectCategory project.Category
      "status",      projectStatus project.Status
      "createdAt",   Encode.datetime project.CreatedAt
      "updatedAt",   Encode.datetime project.UpdatedAt
    ]

  let projectError (error: ProjectError) : JsonValue =
    match error with
    | NotFound (id, ctx) ->
      Encode.object [
        "type", Encode.string "NotFound"
        "projectId", Encode.guid id
        "searchContext", Encode.string ctx
      ]
    
    | ValidationFailed (field, reason, value) ->
      Encode.object [
        "type", Encode.string "ValidationFailed"
        "field", Encode.string field
        "reason", Encode.string reason
        "attemptedValue", Encode.Auto.toString(0, value)
      ]
    
    | DatabaseError (op, table, ex) ->
      Encode.object [
        "type", Encode.string "DatabaseError"
        "operation", Encode.string op
        "tableName", Encode.string table
        "error", Encode.string ex.Message
      ]
    
    | UnknownError msg ->
      Encode.object [
        "type", Encode.string "UnknownError"
        "message", Encode.string msg
      ]