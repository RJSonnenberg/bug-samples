// Standalone Test Runner for F# Union Type Serialization
// Run this file with: dotnet fsi UnionSerializationTestRunner.fsx

#r "nuget: FSharp.SystemTextJson, 1.3.13"

open System
open System.Text.Json
open System.Text.Json.Serialization

// Define simple F# union types for testing

/// Simple union with cases that have no data
type SimpleStatus =
    | Active
    | Inactive
    | Pending

/// Union with cases that have associated data
type Shape =
    | Circle of radius: float
    | Rectangle of width: float * height: float
    | Triangle of base_: float * height: float

/// Union with mixed cases (some with data, some without)
type PaymentMethod =
    | Cash
    | CreditCard of cardNumber: string * expiryDate: DateTime
    | BankTransfer of accountNumber: string
    | PayPal of email: string

/// Nested union types
type Result<'T, 'E> =
    | Success of 'T
    | Failure of 'E

type ApiResponse =
    | UserData of name: string * age: int
    | ErrorMessage of code: int * message: string

/// Union type with record types
type Address =
    { Street: string
      City: string
      ZipCode: string }

type ContactInfo =
    | Email of string
    | Phone of string
    | MailingAddress of Address

/// Test helper functions

let createJsonOptions () =
    let options = JsonSerializerOptions()
    options.PropertyNamingPolicy <- JsonNamingPolicy.CamelCase
    options.WriteIndented <- true
    options.Converters.Add(
        JsonFSharpConverter(JsonFSharpOptions().WithTypes(JsonFSharpTypes.Minimal))
    )
    options

let serializeToJson<'T> (value: 'T) =
    let options = createJsonOptions()
    JsonSerializer.Serialize(value, options)

let deserializeFromJson<'T> (json: string) =
    let options = createJsonOptions()
    JsonSerializer.Deserialize<'T>(json, options)

let roundTripTest<'T when 'T: equality> (value: 'T) (testName: string) =
    try
        printfn "\n=== Testing %s ===" testName
        printfn "Original value: %A" value
        
        let json = serializeToJson value
        printfn "Serialized JSON:\n%s" json
        
        let deserialized = deserializeFromJson<'T> json
        printfn "Deserialized value: %A" deserialized
        
        if value = deserialized then
            printfn "✓ Round-trip test PASSED"
            true
        else
            printfn "✗ Round-trip test FAILED: Values don't match"
            false
    with ex ->
        printfn "✗ Round-trip test FAILED with exception: %s" ex.Message
        printfn "Stack trace: %s" ex.StackTrace
        false

let runAllTests () =
    printfn "================================"
    printfn "F# Union Type Serialization Tests"
    printfn "Using FSharp.SystemTextJson"
    printfn "================================"
    
    let mutable allPassed = true
    
    // Test 1: Simple status union
    allPassed <- roundTripTest Active "SimpleStatus.Active" && allPassed
    allPassed <- roundTripTest Inactive "SimpleStatus.Inactive" && allPassed
    allPassed <- roundTripTest Pending "SimpleStatus.Pending" && allPassed
    
    // Test 2: Shape union with data
    allPassed <- roundTripTest (Circle 5.0) "Shape.Circle" && allPassed
    allPassed <- roundTripTest (Rectangle(10.0, 20.0)) "Shape.Rectangle" && allPassed
    allPassed <- roundTripTest (Triangle(8.0, 6.0)) "Shape.Triangle" && allPassed
    
    // Test 3: Payment method union
    allPassed <- roundTripTest Cash "PaymentMethod.Cash" && allPassed
    allPassed <- roundTripTest (CreditCard("1234-5678-9012-3456", DateTime(2025, 12, 31))) "PaymentMethod.CreditCard" && allPassed
    allPassed <- roundTripTest (BankTransfer("ACC123456")) "PaymentMethod.BankTransfer" && allPassed
    allPassed <- roundTripTest (PayPal("user@example.com")) "PaymentMethod.PayPal" && allPassed
    
    // Test 4: Result type
    allPassed <- roundTripTest (Success 42) "Result<int, string>.Success" && allPassed
    allPassed <- roundTripTest (Failure "Error occurred") "Result<int, string>.Failure" && allPassed
    
    // Test 5: API Response
    allPassed <- roundTripTest (UserData("John Doe", 30)) "ApiResponse.UserData" && allPassed
    allPassed <- roundTripTest (ErrorMessage(404, "Not Found")) "ApiResponse.ErrorMessage" && allPassed
    
    // Test 6: Contact Info with nested record
    let address = { Street = "123 Main St"; City = "Springfield"; ZipCode = "12345" }
    allPassed <- roundTripTest (Email "test@example.com") "ContactInfo.Email" && allPassed
    allPassed <- roundTripTest (Phone "+1-555-0123") "ContactInfo.Phone" && allPassed
    allPassed <- roundTripTest (MailingAddress address) "ContactInfo.MailingAddress" && allPassed
    
    // Test 7: List of unions
    let shapes = [ Circle 3.0; Rectangle(4.0, 5.0); Triangle(6.0, 7.0) ]
    allPassed <- roundTripTest shapes "List<Shape>" && allPassed
    
    // Test 8: Option with union
    let optionalShape = Some (Circle 10.0)
    allPassed <- roundTripTest optionalShape "Option<Shape>.Some" && allPassed
    let noneShape: Shape option = None
    allPassed <- roundTripTest noneShape "Option<Shape>.None" && allPassed
    
    printfn "\n================================"
    if allPassed then
        printfn "✓ ALL TESTS PASSED"
    else
        printfn "✗ SOME TESTS FAILED"
    printfn "================================"
    
    allPassed

// Run the tests
runAllTests() |> ignore

printfn "\n\nPress any key to exit..."
Console.ReadKey() |> ignore
