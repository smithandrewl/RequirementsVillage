module RequirementsVillage.Client.Tests.Helpers.TestHelpers

open Fable.Mocha

// Test assertion helpers
module Assert =
  
  let inline equal expected actual message =
    Expect.equal actual expected message
  
  let inline notEqual expected actual message =
    Expect.notEqual actual expected message
  
  let inline isTrue value message =
    Expect.isTrue value message
  
  let inline isFalse value message =
    Expect.isFalse value message
  
  let inline isEmpty list message =
    Expect.isEmpty list message
  
  let inline isNone option message =
    Expect.isNone option message
  
  let inline isSome option message =
    Expect.isSome option message