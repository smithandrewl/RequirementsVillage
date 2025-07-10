module RequirementsVillage.Client.Infrastructure.Api.Codecs

open Thoth.Json
open RequirementsVillage.Client.Domain.Project

// Decoders
let statusDecoder: Decoder<ProjectStatus> =
  Decode.string
  |> Decode.andThen (fun s ->
    match s with
    | "idea"       -> Decode.succeed Idea
    | "inProgress" -> Decode.succeed InProgress
    | "completed"  -> Decode.succeed Completed
    | "abandoned"  -> Decode.succeed Abandoned
    | "onHold"     -> Decode.succeed OnHold
    | _            -> Decode.fail (sprintf "Unknown status: %s" s)
  )

let categoryDecoder: Decoder<ProjectCategory> =
  Decode.string
  |> Decode.andThen (fun s ->
    match s with
    | "webApp"    -> Decode.succeed WebApp
    | "mobileApp" -> Decode.succeed MobileApp
    | "library"   -> Decode.succeed Library
    | "tool"      -> Decode.succeed Tool
    | "game"      -> Decode.succeed Game
    | other       -> Decode.succeed (Other other)
  )

let projectDecoder: Decoder<Project> =
  Decode.object (fun get ->
    {
      Id          = get.Required.Field "id"          Decode.guid
      Name        = get.Required.Field "name"        Decode.string
      Description = get.Required.Field "description" Decode.string
      Category    = get.Required.Field "category"    categoryDecoder
      Status      = get.Required.Field "status"      statusDecoder
      CreatedAt   = get.Required.Field "createdAt"   Decode.datetimeUtc
      UpdatedAt   = get.Required.Field "updatedAt"   Decode.datetimeUtc
    }
  )

// Encoders
module ProjectStatus =
  let encoder (status: ProjectStatus) : Encoder<obj> =
    match status with
    | Idea       -> Encode.string "Idea"
    | InProgress -> Encode.string "InProgress"
    | Completed  -> Encode.string "Completed"
    | Abandoned  -> Encode.string "Abandoned"
    | OnHold     -> Encode.string "OnHold"
  
  let decoder = statusDecoder

module ProjectCategory =
  let encoder (category: ProjectCategory) : Encoder<obj> =
    match category with
    | WebApp    -> Encode.string "WebApp"
    | MobileApp -> Encode.string "MobileApp"
    | Library   -> Encode.string "Library"
    | Tool      -> Encode.string "Tool"
    | Game      -> Encode.string "Game"
    | Other s   -> Encode.string $"Other:{s}"
  
  let decoder = categoryDecoder

module Project =
  let encoder (project: Project) : Encoder<obj> =
    Encode.object [
      "id",          Encode.guid project.Id
      "name",        Encode.string project.Name
      "description", Encode.string project.Description
      "category",    ProjectCategory.encoder project.Category
      "status",      ProjectStatus.encoder project.Status
      "createdAt",   Encode.datetime project.CreatedAt
      "updatedAt",   Encode.datetime project.UpdatedAt
    ]
  
  let decoder = projectDecoder
