namespace RequirementsVillage.Shared.Tests.Helpers

open RequirementsVillage.Shared.TestGenerators

// Re-export shared generators for use in Shared.Tests
module Generators =
  module Bogus = TestDataGenerators.Bogus.Default
  module FsCheck = TestDataGenerators.FsCheck