# LAPS-WebUI
A nice and simple Web Interface for LAPS (Local Administrator Password Solution)

## Setup Preqesites

- A working Active Directory with Microsoft LAPS installed
- .NET Core 7.0 compatible Operating System (Ubuntu/Debian/CentOS/Alpine Linux/Windows/macOS) or a Docker Host

## Setup (bare metal):

- Download the latest Release for your Platform
- Unzip Archive
- Adjust appsettings.json or set the settings via Environment Variables
- Run *LAPS-WebUI*

## Setup (docker):

Running LAPS-WebUI in docker is quite easy:
```
docker run -d \
  --name=lapswebui \
  -e LDAP__Server=ldap.example.com \
  -e LDAP__Port=389 \
  -e LDAP__UseSSL=false \
  -e LDAP__SearchBase=DC=example,DC=com \
  -p 8080:8080 \
  --restart unless-stopped \
  ghcr.io/seji64/laps-webui:1.4.2
```

## Usage:
- Navigate with your WebBrowser to the LAPS WebUI Page (Default: http://127.0.0.1:8080 )
- Login with any Active Directory User which can access the LAPS LDAP Properties
- Type any Computername in the Searchbox and click a result to view the LAPS Password.

## Why is there no User Management?

There is none cause *LAPS-WebUI* authenticates against your ActiveDirectory. There is also defined who can read those LAPS Passwords.

## Screenshots:

![Screenshot](https://raw.githubusercontent.com/Seji64/LAPS-WebUI/master/screenshots/screenshot01.png)

![Screenshot](https://raw.githubusercontent.com/Seji64/LAPS-WebUI/master/screenshots/screenshot02.png)
