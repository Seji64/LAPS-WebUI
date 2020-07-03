# LAPS-WebUI
A nice and simple Web Interface for LAPS (Local Administrator Password Solution)

## Setup Preqesites

- A working Active Directory with Microsoft LAPS installed

## Setup (bare metal):

- Download the latest Release for your Platform
- Unzip Archive
- Copy `settings.json.example` and renamed it to `settings.json`
- Adjust settings.json and run LAPS WebUI

## Setup (docker):

Running LAPS-WebUI in docker is quite easy:
```
docker run -d \
  --name=lapswebui \
  -e LAPS_ListenAddress=0.0.0.0 \
  -e LAPS_ListenPort=8080 \
  -e LAPS_LDAP__Server=ldap.example.com \
  -e LAPS_LDAP__Port=389 \
  -e LAPS_LDAP__UseSSL=false \
  -e LAPS_LDAP__SearchBase=DC=example,DC=com \
  -p 8080:8080 \
  --restart unless-stopped \
  seji/laps-webui
```

## Usage:
- Navigate with your WebBrowser to the LAPS WebUI Page (Default: http://127.0.0.1:8080 )
- Login with any Active Directory User which can access the LAPS LDAP Properties
- Type any Computername in the Searchbox and click a result to view the LAPS Password.

## Screenshots:

![Screenshot](https://raw.githubusercontent.com/Seji64/LAPS-WebUI/master/screenshots/screenshot01.png)

![Screenshot](https://raw.githubusercontent.com/Seji64/LAPS-WebUI/master/screenshots/screenshot02.png)

![Screenshot](https://raw.githubusercontent.com/Seji64/LAPS-WebUI/master/screenshots/screenshot03.png)
