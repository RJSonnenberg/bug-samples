# SearchApi

All URIs are relative to *http://localhost:5166*

|Method | HTTP request | Description|
|------------- | ------------- | -------------|
|[**searchAnimals**](#searchanimals) | **POST** /animals | Search for animals based on query parameters|
|[**searchPersons**](#searchpersons) | **POST** /persons | Search for persons based on query parameters|

# **searchAnimals**
> FSharpListOfAnimal searchAnimals()

Returns a list of animals matching the search criteria provided in the query parameters.

### Example

```typescript
import {
    SearchApi,
    Configuration
} from 'oxpecker-api-client';

const configuration = new Configuration();
const apiInstance = new SearchApi(configuration);

let name: string; //Filter by name (required) (optional) (default to undefined)
let species: string; //Filter by species (optional) (optional) (default to undefined)
let vaccinated: boolean; //Filter by vaccination status (optional) (optional) (default to undefined)

const { status, data } = await apiInstance.searchAnimals(
    name,
    species,
    vaccinated
);
```

### Parameters

|Name | Type | Description  | Notes|
|------------- | ------------- | ------------- | -------------|
| **name** | [**string**] | Filter by name (required) | (optional) defaults to undefined|
| **species** | [**string**] | Filter by species (optional) | (optional) defaults to undefined|
| **vaccinated** | [**boolean**] | Filter by vaccination status (optional) | (optional) defaults to undefined|


### Return type

**FSharpListOfAnimal**

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
|**200** | OK |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

# **searchPersons**
> FSharpListOfPerson searchPersons()

Returns a list of persons matching the search criteria provided in the query parameters.

### Example

```typescript
import {
    SearchApi,
    Configuration
} from 'oxpecker-api-client';

const configuration = new Configuration();
const apiInstance = new SearchApi(configuration);

let name: string; //Filter by name (optional) (optional) (default to undefined)
let occupation: string; //Filter by occupation (optional) (optional) (default to undefined)

const { status, data } = await apiInstance.searchPersons(
    name,
    occupation
);
```

### Parameters

|Name | Type | Description  | Notes|
|------------- | ------------- | ------------- | -------------|
| **name** | [**string**] | Filter by name (optional) | (optional) defaults to undefined|
| **occupation** | [**string**] | Filter by occupation (optional) | (optional) defaults to undefined|


### Return type

**FSharpListOfPerson**

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
|**200** | OK |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

