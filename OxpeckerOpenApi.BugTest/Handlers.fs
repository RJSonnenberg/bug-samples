module OxpeckerOpenApi.BugTest.Handlers

open Microsoft.AspNetCore.Http
open System

open Oxpecker

open OxpeckerOpenApi.BugTest.Types

/// Redirects to the swagger interface from the root of the site.
let swaggerRedirectHandler : EndpointHandler =
    fun (ctx: HttpContext) -> redirectTo "swagger/index.html" true ctx

let people: Person list = [
        { Name = "Alice"; Age = 30; Occupation = Some "Engineer"; YearsExperience = Some 8 }
        { Name = "Bob"; Age = 17; Occupation = Some "Designer"; YearsExperience = None }
        { Name = "Charlie"; Age = 35; Occupation = None; YearsExperience = None }
    ]

let greetingsHandler (name: string) : EndpointHandler =
    fun (ctx: HttpContext) ->
        task {
            let greeting = sprintf "Hello, %s!" name
            return! text greeting ctx
        }

let searchPersonsHandler : EndpointHandler =
    fun (ctx: HttpContext) ->
        task {
            let request =
                try
                    ctx.BindQuery<PersonSearchRequest>()
                with
                | _ex -> { Name = None; Occupation = None } // In case of binding failure, return an empty search request

            let filteredPeople =
                people
                |> List.filter (fun p ->
                    let nameMatches =
                        match request.Name with
                        | Some nameFilter -> p.Name.Contains(nameFilter, StringComparison.OrdinalIgnoreCase)
                        | None -> true
                    let occupationMatches =
                        match request.Occupation with
                        | Some occupationFilter ->
                            match p.Occupation with
                            | Some occ -> occ.Contains(occupationFilter, StringComparison.OrdinalIgnoreCase)
                            | None -> false
                        | None -> true
                    nameMatches && occupationMatches
                )

            return! json filteredPeople ctx
        }