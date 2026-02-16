module OxpeckerOpenApi.BugTest.Types

open System.ComponentModel.DataAnnotations

[<CLIMutable>]
type PersonSearchRequest =
    { Name: string option
      Occupation: string option }

[<CLIMutable>]
type Person =
    { Name: string
      Age: int
      Occupation: string option
      YearsExperience: int option }

[<CLIMutable>]
type AnimalSearchRequest =
    { Name: string option
      Species: string option
      Vaccinated: bool option }

type AnimalColor =
    | Brown
    | Black
    | White
    | Spotted

[<CLIMutable>]
type Animal =
    {
        // These attributes will be used by the OpenAPI schema generator to add the fields to the shema "required" list.
        // Even though the field is "required", it will still show as nullable unless we also add the <Nullable>
        // annotations to the project file. That comes with its own set of challenges, but it does mean that only the
        // fields that are Option or ValueOption will be generated as nullable in the OpenAPI schema.
        [<Required>]
        Name: string

        [<Required>]
        Species: string option

        Color: AnimalColor option

        Age: int option
        Vaccinated: bool
    }

[<CLIMutable>]
type NumberTestRequest =
    { Value: int
      Price: decimal
      Score: float
      Description: string option }
