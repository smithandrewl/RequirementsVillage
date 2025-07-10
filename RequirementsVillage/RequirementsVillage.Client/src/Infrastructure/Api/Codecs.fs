module RequirementsVillage.Client.Infrastructure.Api.Codecs

open Thoth.Json
open RequirementsVillage.Shared
open System

// Decoders
let statusDecoder: Decoder<ProjectStatus> =
  Decode.string
  |> Decode.andThen (fun s ->
    match ProjectStatus.fromString s with
    | Ok status -> Decode.succeed status
    | Error msg -> Decode.fail msg
  )

let categoryDecoder: Decoder<ProjectCategory> =
  Decode.string
  |> Decode.map ProjectCategory.fromString

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
  let encoder (status: ProjectStatus) =
    ProjectStatus.toString status |> Encode.string
  let decoder = statusDecoder

module ProjectCategory =
  let encoder (category: ProjectCategory) =
    ProjectCategory.toString category |> Encode.string
  let decoder = categoryDecoder

module Project =
  let encoder (project: Project) =
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