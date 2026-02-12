module OxpeckerOpenApi.BugTest.Handlers

open Microsoft.AspNetCore.Http
open System

open Oxpecker

open OxpeckerOpenApi.BugTest.Types

/// Redirects to the swagger interface from the root of the site.
let swaggerRedirectHandler: EndpointHandler =
    fun (ctx: HttpContext) -> redirectTo "swagger/index.html" true ctx

let greetingsHandler (name: string) : EndpointHandler =
    fun (ctx: HttpContext) ->
        task {
            let greeting = sprintf "Hello, %s!" name
            return! text greeting ctx
        }

let people: Person list =
    [ { Name = "Alice"
        Age = 30
        Occupation = Some "Engineer"
        YearsExperience = Some 8 }
      { Name = "Bob"
        Age = 17
        Occupation = Some "Designer"
        YearsExperience = None }
      { Name = "Charlie"
        Age = 35
        Occupation = None
        YearsExperience = None } ]

let searchPersonsHandler: EndpointHandler =
    fun (ctx: HttpContext) ->
        task {
            let request =
                try
                    ctx.BindQuery<PersonSearchRequest>()
                with _ex ->
                    { Name = None; Occupation = None } // In case of binding failure, return an empty search request

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

                    nameMatches && occupationMatches)

            return! json filteredPeople ctx
        }

let animals =
    [ { Name = "Buddy"
        Species = Some "Dog"
        Age = Some 5
        Vaccinated = true }
      { Name = "Mittens"
        Species = Some "Cat"
        Age = Some 3
        Vaccinated = false }
      { Name = "Tweety"
        Species = Some "Bird"
        Age = None
        Vaccinated = true }
      { Name = "Nemo"
        Species = Some "Fish"
        Age = Some 1
        Vaccinated = false }
      { Name = "Rex"
        Species = Some "Reptile"
        Age = Some 4
        Vaccinated = true }
      { Name = "Stitch"
        Species = None
        Age = None
        Vaccinated = false } ]

let searchAnimalsHandler: EndpointHandler =
    fun (ctx: HttpContext) ->
        task {
            let request =
                try
                    ctx.BindQuery<AnimalSearchRequest>()
                with _ex ->
                    // In case of binding failure, return an empty search request
                    { AnimalSearchRequest.Name = None
                      Species = None
                      Vaccinated = None }

            let filteredAnimals =
                animals
                |> List.filter (fun a ->
                    let nameMatches =
                        match request.Name with
                        | Some nameFilter -> a.Name.Contains(nameFilter, StringComparison.OrdinalIgnoreCase)
                        | None -> true

                    let speciesMatches =
                        match request.Species with
                        | Some speciesFilter ->
                            match a.Species with
                            | Some sp -> sp.Contains(speciesFilter, StringComparison.OrdinalIgnoreCase)
                            | None -> false
                        | None -> true

                    let vaccinatedMatches =
                        match request.Vaccinated with
                        | Some vaccinatedFilter -> a.Vaccinated = vaccinatedFilter
                        | None -> true

                    nameMatches && speciesMatches && vaccinatedMatches)

            return! json filteredAnimals ctx
        }
