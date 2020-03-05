# Biking tracking app (BA)

A sample project. REST API that tracks biking times of users

## Prerequisites

- [.NET Core 3.1 SDK or later](https://dotnet.microsoft.com/download/dotnet-core/3.1) to run ASP.NET Core WebAPI.
- [Python 3](https://www.python.org/downloads/) to run Demo and E2E tests.
- [PowerShell](https://docs.microsoft.com/en-us/powershell/scripting/install/installing-windows-powershell?view=powershell-7) to run build scripts and demo.

## Get started

- Make sure prerequisites are installed and ready
- Clone `git clone git@github.com:artkpv/Sample-ASP-Core-API-with-OAuth.git`
- `cd Sample-ASP-Core-API-with-OAuth`
- Run Demo: `powershell ./build.ps1 demo`. It will listen to 5000, 5001, 5002, 5003 ports by default (changable in ./build.ps1).

## Roadmap / features

- [x] All API calls must be authenticated under OAuth 2.
- [x] A time entry when entered has a date, distance, time, and location.
- [x] Based on the provided date and location, API should connect to a weather API provider and get the weather conditions for the trip, and store that with each one.
- [x] The API must create a report on average speed & distance per week.
- [x] The API must be able to return data in the JSON format.
- [x] The API should provide filter capabilities for all endpoints that return a list of elements, as well should be able to support pagination.
- [x] The API filtering should allow using parenthesis for defining operations precedence and use any combination of the available fields. The supported operations should at least include or, and, eq (equals), ne (not equals), gt (greater than), lt (lower than).
- [x] Example -> (date eq '2016-05-01') AND ((distance gt 20) OR (distance lt 10)).
- [x] API Users must be able to create an account and log in.
- [x] Write unit tests
- [x] Write E2E tests

## Refs

- ASP.NET Web API https://docs.microsoft.com/en-us/aspnet/core/web-api/?view=aspnetcore-3.1
- OpenWeather API https://openweathermap.org/api
- IdentityServer4 https://identityserver4.readthedocs.io/en/latest/index.html#
- OAuth 2. https://aaronparecki.com/oauth-2-simplified/

## License

See LICENSE file
