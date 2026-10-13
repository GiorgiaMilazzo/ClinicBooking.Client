# ClinicBooking.Client

## 1. Panoramica

Questo progetto è il frontend Blazor WebAssembly (.NET 10) per
[ClinicBooking.Api](https://github.com/GiorgiaMilazzo/ClinicBooking.Api), un'API
ASP.NET Core per la gestione di visite mediche.

Alla fine della Fase 1 il client permette di:

- leggere l'URL dell'API da `wwwroot/appsettings.json`;
- autenticare un utente tramite `POST api/Login`;
- salvare il token JWT nel `localStorage`;
- ricostruire nome, ruolo e identità dal payload del JWT;
- proteggere le pagine con `AuthorizeRouteView` e `[Authorize]`;
- mostrare nel menu contenuti diversi per utenti autenticati e anonimi;
- effettuare il logout;
- caricare e visualizzare gli appuntamenti;
- creare un appuntamento scegliendo medico e paziente dalle rispettive liste;
- mostrare date e ore nel formato esplicito `dd/MM/yyyy HH:mm`;
- mostrare gli stati `Prenotato`, `Confermato` e `Completato`;
- gestire errori HTTP, risposte `ProblemDetails` ed errori di connessione.

Screenshot da inserire:

- Home anonima;
- pagina di login;
- Home autenticata;
- pagina Appuntamenti con tabella;
- form di creazione appuntamento.

La Fase 2, che riguarderà gestione completa di medici, pazienti, stati e
cancellazioni, non è stata iniziata.

## 2. Perché frontend e backend sono separati

Frontend e backend sono due applicazioni indipendenti, ognuna nella sua cartella
e nella sua repository. Il backend è disponibile qui:
[ClinicBooking.Api](https://github.com/GiorgiaMilazzo/ClinicBooking.Api).
Le applicazioni si parlano solo tramite richieste HTTP e JSON.

Ho scelto questa struttura perché volevo provare un modo di costruire un'app,
diverso dal tenere tutto in un unico progetto. I vantaggi sono:

- frontend e backend possono essere sviluppati e modificati separatamente;
- lo stesso backend può servire un altro client, per esempio un'app mobile;
- le responsabilità dell'interfaccia e delle regole applicative restano
  separate.

Ci sono anche dei costi:

- bisogna avviare due progetti insieme;
- bisogna configurare il CORS nel backend;
- bisogna mantenere coerenti i contratti JSON tra client e API.

## 3. Come avviare backend e frontend

Il backend deve essere avviato sulla base URL:

```text
http://localhost:5035/
```

Dalla root del client:

```powershell
dotnet watch run --launch-profile http
```

Aprire poi l'indirizzo HTTP mostrato dal terminale. Se la porta dell'API
cambia, modificarla in `wwwroot/appsettings.json`:

```json
{
  "ApiBaseUrl": "http://localhost:5035/"
}
```

Per usare gli endpoint protetti serve un utente con ruolo `admin`. Ogni utente
appena registrato riceve il ruolo `doctor`, quindi riceve `403` sulle risorse
amministrative finché un admin non lo promuove manualmente nel database.

## 4. Struttura delle cartelle

### Root

- `Program.cs`: registra i servizi, la configurazione, l'autenticazione,
  `HttpClientFactory` e il client HTTP nominato `Api`.
- `App.razor`: avvolge l'app in `CascadingAuthenticationState` e usa
  `AuthorizeRouteView`.
- `_Imports.razor`: importa namespace condivisi dai componenti Razor.
- `ClinicBooking.Client.csproj`: target `net10.0` e pacchetti NuGet.
- `.gitignore`: generato con `dotnet new gitignore`.
- `README-Frontend.md`: documentazione del frontend.

### `Models/`

- `Appointment.cs`: modello dell'appuntamento e enum dello stato.
- `CreateAppointmentRequest.cs`: dati inviati per creare un appuntamento.
- `Doctor.cs`: modello del medico.
- `Patient.cs`: modello del paziente; `DateOfBirth` è `DateTime`, come nel
  backend.

### `Services/`

- `AuthorizationMessageHandler.cs`: legge il token dal `localStorage` e
  aggiunge l'header `Authorization: Bearer ...` alle richieste.
- `ApiAuthenticationStateProvider.cs`: decodifica il payload Base64Url del JWT,
  gestisce claim brevi e URI lunghi, controlla `exp` e costruisce il
  `ClaimsPrincipal`.
- `AuthService.cs`: esegue login, registrazione e logout e converte gli errori
  in messaggi leggibili.
- `AppointmentService.cs`: carica e crea appuntamenti.
- `DoctorService.cs`: carica i medici per la select.
- `PatientService.cs`: carica i pazienti per la select.
- `ApiResult.cs`: risultato comune con `Success`, `StatusCode`, dati e
  messaggio.
- `ApiErrorMessage.cs`: interpreta messaggi testuali e JSON `ProblemDetails`.

### `Pages/`

- `Home.razor`: mostra un invito all'accesso oppure nome e ruolo dell'utente.
- `Login.razor`: form di accesso con `EditForm`,
  `DataAnnotationsValidator`, stato di caricamento e `returnUrl`.
- `Appointments.razor`: pagina protetta con elenco e creazione degli
  appuntamenti.
- `NotFound.razor`: pagina per rotte inesistenti.

### `Layout/` e `Shared/`

- `MainLayout.razor`: layout principale senza i link demo del template.
- `NavMenu.razor`: menu condizionale con Appuntamenti, logout, Accedi e
  Registrati.
- `RedirectToLogin.razor`: reindirizza al login mantenendo la pagina richiesta
  in `returnUrl`.

### `wwwroot/`

- `appsettings.json`: configurazione dell'URL dell'API.
- asset statici e Bootstrap forniti dal template.

Le pagine Counter, Weather e i dati `sample-data` del template sono stati
rimossi.

## 5. Pacchetti NuGet

- `Blazored.LocalStorage 4.5.0`: accesso al `localStorage` del browser.
- `Microsoft.AspNetCore.Components.Authorization 10.0.12`: autenticazione
  Blazor, `AuthorizeView` e `AuthorizeRouteView`.
- `Microsoft.Extensions.Http 10.0.12`: `IHttpClientFactory` e client nominati.
- `Microsoft.AspNetCore.Components.WebAssembly 10.0.12`: runtime e componenti
  Blazor WebAssembly.
- `Microsoft.AspNetCore.Components.WebAssembly.DevServer 10.0.12`: server di
  sviluppo del template.

## 6. Teoria spiegata in modo semplice

### Blazor WebAssembly

Blazor WebAssembly scarica il runtime e l'applicazione nel browser ed esegue
il codice .NET sul client. Blazor Server esegue invece il codice sul server e
mantiene una connessione interattiva tra browser e server.

### Componenti Razor e Dependency Injection

Un componente `.razor` combina HTML e C#. `@page` assegna una rotta,
`@inject` richiede un servizio alla Dependency Injection e `@code` contiene
stato e metodi del componente.

### Form e validazione

`EditForm` gestisce i form e `DataAnnotationsValidator` controlla i dati
prima dell'invio. Queste annotazioni servono solo alla validazione lato
client: il server deve validare nuovamente ogni richiesta.

### HttpClient, HttpClientFactory e handler

`IHttpClientFactory` crea il client nominato `Api`. Un
`DelegatingHandler` viene eseguito per ogni richiesta e qui aggiunge il token
JWT senza obbligare ogni pagina a gestire manualmente l'header.

### JWT e localStorage

Il token viene salvato nel `localStorage` dopo il login. Il provider decodifica
il payload Base64Url, inclusi claim come `role`, `unique_name` e le varianti
con URI di `ClaimTypes`.

Il client non verifica la firma del JWT: questa non è una responsabilità sicura
del frontend. La validazione della firma e dei permessi viene eseguita dal
server.

### Stato di autenticazione

`AuthenticationStateProvider` rappresenta l'utente corrente.
`CascadingAuthenticationState` rende questo stato disponibile ai componenti
figli. `AuthorizeView` mostra markup diverso a seconda dell'accesso,
`AuthorizeRouteView` gestisce le rotte protette e `[Authorize]` protegge una
pagina.

### CORS ed errori

CORS è una regola applicata dal browser per richieste tra origini diverse.
L'API deve permettere l'origine del client.

- `400`: dati mancanti, non validi o regola di business non rispettata;
- `401`: token assente, non valido o sessione scaduta;
- `403`: utente autenticato ma senza il ruolo richiesto;
- errore di connessione: l'API è spenta, l'URL è errato o il browser blocca
  la richiesta.

Il client distingue questi casi e mostra messaggi comprensibili. Gli errori
`400` possono arrivare come testo semplice oppure come JSON `ProblemDetails`.

## 7. Flussi principali

### Login

1. L'utente apre `/login`.
2. Compila email e password.
3. `AuthService` invia `POST api/Login`.
4. L'API restituisce solo il token nel JSON.
5. Il token viene salvato nel `localStorage`.
6. Il provider notifica il nuovo stato autenticato.
7. Il menu viene aggiornato.
8. L'utente torna a `returnUrl`, oppure alla Home.

### Richiesta protetta

1. Una pagina chiama il relativo service.
2. `HttpClient` passa dal `AuthorizationMessageHandler`.
3. Il handler aggiunge il bearer token.
4. Il backend verifica token e ruolo.
5. Il service restituisce dati oppure `Success`, `StatusCode` e messaggio.
6. La pagina aggiorna la UI.

### Creazione appuntamento

1. La pagina carica medici e pazienti con `GET api/Doctors` e
   `GET api/Patients`.
2. L'utente li sceglie dalle select, senza digitare ID.
3. Inserisce la data e l'ora con `InputDateType.DateTimeLocal`.
4. Il client invia `POST api/Appointments`.
5. In caso di successo ricarica la lista.
6. In caso di errore mostra il messaggio del server.
7. Durante l'invio il pulsante è disabilitato per evitare duplicati.

## 8. Limiti noti e possibili miglioramenti

- Il token in `localStorage` è esposto a un eventuale attacco XSS; una possibile
  alternativa è usare cookie `httpOnly`.
- Non è presente un refresh token.
- Il controllo dei ruoli nella UI è solo estetico: il controllo di sicurezza
  vero viene eseguito dal server.
- Oggi i pazienti sono solo dati gestiti dall'admin e non possono accedere né
  prenotare da soli. Per permetterlo servirebbero modifiche al backend: un
  ruolo `patient`, il collegamento tra l'account utente e il record paziente e
  endpoint che restituiscono solo gli appuntamenti del paziente loggato,
  perché i permessi vengono decisi dal server e il frontend da solo non basta.

## 9. Errori incontrati

- Il comando `dotnet add package Blazored.LocalStorage` senza versione non ha
  trovato una versione disponibile. Ho risolto indicando esplicitamente la
  versione compatibile `4.5.0`.
