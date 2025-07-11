namespace RequirementsVillage.Api.Tests.Integration

open System
open System.Net.Http
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Mvc.Testing
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open FsUnit.Xunit
open RequirementsVillage.Api
open RequirementsVillage.Api.Persistence
open RequirementsVillage.Api.Services
open RequirementsVillage.Api.Tests.Helpers
open FSharp.SystemTextJson

type TestWebApplicationFactory() =
  inherit WebApplicationFactory<RequirementsVillage.Api.Program.Program>()
  
  let mutable configureTestServices: (IServiceCollection -> unit) option = None
  
  member this.WithServices(configure: IServiceCollection -> unit) =
    configureTestServices <- Some configure
    this
  
  member this.WithInMemoryRepository() =
    this.WithServices(fun services ->
      // Remove existing repository registration
      services
      |> Seq.filter (fun descriptor ->
        descriptor.ServiceType = typeof<IProjectRepository>)
      |> Seq.toList
      |> List.iter (fun descriptor ->
        services.Remove(descriptor) |> ignore)
      
      // Add in-memory repository
      services.AddSingleton<IProjectRepository>(InMemoryProjectRepository()) |> ignore
    )
  
  member this.WithMockRepository(repository: IProjectRepository) =
    this.WithServices(fun services ->
      // Remove existing repository registration
      services
      |> Seq.filter (fun descriptor ->
        descriptor.ServiceType = typeof<IProjectRepository>)
      |> Seq.toList
      |> List.iter (fun descriptor ->
        services.Remove(descriptor) |> ignore)
      
      // Add mock repository
      services.AddSingleton<IProjectRepository>(repository) |> ignore
    )
  
  override _.ConfigureWebHost(builder: IWebHostBuilder) =
    builder.ConfigureServices(fun services ->
      match configureTestServices with
      | Some configure -> configure services
      | None -> ()
    ) |> ignore

module ApiTestHelpers =
  
  open System.Text
  open System.Text.Json
  open RequirementsVillage.Shared
  open RequirementsVillage.Api.Models.Serialization
  
  let createClient (factory: TestWebApplicationFactory) =
    factory.CreateClient()
  
  let createClientWithInMemoryData (factory: TestWebApplicationFactory) =
    factory.WithInMemoryRepository().CreateClient()
  
  let createClientWithMockRepo (repository: IProjectRepository) (factory: TestWebApplicationFactory) =
    factory.WithMockRepository(repository).CreateClient()
  
  let toJson (obj: 'a) =
    JsonSerializer.Serialize(obj, jsonOptions)
  
  let fromJson<'a> (json: string) =
    JsonSerializer.Deserialize<'a>(json, jsonOptions)
  
  let createJsonContent (obj: 'a) =
    new StringContent(toJson obj, Encoding.UTF8, "application/json")
  
  let isSuccessStatusCode (response: HttpResponseMessage) =
    response.IsSuccessStatusCode
  
  let getResponseContent (response: HttpResponseMessage) =
    response.Content.ReadAsStringAsync()
    |> Async.AwaitTask
    |> TestHelpers.runAsync
  
  let getResponseJson<'a> (response: HttpResponseMessage) =
    async {
      let! content = response.Content.ReadAsStringAsync() |> Async.AwaitTask
      return fromJson<'a> content
    }
  
  // Request builders
  let get (path: string) (client: HttpClient) =
    client.GetAsync(path)
    |> Async.AwaitTask
  
  let post (path: string) (content: HttpContent) (client: HttpClient) =
    client.PostAsync(path, content)
    |> Async.AwaitTask
  
  let put (path: string) (content: HttpContent) (client: HttpClient) =
    client.PutAsync(path, content)
    |> Async.AwaitTask
  
  let patch (path: string) (content: HttpContent) (client: HttpClient) =
    client.PatchAsync(path, content)
    |> Async.AwaitTask
  
  let delete (path: string) (client: HttpClient) =
    client.DeleteAsync(path)
    |> Async.AwaitTask
  
  // Assertion helpers
  let shouldBeOk (response: HttpResponseMessage) =
    response.StatusCode |> should equal System.Net.HttpStatusCode.OK
  
  let shouldBeCreated (response: HttpResponseMessage) =
    response.StatusCode |> should equal System.Net.HttpStatusCode.Created
  
  let shouldBeNoContent (response: HttpResponseMessage) =
    response.StatusCode |> should equal System.Net.HttpStatusCode.NoContent
  
  let shouldBeBadRequest (response: HttpResponseMessage) =
    response.StatusCode |> should equal System.Net.HttpStatusCode.BadRequest
  
  let shouldBeNotFound (response: HttpResponseMessage) =
    response.StatusCode |> should equal System.Net.HttpStatusCode.NotFound
  
  let shouldBeInternalServerError (response: HttpResponseMessage) =
    response.StatusCode |> should equal System.Net.HttpStatusCode.InternalServerError
  
  // Project-specific helpers
  type CreateProjectRequest = {
    Name:        string
    Description: string
    Category:    string
  }
  
  type UpdateStatusRequest = {
    Status: string
  }
  
  let createProjectRequest name description category = {
    Name        = name
    Description = description
    Category    = category
  }
  
  let updateStatusRequest status = {
    Status = status
  }
  
  let projectToRequest (project: Project) = {
    Name        = project.Name
    Description = project.Description
    Category    = 
      match project.Category with
      | WebApp    -> "webApp"
      | MobileApp -> "mobileApp"
      | Library   -> "library"
      | Tool      -> "tool"
      | Game      -> "game"
      | Other s   -> s
  }