# LAPS-WebUI
A nice and simple Web Interface for LAPS (Local Administrator Password Solution)

## Version 1.6.0 breaking change

Version 1.6.0 adds multidomain support. Due this change the configurations changes. Please see `appsettings.json.example` and

## Setup Preqesites

### Setup Preqesites

- A working Active Directory with Microsoft LAPS installed
- .NET Core 8.0 compatible Operating System (Ubuntu/Debian/CentOS/Alpine Linux/Windows/macOS) or a Docker Host

### Bare Metal:

- Download the latest Release for your Platform
- Unzip Archive
- Rename `appsettings.json.example` to `appsettings.json` and edit as needed or set the settings via Environment Variables
- Ensure Python3 and dpapi-ng (`pip install dpapi-ng[kerberos]`) is installed
- Run *LAPS-WebUI*

### Notes for LAPS v2
- Since Version 1.5.0 LAPS v2 is supported
- By default, LAPS v2 passwords are encrypted. If the LAPS v2 passwords are stored unencrypted, then you have to set
  `EncryptionDisabled` to `true` in the settings
- When LAPS v2 Passwords are encrypted a direct connection to the domain controllers with `Kerberos` and `DCE-RPC` is needed in order to decrypt those passwords. For LAPS v1 and unecrypted LAPS v2 passwords only `LDAP` is needed

## Setup (docker):

Running LAPS-WebUI in docker is quite easy:
```
docker run -d \
  --name=lapswebui \
  -e Domains__0__Name=example.com \
  -e Domains__0__Ldap__Server=ldap.example.com \
  -e Domains__0__Ldap__Port=389 \
  -e Domains__0__Ldap__UseSSL=false \
  -e Domains__0__Ldap__TrustAllCertificates=true \
  -e Domains__0__Ldap__SearchBase='DC=example,DC=com' \
  -p 8080:8080 \
  --restart unless-stopped \
  ghcr.io/seji64/laps-webui:1.6.0
```
## Advanced configuration

### Change Listen Address and/or Port

There are a few options to configure this. [Here](https://andrewlock.net/exploring-the-dotnet-8-preview-updates-to-docker-images-in-dotnet-8/) is a quite good writeup with explains all options.


### Reverse Proxy
If you are using a reverse proxy ensure `WebSockets` are allowed / enabled

## Usage:
- Navigate with your WebBrowser to the LAPS WebUI Page (Default: http://127.0.0.1:8080 )
- Login with any Active Directory User which can access the LAPS LDAP Properties
- Type any Computername in the Searchbox and click a result to view the LAPS Password.

## Why is there no User Management?

There is none cause *LAPS-WebUI* authenticates against your ActiveDirectory. There is also defined who can read those LAPS Passwords.

## Screenshots:

![Screenshot](https://raw.githubusercontent.com/Seji64/LAPS-WebUI/master/screenshots/screenshot01.png)

![Screenshot](https://raw.githubusercontent.com/Seji64/LAPS-WebUI/master/screenshots/screenshot02.png)
