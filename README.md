# LAPS-WebUI

A simple web interface for Microsoft LAPS (Local Administrator Password Solution).

---

## 📚 About

This is a modern frontend for Microsoft LAPS, supporting:
- LAPS v1 and v2
- Multiple Active Directory domains
- Authentication directly via Active Directory
- Bare-metal and Docker deployment

No additional user management is needed — access is fully controlled by Active Directory permissions.

---

## ⚠️ Version 1.6.0 Notice

> Starting with version 1.6.0, multi-domain support was added.  
> As a result, the configuration format has changed.  
> Review the updated `appsettings.json.example` for details and adjust your setup accordingly.

---

## 🛠 Requirements

- Active Directory with Microsoft LAPS installed
- .NET 9 runtime or a Docker host
- Python 3 with `dpapi-ng` installed:
  ```bash
  pip install dpapi-ng[kerberos]

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
  ghcr.io/seji64/laps-webui:1.6
```

## ⚙️ Advanced Configuration
- Listen address and port: [Learn more](https://andrewlock.net/exploring-the-dotnet-8-preview-updates-to-docker-images-in-dotnet-8/)
- Behind a reverse proxy: WebSocket support must be enabled!


## 🧑‍💻 Usage
 - Access the app at: http://127.0.0.1:8080
 - Authenticate with your Active Directory user credentials
 - Search for a computer by its name
 - Click on the result to display the LAPS-managed password

## ❓ FAQ
### Why is there no user management?
Authentication and authorization are fully handled by Active Directory.

### What LAPS versions are supported?
Both Microsoft LAPS v1 (legacy) and LAPS v2 (modern) are supported.

## Screenshots:

![Screenshot](https://raw.githubusercontent.com/Seji64/LAPS-WebUI/master/screenshots/screenshot01.png)

![Screenshot](https://raw.githubusercontent.com/Seji64/LAPS-WebUI/master/screenshots/screenshot02.png)
