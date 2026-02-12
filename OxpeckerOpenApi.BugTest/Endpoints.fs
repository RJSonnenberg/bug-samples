module OxpeckerOpenApi.BugTest.Endpoints

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.OpenApi
open System.Collections.Generic
open System.Threading.Tasks

open Oxpecker
open Oxpecker.OpenApi

open OxpeckerOpenApi.BugTest.Types
open OxpeckerOpenApi.BugTest.Handlers

let endpoints =
    [
        GET [ route "/" swaggerRedirectHandler ]
        GET [
            route "/hello" (text "Hello, Oxpecker!")
            |> configureEndpoint _.WithName("Hello")
            |> addOpenApiSimple<unit, string>
        ]
        GET [
            routef "/greetings/{%s}" greetingsHandler
            |> configureEndpoint _.WithName("GetGreeting")
            |> addOpenApiSimple<unit, string>
        ]
        POST [
            route "/persons" searchPersonsHandler
            // Note the different way that we configure the tags, summary, and description here compared to the /animals
            // endpoint below. Both approaches work the same in terms of the generated OpenAPI document.
            // See https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/include-metadata?view=aspnetcore-10.0&tabs=minimal-apis#include-openapi-metadata-for-endpoints for more details.
            |> configureEndpoint
                _
                .WithName("SearchPersons")
                .WithTags([|"search"|])
                .WithSummary("Search for persons based on query parameters")
                .WithDescription("Returns a list of persons matching the search criteria provided in the query parameters.")
            |> addOpenApi (OpenApiConfig(
                responseBodies = [| ResponseBody(typeof<Person list>) |],
                configureOperation = fun op _ _ ->
                    let parameters = List<IOpenApiParameter>()
                    parameters.Add(OpenApiParameter(
                        Required = false,
                        Name = "Name",
                        In = ParameterLocation.Query,
                        Description = "Filter by name (optional)",
                        Schema = OpenApiSchema(Type = JsonSchemaType.String)
                    ))
                    parameters.Add(OpenApiParameter(
                        Required = false,
                        Name = "Occupation",
                        In = ParameterLocation.Query,
                        Description = "Filter by occupation (optional)",
                        Schema = OpenApiSchema(Type = JsonSchemaType.String)
                    ))
                    op.Parameters <- parameters
                    Task.CompletedTask
            ))
            route "/animals" searchAnimalsHandler
            |> configureEndpoint _.WithName("SearchAnimals").WithTags([|"search"|])
            |> addOpenApi (OpenApiConfig(
                responseBodies = [| ResponseBody(typeof<Animal list>) |],
                configureOperation = fun op _ _ ->
                    op.Tags <- OpenApiTagReference("search") |> Seq.singleton |> HashSet
                    op.Summary <- "Search for animals based on query parameters"
                    op.Description <- "Returns a list of animals matching the search criteria provided in the query parameters."
                    let parameters = List<IOpenApiParameter>()
                    parameters.Add(OpenApiParameter(
                        Required = false,
                        Name = "Name",
                        In = ParameterLocation.Query,
                        Description = "Filter by name (required)",
                        Schema = OpenApiSchema(Type = JsonSchemaType.String)
                    ))
                    parameters.Add(OpenApiParameter(
                        Required = false,
                        Name = "Species",
                        In = ParameterLocation.Query,
                        Description = "Filter by species (optional)",
                        Schema = OpenApiSchema(Type = JsonSchemaType.String)
                    ))
                    parameters.Add(OpenApiParameter(
                        Required = false,
                        Name = "Vaccinated",
                        In = ParameterLocation.Query,
                        Description = "Filter by vaccination status (optional)",
                        Schema = OpenApiSchema(Type = JsonSchemaType.Boolean)
                    ))
                    op.Parameters <- parameters
                    Task.CompletedTask
            ))
        ]
    ]