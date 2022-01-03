# Change Log

## Current Version

v2.0.0

- Breaking changes due to dependency update
- Leveraging ```ExpressionTree``` package, replacing ```DbExpression``` and ```DbOperators```
- Passthrough of ```DatabaseSettings```, ```OrderDirection```, and ```ResultOrder``` from ```DatabaseWrapper```, eliminating proxy classes

## Previous Versions

v1.3.x

- Dependency update
- New APIs: Exists, Count, Sum
- Select with ordering
- Better support for DateTimeOffset
- ```ValidateTable```, ```ValidateTables```, and ```InitializeTables``` APIs

v1.2.3

- Sanitize API

v1.2.2

- Timestamp API

v1.2.1

- GetTableName API

v1.2.0

- Breaking changes; update to .NET Standard 2.1

v1.1.3

- Added 'Query' API

v1.1.2

- Add 'Between' DbOperator
- Dependency update

v1.1.1

- Minor refactor, better support for nullable types (i.e. int?, decimal?, etc) 

v1.1.0

- Minor refactor, split into multiple projects
 
v1.0.0

- Initial release

Notes from previous versions will be placed here.