module RequirementsVillage.Client.Tests.Main

open Fable.Mocha
open RequirementsVillage.Client.Tests.State

let allTests =
  testList "Requirements Village Client Tests" [
    // Basic tests to verify setup
    test "Client tests module configuration is working" {
      Expect.isTrue true "ES modules are configured correctly"
    }
    
    // Real test suites
    TypesTests.tests
  ]

// Run tests
[<EntryPoint>]
let main args =
  Mocha.runTests allTests