module OxpeckerOpenApi.BugTest.Types

[<CLIMutable>]
type PersonSearchRequest = { Name: string option; Occupation: string option }

[<CLIMutable>]
type Person = { Name: string; Age: int; Occupation: string option; YearsExperience: int option }