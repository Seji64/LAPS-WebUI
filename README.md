# LAPS-WebUI
A nice and simple Web Interface for LAPS (Local Administrator Password Solution)

## Setup Prerequisites

- A working Active Directory with Microsoft LAPS installed

## Setup:

- Download the latest Release for your Platform
- Unzip Archive
- Copy `settings.json.example` and renamed it to `settings.json`
- Adjust settings.json and run LAPS WebUI

## Docker:

You can build your own docker image containing your specific config.
- Adjust settings.json as above.
- Also adjust settings.json to set the ListenAddress to "0.0.0.0"
- From the src directory, run `docker build . -t lapsui`
- Then run `docker run -p 8080:8080 laps` and follow the usage steps below.

## Usage:

- Navigate with your WebBrowser to the LAPS WebUI Page (Default: http://127.0.0.1:8080 )
- Login with any Active Directory User which can access the LAPS LDAP Properties
- Type any Computername in the Searchbox and click a result to view the LAPS Password.


## Screenshots:

![Screenshot](https://raw.githubusercontent.com/Seji64/LAPS-WebUI/master/screenshots/screenshot01.png)

![Screenshot](https://raw.githubusercontent.com/Seji64/LAPS-WebUI/master/screenshots/screenshot02.png)

![Screenshot](https://raw.githubusercontent.com/Seji64/LAPS-WebUI/master/screenshots/screenshot03.png)
