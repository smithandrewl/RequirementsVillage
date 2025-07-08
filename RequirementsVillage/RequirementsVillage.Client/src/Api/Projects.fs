module RequirementsVillage.Client.Api.Projects

open Fable.Core

open Thoth.Json
open RequirementsVillage.Client.Types

open Codecs

let getProjects () : JS.Promise<Result<Project list, ApiError>> =
  promise {
    try
      let! response = Fetch.fetch "/api/projects" []

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
