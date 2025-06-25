module RequirementsVillage.FSharp.Client.Api.Projects

open Fable.Core
open Fable.Core.JsInterop
open Fetch
open Thoth.Json
open RequirementsVillage.FSharp.Client.Types


let private statusDecoder: Decoder<ProjectStatus> =
  Decode.string
  |> Decode.andThen (fun s ->
    match s with
    | "idea" -> Decode.succeed Idea
    | "inProgress" -> Decode.succeed InProgress
    | "completed" -> Decode.succeed Completed
    | "abandoned" -> Decode.succeed Abandoned
    | "onHold" -> Decode.succeed OnHold
    | _ -> Decode.fail (sprintf "Unknown status: %s" s)
  )

let private categoryDecoder: Decoder<ProjectCategory> =
  Decode.string
  |> Decode.andThen (fun s ->
    match s with
    | "webApp" -> Decode.succeed WebApp
    | "mobileApp" -> Decode.succeed MobileApp
    | "library" -> Decode.succeed Library
    | "tool" -> Decode.succeed Tool
    | "game" -> Decode.succeed Game
    | other -> Decode.succeed (Other other)
  )

let private projectDecoder: Decoder<Project> =
  Decode.object (fun get ->
    {
      Id = get.Required.Field "id" Decode.guid
      Name = get.Required.Field "name" Decode.string
      Description = get.Required.Field "description" Decode.string
      Category = get.Required.Field "category" categoryDecoder
      Status = get.Required.Field "status" statusDecoder
      CreatedAt = get.Required.Field "createdAt" Decode.datetime
      UpdatedAt = get.Required.Field "updatedAt" Decode.datetime
    }
  )

let getProjects () : JS.Promise<Result<Project list, ApiError>> =
  promise {
    try
      let! response = Fetch.fetch "/api/projects" []
      
      if response.Ok then
        let! text = response.text()
        match Decode.fromString (Decode.list projectDecoder) text with
        | Ok projects -> return Ok projects
        | Error err -> return Error (DecodingError err)
      else
        let! errorText = response.text()
        return Error (ServerError (int response.Status, errorText))
    with
    | ex -> return Error (NetworkError ex.Message)
  }