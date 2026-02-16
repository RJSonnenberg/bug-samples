// Example usage of the generated TypeScript client
import {
  Configuration,
  OxpeckerOpenApiBugTestApi,
  ExamplesApi,
  SearchApi,
  TestingApi,
  AnimalColor,
  Shape
} from './generated-client';

// Create a configuration with your API base URL
const config = new Configuration({
  basePath: 'http://localhost:5166',
});

// Create instances of the API clients
const mainApi = new OxpeckerOpenApiBugTestApi(config);
const examplesApi = new ExamplesApi(config);
const searchApi = new SearchApi(config);
const testingApi = new TestingApi(config);

// Example 1: Call the hello endpoint
async function callHello() {
  try {
    const response = await mainApi.hello();
    console.log('Hello response:', response.data);
  } catch (error) {
    console.error('Error calling hello:', error);
  }
}

// Example 2: Call the greeting endpoint with a parameter
async function callGreeting(name: string) {
  try {
    const response = await mainApi.getGreeting({ name });
    console.log(`Greeting for ${name}:`, response.data);
  } catch (error) {
    console.error('Error calling greeting:', error);
  }
}

// Example 3: Get union examples (F# discriminated unions)
async function getUnionExamples() {
  try {
    const response = await examplesApi.getUnionExamples();
    console.log('Union examples:', response.data);
  } catch (error) {
    console.error('Error getting union examples:', error);
  }
}

// Example 4: Get a single Shape union value
async function getSingleShape() {
  try {
    const response = await examplesApi.getSingleShape();
    console.log('Single shape:', response.data);

    // TypeScript discriminated union handling
    const shape: Shape = response.data;
    if (shape.type === 'Circle') {
      console.log('Circle radius:', shape.radius);
    } else if (shape.type === 'Rectangle') {
      console.log('Rectangle dimensions:', shape.width, 'x', shape.height);
    } else if (shape.type === 'Triangle') {
      console.log('Triangle base and height:', shape.base, shape.height);
    }
  } catch (error) {
    console.error('Error getting single shape:', error);
  }
}

// Example 5: Get a single AnimalColor enum value
async function getSingleAnimalColor() {
  try {
    const response = await examplesApi.getSingleAnimalColor();
    console.log('Single animal color:', response.data);

    // Working with enum
    const color: AnimalColor = response.data;
    if (color === AnimalColor.Brown) {
      console.log('The animal is brown!');
    }
  } catch (error) {
    console.error('Error getting single animal color:', error);
  }
}

// Example 6: Search for animals
async function searchAnimals() {
  try {
    const response = await searchApi.searchAnimals({
      name: 'Fluffy',
      species: 'Cat',
      vaccinated: true
    });
    console.log('Animals found:', response.data);
  } catch (error) {
    console.error('Error searching animals:', error);
  }
}

// Example 7: Search for persons
async function searchPersons() {
  try {
    const response = await searchApi.searchPersons({
      name: 'John',
      occupation: 'Developer'
    });
    console.log('Persons found:', response.data);
  } catch (error) {
    console.error('Error searching persons:', error);
  }
}

// Example 8: Test animal color serialization
async function testAnimalColor() {
  try {
    const response = await testingApi.testAnimalColor();
    console.log('Animal color test:', response.data);
  } catch (error) {
    console.error('Error testing animal color:', error);
  }
}

// Example 9: Test union serialization
async function testUnionSerialization() {
  try {
    const response = await testingApi.testUnionSerialization();
    console.log('Union serialization test:', response.data);
  } catch (error) {
    console.error('Error testing union serialization:', error);
  }
}

// Run examples
(async () => {
  console.log('Running API client examples...\n');

  await callHello();
  await callGreeting('World');
  await getUnionExamples();
  await getSingleShape();
  await getSingleAnimalColor();
  await searchAnimals();
  await searchPersons();
  await testAnimalColor();
  await testUnionSerialization();

  console.log('\nAll examples completed!');
})();

// Export for use in other files
export { mainApi, examplesApi, searchApi, testingApi, config };
