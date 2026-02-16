# TestingApi

All URIs are relative to *http://localhost:5166*

|Method | HTTP request | Description|
|------------- | ------------- | -------------|
|[**testAnimalColor**](#testanimalcolor) | **GET** /test-animal-color | Test AnimalColor serialization|
|[**testNumberDeserialization**](#testnumberdeserialization) | **POST** /test-number-deserialization | Test number deserialization with AllowReadingFromString|
|[**testUnionSerialization**](#testunionserialization) | **GET** /test-union-serialization | Test F# union type serialization|

# **testAnimalColor**
> FSharpListOfAnimal testAnimalColor()

Returns sample animals to verify how AnimalColor option is serialized

### Example

```typescript
import {
    TestingApi,
    Configuration
} from 'oxpecker-api-client';

const configuration = new Configuration();
const apiInstance = new TestingApi(configuration);

const { status, data } = await apiInstance.testAnimalColor();
```

### Parameters
This endpoint does not have any parameters.


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

# **testNumberDeserialization**
> string testNumberDeserialization(body)

Test endpoint to verify that numbers can be deserialized from strings when using AllowReadingFromString. This endpoint accepts JSON with numbers that might be strings.

### Example

```typescript
import {
    TestingApi,
    Configuration
} from 'oxpecker-api-client';

const configuration = new Configuration();
const apiInstance = new TestingApi(configuration);

let body: any; //

const { status, data } = await apiInstance.testNumberDeserialization(
    body
);
```

### Parameters

|Name | Type | Description  | Notes|
|------------- | ------------- | ------------- | -------------|
| **body** | **any**|  | |


### Return type

**string**

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: application/json
 - **Accept**: text/plain


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
|**200** | OK |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

# **testUnionSerialization**
> string testUnionSerialization()

Runs comprehensive tests of F# union type serialization/deserialization using FSharp.SystemTextJson

### Example

```typescript
import {
    TestingApi,
    Configuration
} from 'oxpecker-api-client';

const configuration = new Configuration();
const apiInstance = new TestingApi(configuration);

const { status, data } = await apiInstance.testUnionSerialization();
```

### Parameters
This endpoint does not have any parameters.


### Return type

**string**

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: text/plain


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
|**200** | OK |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

