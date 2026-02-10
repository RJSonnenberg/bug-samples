module OxpeckerOpenApi.BugTest.Program

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open System.Text.Json
open System.Threading.Tasks

open Oxpecker
open Oxpecker.OpenApi

open OxpeckerOpenApi.BugTest.Endpoints

let addOxpecker (bldr: WebApplicationBuilder) =
    bldr.Services
        .ConfigureHttpJsonOptions(fun options ->
            options.SerializerOptions.PropertyNamingPolicy <- JsonNamingPolicy.CamelCase
            options.SerializerOptions.WriteIndented <- true
            options.SerializerOptions.Converters.Add(Serialization.JsonStringEnumConverter())
        )
        .AddRouting()
        .AddOxpecker()
        .AddOpenApi("v1", fun options ->
            options
                .AddDocumentTransformer(fun doc _ _ ->
                    doc.Info.Title <- "Oxpecker OpenAPI Bug Test API"
                    doc.Info.Version <- "v1"
                    Task.CompletedTask
                )
                .AddSchemaTransformer(FSharpOptionSchemaTransformer())
            |> ignore
        )
        |> ignore
    bldr

let useOxpecker (app: WebApplication) =
    let env = app.Services.GetService<IWebHostEnvironment>()
    app
        .UseRouting()
        .UseOxpecker(endpoints)
        .UseSwaggerUI(fun c ->
            c.DocumentTitle <- "Oxpecker OpenAPI Bug Test API - Swagger UI"
            c.DocExpansion Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List
            c.DisplayOperationId()
            c.DisplayRequestDuration()
            c.EnableDeepLinking()
            c.SwaggerEndpoint("../openapi/v1.json", "Oxpecker OpenAPI Bug Test API")
        )
        |> ignore
    app.MapOpenApi() |> ignore
    if not (env.IsDevelopment()) then
        app.UseDeveloperExceptionPage() |> ignore
    app

let build (bldr: WebApplicationBuilder) = bldr.Build()

let run (host: WebApplication) = host.Run()

[<EntryPoint>]
let main args =
    WebApplication.CreateBuilder(args)
    |> addOxpecker
    |> build
    |> useOxpecker
    |> run

    0 // Exit code

