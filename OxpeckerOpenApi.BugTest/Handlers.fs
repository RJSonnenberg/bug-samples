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

/// Response type that includes supported union type examples for OpenAPI schema generation
/// Note: Removed unsupported types (Result, PaymentMethod, ApiResponse, ContactInfo, Address)
/// These generic and complex unions do not generate schemas in Oxpecker
type UnionExamplesResponse =
    { SimpleStatus: SimpleStatus list
      Shape: Shape list
      HttpStatus: HttpStatus list
      AnimalColor: AnimalColor list
      Animals: Animal list
      SampleLocation: Location
      SampleMapData: MapData
      Responses: HttpResponse list }

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

                // HTTP Status examples (enum-like)
                HttpStatus = [
                    OK
                    Created
                    BadRequest
                    NotFound
                    ServerError
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

                // Sample Location union (new complex type)
                SampleLocation = PointLocation { X = 42.5; Y = 73.2; Z = Some 1000.0 }

                // Sample MapData (record with unions and lists)
                SampleMapData = {
                    Name = "Example Region"
                    Locations = [
                        PointLocation { X = 0.0; Y = 0.0; Z = None }
                        AreaName "Example Area"
                    ]
                    Status = OK
                    Altitude = Some 500.0
                }

                // HTTP Response examples (union with various field types)
                Responses = [
                    SuccessResponse(200, "Operation successful")
                    RedirectResponse(301, "/new-path")
                    ErrorResponse(400, "Bad request", Some "Invalid format")
                    FatalError(500, "Internal error")
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

/// Handler that tests number deserialization with AllowReadingFromString
let testNumberDeserializationHandler: EndpointHandler =
    fun (ctx: HttpContext) ->
        task {
            try
                let! request = ctx.BindJson<NumberTestRequest>()
                let response =
                    sprintf "Successfully deserialized: Value=%d, Price=%M, Score=%f, Description=%s"
                        request.Value
                        request.Price
                        request.Score
                        (request.Description |> Option.defaultValue "none")
                return! text response ctx
            with ex ->
                ctx.Response.StatusCode <- 400
                return! text (sprintf "Error: %s" ex.Message) ctx
        }
/// Handler returning complex nested data with lists and unions
let complexDataHandler: EndpointHandler =
    fun (ctx: HttpContext) ->
        task {
            let mapData: MapData =
                { Name = "Test Region"
                  Locations =
                    [ PointLocation { X = 10.5; Y = 20.3; Z = Some 100.0 }
                      AreaName "Mountain Range"
                      PointLocation { X = 15.0; Y = 25.0; Z = None }
                      Unknown ]
                  Status = OK
                  Altitude = Some 5280.0 }

            return! json mapData ctx
        }

/// Handler returning union with various field types
let httpResponseHandler: EndpointHandler =
    fun (ctx: HttpContext) ->
        task {
            let responses: HttpResponse list =
                [ SuccessResponse(200, "Operation completed successfully")
                  ErrorResponse(400, "Invalid request", Some "Missing required field: id")
                  RedirectResponse(301, "https://example.com/new-location")
                  FatalError(500, "Database connection failed") ]

            return! json responses ctx
        }

/// Handler returning record with nested unions
let apiRequestHandler: EndpointHandler =
    fun (ctx: HttpContext) ->
        task {
            let request: ApiRequest =
                { Id = "req-12345"
                  Timestamp = DateTime.UtcNow
                  Status = Created
                  Response = Some (SuccessResponse(201, "Resource created"))
                  Tags = [ "api"; "test"; "v1" ] }

            return! json request ctx
        }

/// Handler testing map data with multiple locations
let mapDataListHandler: EndpointHandler =
    fun (ctx: HttpContext) ->
        task {
            let mapDataList: MapData list =
                [ { Name = "Region A"
                    Locations = [ PointLocation { X = 0.0; Y = 0.0; Z = None } ]
                    Status = OK
                    Altitude = Some 0.0 }
                  { Name = "Region B"
                    Locations =
                      [ AreaName "Northern Area"
                        PointLocation { X = 100.0; Y = 100.0; Z = Some 500.0 } ]
                    Status = BadRequest
                    Altitude = None }
                  { Name = "Region C"
                    Locations = [ Unknown ]
                    Status = ServerError
                    Altitude = Some 2000.0 } ]

            return! json mapDataList ctx
        }