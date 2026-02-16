# ExamplesApi

All URIs are relative to *http://localhost:5166*

|Method | HTTP request | Description|
|------------- | ------------- | -------------|
|[**getSingleAnimalColor**](#getsingleanimalcolor) | **GET** /test-single-color | Get a single AnimalColor union value|
|[**getSingleShape**](#getsingleshape) | **GET** /test-single-shape | Get a single Shape union value|
|[**getUnionExamples**](#getunionexamples) | **GET** /union-examples | Get examples of all F# union types|

# **getSingleAnimalColor**
> AnimalColor getSingleAnimalColor()

Returns a single AnimalColor (Brown) to test enum-like union schema generation

### Example

```typescript
import {
    ExamplesApi,
    Configuration
} from 'oxpecker-api-client';

const configuration = new Configuration();
const apiInstance = new ExamplesApi(configuration);

const { status, data } = await apiInstance.getSingleAnimalColor();
```

### Parameters
This endpoint does not have any parameters.


### Return type

**AnimalColor**

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

# **getSingleShape**
> Shape getSingleShape()

Returns a single Shape (Circle) to test union schema generation

### Example

```typescript
import {
    ExamplesApi,
    Configuration
} from 'oxpecker-api-client';

const configuration = new Configuration();
const apiInstance = new ExamplesApi(configuration);

const { status, data } = await apiInstance.getSingleShape();
```

### Parameters
This endpoint does not have any parameters.


### Return type

**Shape**

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

# **getUnionExamples**
> any getUnionExamples()

Returns dummy data for all union types (SimpleStatus, Shape, PaymentMethod, Result, ApiResponse, ContactInfo, AnimalColor). Use this to visualize the generated OpenAPI schemas for F# discriminated unions.

### Example

```typescript
import {
    ExamplesApi,
    Configuration
} from 'oxpecker-api-client';

const configuration = new Configuration();
const apiInstance = new ExamplesApi(configuration);

const { status, data } = await apiInstance.getUnionExamples();
```

### Parameters
This endpoint does not have any parameters.


### Return type

**any**

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

