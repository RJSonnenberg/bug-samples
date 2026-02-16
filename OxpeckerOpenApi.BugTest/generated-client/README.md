## oxpecker-api-client@1.0.0

This generator creates TypeScript/JavaScript client that utilizes [axios](https://github.com/axios/axios). The generated Node module can be used in the following environments:

Environment
* Node.js
* Webpack
* Browserify

Language level
* ES5 - you must have a Promises/A+ library installed
* ES6

Module system
* CommonJS
* ES6 module system

It can be used in both TypeScript and JavaScript. In TypeScript, the definition will be automatically resolved via `package.json`. ([Reference](https://www.typescriptlang.org/docs/handbook/declaration-files/consumption.html))

### Building

To build and compile the typescript sources to javascript use:
```
npm install
npm run build
```

### Publishing

First build the package then run `npm publish`

### Consuming

navigate to the folder of your consuming project and run one of the following commands.

_published:_

```
npm install oxpecker-api-client@1.0.0 --save
```

_unPublished (not recommended):_

```
npm install PATH_TO_GENERATED_PACKAGE --save
```

### Documentation for API Endpoints

All URIs are relative to *http://localhost:5166*

Class | Method | HTTP request | Description
------------ | ------------- | ------------- | -------------
*ExamplesApi* | [**getSingleAnimalColor**](docs/ExamplesApi.md#getsingleanimalcolor) | **GET** /test-single-color | Get a single AnimalColor union value
*ExamplesApi* | [**getSingleShape**](docs/ExamplesApi.md#getsingleshape) | **GET** /test-single-shape | Get a single Shape union value
*ExamplesApi* | [**getUnionExamples**](docs/ExamplesApi.md#getunionexamples) | **GET** /union-examples | Get examples of all F# union types
*OxpeckerOpenApiBugTestApi* | [**getGreeting**](docs/OxpeckerOpenApiBugTestApi.md#getgreeting) | **GET** /greetings/{name} | 
*OxpeckerOpenApiBugTestApi* | [**hello**](docs/OxpeckerOpenApiBugTestApi.md#hello) | **GET** /hello | 
*SearchApi* | [**searchAnimals**](docs/SearchApi.md#searchanimals) | **POST** /animals | Search for animals based on query parameters
*SearchApi* | [**searchPersons**](docs/SearchApi.md#searchpersons) | **POST** /persons | Search for persons based on query parameters
*TestingApi* | [**testAnimalColor**](docs/TestingApi.md#testanimalcolor) | **GET** /test-animal-color | Test AnimalColor serialization
*TestingApi* | [**testNumberDeserialization**](docs/TestingApi.md#testnumberdeserialization) | **POST** /test-number-deserialization | Test number deserialization with AllowReadingFromString
*TestingApi* | [**testUnionSerialization**](docs/TestingApi.md#testunionserialization) | **GET** /test-union-serialization | Test F# union type serialization


### Documentation For Models

 - [AnimalColor](docs/AnimalColor.md)
 - [FSharpList1](docs/FSharpList1.md)
 - [FSharpList1Cons](docs/FSharpList1Cons.md)
 - [FSharpList1Empty](docs/FSharpList1Empty.md)
 - [FSharpList1OneOf](docs/FSharpList1OneOf.md)
 - [FSharpList1OneOf1](docs/FSharpList1OneOf1.md)
 - [FSharpListOfAnimal](docs/FSharpListOfAnimal.md)
 - [FSharpListOfPerson](docs/FSharpListOfPerson.md)
 - [FSharpListOfPersonOneOf](docs/FSharpListOfPersonOneOf.md)
 - [Shape](docs/Shape.md)
 - [ShapeCircle](docs/ShapeCircle.md)
 - [ShapeOneOf](docs/ShapeOneOf.md)
 - [ShapeOneOf1](docs/ShapeOneOf1.md)
 - [ShapeOneOf2](docs/ShapeOneOf2.md)
 - [ShapeRectangle](docs/ShapeRectangle.md)
 - [ShapeTriangle](docs/ShapeTriangle.md)


<a id="documentation-for-authorization"></a>
## Documentation For Authorization

Endpoints do not require authorization.

