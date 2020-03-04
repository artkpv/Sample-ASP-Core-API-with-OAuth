# Biking tracking app (BA)

A sample project. REST API that tracks biking times of users

## Roadmap / features

- [ ] Demo
    - [ ] Python client, script with interactive work. See https://github.com/swagger-api/swagger-codegen#to-generate-a-sample-client-library

- [ ] All API calls must be authenticated under OAuth 2.
    - [ ] HTTPS. TSL / Certificates. 
- [ ] A time entry when entered has a date, distance, time, and location.
    - [ ] Fix E2E test if on SQLite. Time zone issues? See filtering with startTime.
- [ ] Based on the provided date and location, API should connect to a weather API provider and get the weather conditions for the trip, and store that with each one.
- [ ] The API must create a report on average speed & distance per week.
- [ ] The API must be able to return data in the JSON format.
- [ ] The API should provide filter capabilities for all endpoints that return a list of elements, as well should be able to support pagination.
- [ ] The API filtering should allow using parenthesis for defining operations precedence and use any combination of the available fields. The supported operations should at least include or, and, eq (equals), ne (not equals), gt (greater than), lt (lower than).
- [ ] Example -> (date eq '2016-05-01') AND ((distance gt 20) OR (distance lt 10)).
- [ ] API Users must be able to create an account and log in.
- [ ] Write unit tests
    - [ ] Test empty coordinates
- [ ] Write E2E tests


## Refs

- ASP.NET Web API https://docs.microsoft.com/en-us/aspnet/core/web-api/?view=aspnetcore-3.1
- OpenWeather API https://openweathermap.org/api
- IdentityServer4 https://identityserver4.readthedocs.io/en/latest/index.html#
- OAuth 2. https://aaronparecki.com/oauth-2-simplified/


