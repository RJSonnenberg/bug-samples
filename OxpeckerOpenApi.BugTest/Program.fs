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

let addOxpecker (bldr: WebApplicationBuilder) =
    bldr.Services
        .ConfigureHttpJsonOptions(fun options ->
            options.SerializerOptions.PropertyNamingPolicy <- JsonNamingPolicy.CamelCase
            options.SerializerOptions.WriteIndented <- true
            // If we do not set the NumberHandling to Strict, then the OpenAPI schema generator will generate a schema
            // that allows numbers to be represented as strings (the type will be `string | integer`), which is not what
            // we want in this case. Setting it to Strict ensures that the generated schema correctly reflects the
            // expected JSON structure.
            // See https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/include-metadata?view=aspnetcore-10.0&tabs=minimal-apis#numeric-types for more details.
            options.SerializerOptions.NumberHandling <- JsonNumberHandling.Strict
            options.SerializerOptions.Converters.Add(Serialization.JsonStringEnumConverter()))
        .AddRouting()
        .AddOxpecker()
        .AddOpenApi(
            "v1",
            fun options ->
                options
                    .AddDocumentTransformer(fun doc _ _ ->
                        doc.Info.Title <- "Oxpecker OpenAPI Bug Test API"
                        doc.Info.Version <- "v1"
                        Task.CompletedTask)
                    .AddSchemaTransformer(FSharpOptionSchemaTransformer())
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
