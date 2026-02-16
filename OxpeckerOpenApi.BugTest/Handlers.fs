module OxpeckerOpenApi.BugTest.Handlers

open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Http.Json
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Options
open System
open System.Text.Json

open Oxpecker

open OxpeckerOpenApi.BugTest.Types
open OxpeckerOpenApi.BugTest.UnionTypeTests

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
        Color = Some Brown
        Vaccinated = true }
      { Name = "Mittens"
        Species = Some "Cat"
        Age = Some 3
        Color = Some Black
        Vaccinated = false }
      { Name = "Tweety"
        Species = Some "Bird"
        Age = None
        Color = Some White
        Vaccinated = true }
      { Name = "Nemo"
        Species = Some "Fish"
        Age = Some 1
        Color = Some Spotted
        Vaccinated = false }
      { Name = "Rex"
        Species = Some "Reptile"
        Age = Some 4
        Color = None
        Vaccinated = true }
      { Name = "Stitch"
        Species = None
        Age = None
        Color = None
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

            // Now use Oxpecker's json helper - it should work!
            return! json filteredAnimals ctx
        }

let testUnionSerializationHandler: EndpointHandler =
    fun (ctx: HttpContext) ->
        task {
            // Get the JsonSerializerOptions from the IoC container
            let jsonOptions = ctx.RequestServices.GetRequiredService<IOptions<JsonOptions>>()
            let serializerOptions = jsonOptions.Value.SerializerOptions

            // Capture console output
            let originalOut = Console.Out
            use stringWriter = new System.IO.StringWriter()
            Console.SetOut(stringWriter)

            try
                let allPassed = runAllTests serializerOptions
                let output = stringWriter.ToString()
                Console.SetOut(originalOut)

                ctx.Response.ContentType <- "text/plain"
                return! text output ctx
            with ex ->
                Console.SetOut(originalOut)
                let errorOutput = stringWriter.ToString() + sprintf "\n\nException: %s\nStack Trace: %s" ex.Message ex.StackTrace
                ctx.Response.ContentType <- "text/plain"
                ctx.Response.StatusCode <- 500
                return! text errorOutput ctx
        }

let testAnimalColorHandler: EndpointHandler =
    fun (ctx: HttpContext) ->
        task {
            // Get the JsonSerializerOptions from the IoC container
            let jsonOptions = ctx.RequestServices.GetRequiredService<IOptions<JsonOptions>>()
            let serializerOptions = jsonOptions.Value.SerializerOptions

            // Return sample animals to test how Color serializes
            let testAnimals = [
                { Name = "Buddy"; Species = Some "Dog"; Age = Some 5; Color = Some Brown; Vaccinated = true }
                { Name = "Mystery"; Species = Some "Cat"; Age = Some 3; Color = None; Vaccinated = false }
                { Name = "Spot"; Species = Some "Dog"; Age = Some 2; Color = Some Spotted; Vaccinated = true }
            ]

            // Serialize using the configured JsonSerializerOptions
            let jsonString = JsonSerializer.Serialize(testAnimals, serializerOptions)
            ctx.Response.ContentType <- "application/json"
            return! text jsonString ctx
        }

/// Response type that includes all union type examples for OpenAPI schema generation
type UnionExamplesResponse =
    { SimpleStatus: SimpleStatus list
      Shape: Shape list
      PaymentMethod: PaymentMethod list
      ResultSuccess: Result<int, string>
      ResultFailure: Result<int, string>
      ApiResponseUser: ApiResponse
      ApiResponseError: ApiResponse
      ContactInfo: ContactInfo list
      AnimalColor: AnimalColor list
      Animals: Animal list }

/// Handler that returns dummy data for all union types to visualize in OpenAPI spec
let unionExamplesHandler: EndpointHandler =
    fun (ctx: HttpContext) ->
        task {
            // Get the JsonSerializerOptions from the IoC container
            let jsonOptions = ctx.RequestServices.GetRequiredService<IOptions<JsonOptions>>()
            let serializerOptions = jsonOptions.Value.SerializerOptions

            let examples = {
                // Simple union examples (enum-like)
                SimpleStatus = [
                    Active
                    Inactive
                    Pending
                ]

                // Shape examples (complex union with fields)
                Shape = [
                    Circle 5.0
                    Rectangle(10.0, 20.0)
                    Triangle(8.0, 6.0)
                ]

                // PaymentMethod examples (mixed union - some with data, some without)
                PaymentMethod = [
                    Cash
                    CreditCard("1234-5678-9012-3456", DateTime(2025, 12, 31))
                    BankTransfer("ACC123456789")
                    PayPal("user@example.com")
                ]

                // Result examples (generic union)
                ResultSuccess = Success 42
                ResultFailure = Failure "Error occurred"

                // ApiResponse examples (multi-field union)
                ApiResponseUser = UserData("John Doe", 30)
                ApiResponseError = ErrorMessage(404, "Not Found")

                // ContactInfo examples (union with nested record)
                ContactInfo = [
                    Email "test@example.com"
                    Phone "+1-555-0123"
                    MailingAddress { Street = "123 Main St"; City = "Springfield"; ZipCode = "12345" }
                ]

                // AnimalColor examples (enum-like union from Types.fs)
                AnimalColor = [
                    Brown
                    Black
                    White
                    Spotted
                ]

                // Animals with different color combinations
                Animals = [
                    { Name = "Buddy"; Species = Some "Dog"; Age = Some 5; Color = Some Brown; Vaccinated = true }
                    { Name = "Mittens"; Species = Some "Cat"; Age = Some 3; Color = Some Black; Vaccinated = false }
                    { Name = "Tweety"; Species = Some "Bird"; Age = None; Color = Some White; Vaccinated = true }
                    { Name = "Nemo"; Species = Some "Fish"; Age = Some 1; Color = Some Spotted; Vaccinated = false }
                    { Name = "Mystery"; Species = Some "Unknown"; Age = None; Color = None; Vaccinated = false }
                ]
            }

            // Serialize using the configured JsonSerializerOptions that includes FSharp.SystemTextJson converter
            let jsonString = JsonSerializer.Serialize(examples, serializerOptions)
            ctx.Response.ContentType <- "application/json"
            return! text jsonString ctx
        }

/// Handler that returns a single Shape union value to test direct union serialization
let singleShapeHandler: EndpointHandler =
    fun (ctx: HttpContext) ->
        task {
            // Get the JsonSerializerOptions from the IoC container
            let jsonOptions = ctx.RequestServices.GetRequiredService<IOptions<JsonOptions>>()
            let serializerOptions = jsonOptions.Value.SerializerOptions

            let shape = Circle 10.0

            // Serialize using the configured JsonSerializerOptions
            let jsonString = JsonSerializer.Serialize(shape, serializerOptions)
            ctx.Response.ContentType <- "application/json"
            return! text jsonString ctx
        }

/// Handler that returns an AnimalColor enum value
let singleAnimalColorHandler: EndpointHandler =
    fun (ctx: HttpContext) ->
        task {
            // Get the JsonSerializerOptions from the IoC container
            let jsonOptions = ctx.RequestServices.GetRequiredService<IOptions<JsonOptions>>()
            let serializerOptions = jsonOptions.Value.SerializerOptions

            let color = Brown

            // Serialize using the configured JsonSerializerOptions
            let jsonString = JsonSerializer.Serialize(color, serializerOptions)
            ctx.Response.ContentType <- "application/json"
            return! text jsonString ctx
        }
