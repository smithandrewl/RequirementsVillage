module RequirementsVillage.FSharp.Client.Api.Projects

open Fable.Core
open Fable.Core.JsInterop
open Fetch
open Thoth.Json
open RequirementsVillage.FSharp.Client.Types


let private projectDecoder: Decoder<Project> =
    Decode.object (fun get ->
        {
            Id = get.Required.Field "id" Decode.guid
            Title = get.Required.Field "name" Decode.string
            Description = get.Required.Field "description" Decode.string
            TechStack = get.Optional.Field "techStack" (Decode.list Decode.string) |> Option.defaultValue []
            Status = 
                get.Required.Field "status" Decode.string
                |> fun s ->
                    match s.ToLower() with
                    | "in progress" -> Current
                    | "abandoned" -> Archive
                    | "someday" -> Someday
                    | _ -> Someday
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