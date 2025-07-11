namespace RequirementsVillage.Api.Tests.Unit.Repository

open System
open Xunit
open FsUnit.Xunit
open RequirementsVillage.Shared
open RequirementsVillage.Api.Persistence
open RequirementsVillage.Api.Tests.Helpers
open RequirementsVillage.Shared.Tests.TestGenerators

/// Unit tests for InMemoryProjectRepository
module InMemoryProjectRepositoryTests =
  
  module CreateOperations =
    
    [<Fact>]
    let ``CreateAsync should add project to repository`` () =
      async {
        let repository = InMemoryProjectRepository() :> IProjectRepository
        let project = TestDataGenerators.Bogus.Default.project()
        
        let! result = repository.CreateAsync(project)
        match result with
        | Ok () -> ()
        | Error err -> failwithf "Expected Ok but got Error: %A" err
        
        let! getResult = repository.GetByIdAsync(project.Id)
        match getResult with
        | Ok (Some retrieved) -> 
          retrieved |> should equal project
        | _ -> 
          failwith "Project should exist after creation"
      } |> TestHelpers.runAsync