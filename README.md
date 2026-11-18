# mail-notification

Aplicació de Windows 11 que avisa de correu nou a diversos comptes de Gmail i de Google Workspace, i permet obrir la safata d'entrada del compte corresponent amb un clic.

## Característiques

- Interfície visual amb la llista de comptes i el nombre de correus sense llegir de cadascun.
- Diversos comptes de Gmail i de Google Workspace.
- Tipus de notificació configurable per compte:
  - **Cap**
  - **Comptador a la icona** de l'app (barra de tasques i safata del sistema), amb el total de tots els comptes marcats.
  - **Notificació** al centre de notificacions, amb remitent i assumpte.
- Botó **Obre Gmail** que obre la safata d'entrada del compte correcte al navegador.
- Es minimitza i es tanca a la **safata del sistema**, al costat de l'hora.
- Pot **obrir-se en iniciar la sessió**, opcionalment minimitzada.
- Interval de comprovació configurable.
- Idioma configurable: català, castellà i anglès.

Només es consulta la safata d'entrada (correus sense llegir). L'app no marca res com a llegit.

## Requisits

- Windows 11
- [SDK de .NET 8](https://dotnet.microsoft.com/download/dotnet/8.0), per compilar-la

## Compilar i executar

```bash
git clone https://github.com/surikata3k/mail-notification.git
cd mail-notification
dotnet run
```

Per generar un executable autònom:

```bash
dotnet publish -c Release -r win-x64 --self-contained -o publish
```

## Afegir un compte

L'app es connecta per IMAP (`imap.gmail.com`) i admet dues maneres d'accedir al compte.

### A) Contrasenya d'aplicació

És la més senzilla.

1. Activa la **verificació en dos passos** al compte de Google.
2. Crea una contrasenya d'aplicació a <https://myaccount.google.com/apppasswords>. És un codi de 16 lletres.
3. A l'app, tria **Afegeix un compte**, escriu el correu i la contrasenya d'aplicació.

Cada compte necessita la seva pròpia contrasenya. A Google Workspace, l'administrador ha de tenir permesos IMAP i les contrasenyes d'aplicació.

### B) Inici de sessió amb Google (OAuth)

Útil quan l'administrador del domini no permet contrasenyes d'aplicació. L'app obre el navegador perquè iniciïs sessió i acceptis el permís. Primer cal crear un client OAuth, una sola vegada:

1. A <https://console.cloud.google.com> crea un projecte nou.
2. Ves a **APIs i serveis → Pantalla de consentiment d'OAuth** i configura-la amb públic **Extern**.
3. Afegeix els comptes que vulguis fer servir com a **usuaris de prova**. En mode prova, l'autorització caduca als 7 dies i cal tornar a iniciar sessió.
4. A **Credencials**, crea un **ID de client OAuth** de tipus **Aplicació d'escriptori**.
5. A l'app, a **Configuració → Client OAuth de Google**, enganxa l'ID i el secret del client.
6. Afegeix el compte triant **Inicia sessió amb Google (OAuth)**.

Quan Google avisi que l'aplicació no està verificada, és normal: tria **Configuració avançada** i continua. Si l'administrador del domini bloqueja les aplicacions de tercers, aquest mètode tampoc funcionarà.

## On es desen les dades

Les dades de l'app es desen fora del repositori, a `%APPDATA%\MailNotification\settings.json`. Les contrasenyes d'aplicació, els tokens d'OAuth i el secret del client es xifren amb el xifratge de Windows (DPAPI), de manera que només els pot llegir el mateix usuari de Windows.

Aquest repositori no conté cap credencial, i el `.gitignore` bloqueja fitxers com `settings.json`, `client_secret*.json`, `*.pfx` i `.env*`. No pugis mai aquests fitxers.

## Estructura

| Fitxer | Funció |
|---|---|
| `Controller.cs` | Comptes, comprovació periòdica, safata, comptador i inici automàtic |
| `MailChecker.cs` | Consulta IMAP de la safata d'entrada |
| `OAuthService.cs` | Inici de sessió amb Google |
| `Models.cs` | Configuració i xifratge de secrets |
| `IconFactory.cs` | Icona de la safata i insígnia del comptador |
| `Loc.cs` | Textos en català, castellà i anglès |
| `MainWindow.xaml`, `AccountDialog.xaml` | Interfície |
| `scripts/make-icon.ps1` | Genera la icona `Assets/app.ico` |

## Llicència

Vegeu [LICENSE](LICENSE).
