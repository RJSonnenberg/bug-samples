module OxpeckerOpenApi.BugTest.Endpoints

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Mvc
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
            |> configureEndpoint _.WithName("SearchPersons")
            |> addOpenApi (OpenApiConfig(
                responseBodies = [| ResponseBody(typeof<Person list>) |],
                configureOperation = fun op _ _ ->
                    op.Summary <- "Search for persons based on query parameters"
                    op.Description <- "Returns a list of persons matching the search criteria provided in the query parameters."
                    op.Parameters <- List<IOpenApiParameter>()
                    op.Parameters.Add(OpenApiParameter(
                        Required = false,
                        Name = "Name",
                        In = ParameterLocation.Query,
                        Description = "Filter by name (optional)",
                        Schema = OpenApiSchema(Type = JsonSchemaType.String)
                    ))
                    op.Parameters.Add(OpenApiParameter(
                        Required = false,
                        Name = "Occupation",
                        In = ParameterLocation.Query,
                        Description = "Filter by occupation (optional)",
                        Schema = OpenApiSchema(Type = JsonSchemaType.String)
                    ))
                    Task.CompletedTask
            ))
            route "/animals" searchAnimalsHandler
            |> configureEndpoint _.WithName("SearchAnimals")
            |> addOpenApi (OpenApiConfig(
                responseBodies = [| ResponseBody(typeof<Animal list>) |],
                configureOperation = fun op _ _ ->
                    op.Summary <- "Search for animals based on query parameters"
                    op.Description <- "Returns a list of animals matching the search criteria provided in the query parameters."
                    op.Parameters <- List<IOpenApiParameter>()
                    op.Parameters.Add(OpenApiParameter(
                        Required = false,
                        Name = "Name",
                        In = ParameterLocation.Query,
                        Description = "Filter by name (required)",
                        Schema = OpenApiSchema(Type = JsonSchemaType.String)
                    ))
                    op.Parameters.Add(OpenApiParameter(
                        Required = false,
                        Name = "Species",
                        In = ParameterLocation.Query,
                        Description = "Filter by species (optional)",
                        Schema = OpenApiSchema(Type = JsonSchemaType.String)
                    ))
                    op.Parameters.Add(OpenApiParameter(
                        Required = false,
                        Name = "Vaccinated",
                        In = ParameterLocation.Query,
                        Description = "Filter by vaccination status (optional)",
                        Schema = OpenApiSchema(Type = JsonSchemaType.Boolean)
                    ))
                    Task.CompletedTask
            ))
        ]
    ]