module RequirementsVillage.Client.Api.Codecs

open Thoth.Json
open RequirementsVillage.Client.Models.Domain

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
