namespace RequirementsVillage.Api.Tests.Unit.Services

open Xunit
open FsUnit.Xunit
open RequirementsVillage.Shared
open RequirementsVillage.Api.Services
open RequirementsVillage.Api.Tests.Helpers

/// Validation tests for ProjectService
module ProjectServiceValidationTests =
  
  module CreateProjectValidation =
    
    [<Fact>]
    let ``CreateProject should fail with empty name`` () =
      async {
        let mockRepo = Fixtures.Mocks.ConfigurableMockRepository()
        let service = Fixtures.ServiceFactories.createProjectService mockRepo
        let! result = service.CreateProjectAsync("", "Valid description", Tool)
        
        match result with
        | Error (ValidationFailed ("name", reason, _)) ->
          reason |> should haveSubstring "empty"
        | _ -> 
          failwith "Expected ValidationFailed error for name"
      } |> TestHelpers.runAsync