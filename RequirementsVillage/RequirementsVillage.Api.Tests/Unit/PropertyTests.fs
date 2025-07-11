namespace RequirementsVillage.Api.Tests.Unit

open System
open Xunit
open FsUnit.Xunit
open FsCheck
open FsCheck.Xunit
open RequirementsVillage.Shared
open RequirementsVillage.Shared.Tests.TestGenerators

/// Property-based tests for domain models
module PropertyTests =
  
  module ModelInvariants =
    
    [<Property>]
    let ``UpdatedAt should always be greater than or equal to CreatedAt`` () =
      // Register our custom generators before running the property test
      TestDataGenerators.FsCheck.registerGenerators()
      
      Prop.forAll (TestDataGenerators.FsCheck.Generators.Project()) (fun project ->
        project.UpdatedAt >= project.CreatedAt
      )