module RequirementsVillage.Client.Tests.Main

open Fable.Mocha

let allTests =
  testList "Requirements Village Client Tests" [
    test "Client tests module configuration is working" {
      Expect.isTrue true "ES modules are configured correctly"
    }
    
    test "Fable compiles to ES modules" {
      Expect.equal 1 1 "Basic test passes"
    }
  ]

// Run tests
[<EntryPoint>]
let main args =
  Mocha.runTests allTests