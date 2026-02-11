module OxpeckerOpenApi.BugTest.Types

open System.ComponentModel.DataAnnotations

[<CLIMutable>]
type PersonSearchRequest =
    {
        Name: string option
        Occupation: string option
    }

[<CLIMutable>]
type Person =
    {
        Name: string
        Age: int
        Occupation: string option
        YearsExperience: int option
    }


[<CLIMutable>]
type AnimalSearchRequest =
    {
        Name: string option
        Species: string option
        Vaccinated: bool option
    }

[<CLIMutable>]
type Animal =
    {
        [<Required()>]
        Name: string
        
        [<Required()>]
        Species: string option

        Age: int option
        Vaccinated: bool
    }