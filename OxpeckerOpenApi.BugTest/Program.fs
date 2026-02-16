module OxpeckerOpenApi.BugTest.Program

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open System.Text.Json
open System.Text.Json.Serialization
open System.Threading.Tasks

open Oxpecker
open Oxpecker.OpenApi

open OxpeckerOpenApi.BugTest.Endpoints
open OxpeckerOpenApi.BugTest.FSharpUnionSchemaTransformer

let addOxpecker (bldr: WebApplicationBuilder) =
    // Configure JSON serializer options first
    let jsonFSharpConverter =
        JsonFSharpOptions()
            .WithUnionInternalTag()           // Discriminator inside: { "type": "Circle", "radius": 5.0 }
            .WithUnionTagName("type")         // Use "type" as discriminator field
            .WithUnionNamedFields()           // Use named properties
            .WithUnionUnwrapFieldlessTags()   // Simple unions serialize as plain strings
            .WithUnwrapOption()               // Option<'T> serializes as T | null
        |> JsonFSharpConverter

    let oxpeckerJsonOptions = JsonSerializerOptions()
    oxpeckerJsonOptions.PropertyNamingPolicy <- JsonNamingPolicy.CamelCase  // camelCase
    oxpeckerJsonOptions.WriteIndented <- true
    oxpeckerJsonOptions.NumberHandling <- JsonNumberHandling.Strict
    oxpeckerJsonOptions.Converters.Add(jsonFSharpConverter)

    bldr.Services
        // Configure HttpJsonOptions (for ASP.NET Core's serialization)
        .ConfigureHttpJsonOptions(fun options ->
            options.SerializerOptions.PropertyNamingPolicy <- JsonNamingPolicy.CamelCase
            options.SerializerOptions.WriteIndented <- true
            options.SerializerOptions.NumberHandling <- JsonNumberHandling.Strict
            options.SerializerOptions.Converters.Add(jsonFSharpConverter)
        )
        .AddRouting()
        .AddOxpecker()
        // Register Oxpecker's SystemTextJsonSerializer
        .AddSingleton<IJsonSerializer>(Oxpecker.SystemTextJsonSerializer(oxpeckerJsonOptions))
        .AddOpenApi(
            "v1",
            fun options ->
                options
                    .AddDocumentTransformer(fun doc _ _ ->
                        doc.Info.Title <- "Oxpecker OpenAPI Bug Test API"
                        doc.Info.Version <- "v1"
                        Task.CompletedTask)
                    .AddSchemaTransformer(FSharpOptionSchemaTransformer())
                    .AddSchemaTransformer<FSharpUnionSchemaTransformer>()
                |> ignore
        )
    |> ignore

    bldr

let useOxpecker (app: WebApplication) =
    app
        .UseRouting()
        .UseOxpecker(endpoints)
        .UseSwaggerUI(fun c ->
            c.DocumentTitle <- "Oxpecker OpenAPI Bug Test API - Swagger UI"
            c.DocExpansion Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List
            c.DisplayOperationId()
            c.DisplayRequestDuration()
            c.EnableDeepLinking()
            c.SwaggerEndpoint("../openapi/v1.json", "Oxpecker OpenAPI Bug Test API"))
    |> ignore

    app.MapOpenApi() |> ignore

    // One of the side-effect of having the <Nullable> annotations enabled in the project is that the we have to be more
    // careful when retrieving a possibly null value from some ASP.NET Core APIs.
    match app.Services.GetService<IWebHostEnvironment>() |> Option.ofObj with
    | Some env when not (env.IsDevelopment()) ->
        app.UseDeveloperExceptionPage() |> ignore
    | _ -> ()

    app

let build (bldr: WebApplicationBuilder) = bldr.Build()

let run (host: WebApplication) = host.Run()

[<EntryPoint>]
let main args =
    WebApplication.CreateBuilder(args) |> addOxpecker |> build |> useOxpecker |> run

    0 // Exit code
