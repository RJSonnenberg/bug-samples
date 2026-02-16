# OxpeckerOpenApiBugTestApi

All URIs are relative to *http://localhost:5166*

|Method | HTTP request | Description|
|------------- | ------------- | -------------|
|[**getGreeting**](#getgreeting) | **GET** /greetings/{name} | |
|[**hello**](#hello) | **GET** /hello | |

# **getGreeting**
> string getGreeting()


### Example

```typescript
import {
    OxpeckerOpenApiBugTestApi,
    Configuration
} from 'oxpecker-api-client';

const configuration = new Configuration();
const apiInstance = new OxpeckerOpenApiBugTestApi(configuration);

let name: string; // (default to undefined)

const { status, data } = await apiInstance.getGreeting(
    name
);
```

### Parameters

|Name | Type | Description  | Notes|
|------------- | ------------- | ------------- | -------------|
| **name** | [**string**] |  | defaults to undefined|


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

# **hello**
> string hello()


### Example

```typescript
import {
    OxpeckerOpenApiBugTestApi,
    Configuration
} from 'oxpecker-api-client';

const configuration = new Configuration();
const apiInstance = new OxpeckerOpenApiBugTestApi(configuration);

const { status, data } = await apiInstance.hello();
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

