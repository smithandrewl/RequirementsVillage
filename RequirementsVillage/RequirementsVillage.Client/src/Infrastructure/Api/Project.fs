module RequirementsVillage.Client.Infrastructure.Api.Project

open Fable.Core
open Fetch
open Thoth.Json
open RequirementsVillage.Shared
open RequirementsVillage.Client.Infrastructure.Api.Codecs
open RequirementsVillage.Client.Infrastructure.Api.Types
open RequirementsVillage.Client.Configuration.Constants

let getProjects () : JS.Promise<Result<Project list, ApiError>> =
  promise {
    try
      let! response = Fetch.fetch (Api.url Api.ProjectsPath) []

      if response.Ok then
        let! text = response.text()

        match Decode.fromString (Decode.list projectDecoder) text with
        | Ok projects -> return Ok projects
        | Error err   -> return Error (DecodingError err)
      else
        let! errorText = response.text()
        return Error (ServerError (int response.Status, errorText))
    with
    | ex -> return Error (NetworkError ex.Message)
  }
