namespace RequirementsVillage.Api.Tests.Helpers

open RequirementsVillage.Shared
open RequirementsVillage.Shared.TestGenerators

// Re-export shared generators for backward compatibility
module Generators =
  
  // Use shared test data generators
  module Bogus = TestDataGenerators.Bogus.Default
  module FsCheck = TestDataGenerators.FsCheck