using System.ComponentModel;

namespace MailNotification;

/// <summary>Textos en català, castellà i anglès. S'hi enllaça des de XAML amb {Binding [clau], Source={x:Static l:Loc.I}}.</summary>
public class Loc : INotifyPropertyChanged
{
    public static readonly Loc I = new();
    public event PropertyChangedEventHandler? PropertyChanged;

    string _lang = "ca";
    public string Language
    {
        get => _lang;
        set
        {
            if (!All.ContainsKey(value)) value = "ca";
            _lang = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Language)));
        }
    }

    public string this[string key] => T(key);
    public static string T(string key) =>
        All[I._lang].TryGetValue(key, out var v) ? v : (All["ca"].TryGetValue(key, out var d) ? d : key);

    static readonly Dictionary<string, Dictionary<string, string>> All = new()
    {
        ["ca"] = new()
        {
            ["accounts"] = "Comptes", ["settings"] = "Configuració", ["addAccount"] = "Afegeix un compte",
            ["checkNow"] = "Comprova ara", ["openGmail"] = "Obre Gmail", ["edit"] = "Edita", ["remove"] = "Elimina",
            ["unread"] = "sense llegir", ["noUnread"] = "Cap correu nou", ["checking"] = "Comprovant…",
            ["lastCheck"] = "Última comprovació", ["never"] = "mai",
            ["noAccounts"] = "Encara no hi ha cap compte. Afegeix-ne un per començar.",
            ["email"] = "Adreça de correu", ["appPassword"] = "Contrasenya d'aplicació",
            ["appPasswordKeep"] = "Deixa-ho buit per mantenir l'actual",
            ["appPasswordHelp"] = "Cal tenir la verificació en dos passos activada. Crea la contrasenya a myaccount.google.com/apppasswords",
            ["notifyType"] = "Tipus de notificació", ["notifyNone"] = "Cap",
            ["notifyBadge"] = "Comptador a la icona de l'app", ["notifyToast"] = "Notificació al centre de notificacions",
            ["save"] = "Desa", ["cancel"] = "Cancel·la", ["verifying"] = "Verificant el compte…",
            ["confirmRemove"] = "Vols eliminar aquest compte?",
            ["language"] = "Idioma", ["interval"] = "Comprova cada", ["minutes"] = "minuts",
            ["autostart"] = "Obre l'aplicació en iniciar la sessió", ["startMinimized"] = "Inicia minimitzada a la safata",
            ["minimizeToTray"] = "Minimitza i tanca a la safata del sistema (al costat de l'hora)",
            ["imapHelp"] = "Es fa servir IMAP (imap.gmail.com). A Google Workspace l'administrador ha de tenir IMAP permès.",
            ["trayOpen"] = "Obre", ["trayCheck"] = "Comprova ara", ["trayQuit"] = "Surt",
            ["newMail"] = "Correu nou", ["newMailMany"] = "correus nous",
            ["errAuth"] = "Usuari o contrasenya d'aplicació incorrectes", ["errGeneric"] = "Error de connexió",
            ["errEmail"] = "Adreça de correu no vàlida", ["errPass"] = "Cal la contrasenya d'aplicació",
            ["errDup"] = "Aquest compte ja existeix",
            ["authMethod"] = "Mètode d'accés", ["methodPassword"] = "Contrasenya d'aplicació", ["methodOAuth"] = "Inicia sessió amb Google (OAuth)", ["oauthHelp"] = "S'obrirà el navegador perquè iniciïs sessió amb aquest compte de Google i donis permís a l'app. Cal configurar abans el client OAuth a Configuració.",
            ["oauthClient"] = "Client OAuth de Google", ["oauthClientHelp"] = "Per a l'inici de sessió amb Google, crea un «ID de client OAuth» de tipus «Aplicació d'escriptori» a console.cloud.google.com i enganxa aquí l'ID i el secret.", ["errOAuthMissing"] = "Cal configurar el client OAuth a Configuració",
        },
        ["es"] = new()
        {
            ["accounts"] = "Cuentas", ["settings"] = "Configuración", ["addAccount"] = "Añadir una cuenta",
            ["checkNow"] = "Comprobar ahora", ["openGmail"] = "Abrir Gmail", ["edit"] = "Editar", ["remove"] = "Eliminar",
            ["unread"] = "sin leer", ["noUnread"] = "Ningún correo nuevo", ["checking"] = "Comprobando…",
            ["lastCheck"] = "Última comprobación", ["never"] = "nunca",
            ["noAccounts"] = "Todavía no hay ninguna cuenta. Añade una para empezar.",
            ["email"] = "Dirección de correo", ["appPassword"] = "Contraseña de aplicación",
            ["appPasswordKeep"] = "Déjalo vacío para mantener la actual",
            ["appPasswordHelp"] = "Hay que tener activada la verificación en dos pasos. Crea la contraseña en myaccount.google.com/apppasswords",
            ["notifyType"] = "Tipo de notificación", ["notifyNone"] = "Ninguna",
            ["notifyBadge"] = "Contador en el icono de la app", ["notifyToast"] = "Notificación en el centro de notificaciones",
            ["save"] = "Guardar", ["cancel"] = "Cancelar", ["verifying"] = "Verificando la cuenta…",
            ["confirmRemove"] = "¿Quieres eliminar esta cuenta?",
            ["language"] = "Idioma", ["interval"] = "Comprobar cada", ["minutes"] = "minutos",
            ["autostart"] = "Abrir la aplicación al iniciar la sesión", ["startMinimized"] = "Iniciar minimizada en la bandeja",
            ["minimizeToTray"] = "Minimizar y cerrar en la bandeja del sistema (junto al reloj)",
            ["imapHelp"] = "Se usa IMAP (imap.gmail.com). En Google Workspace el administrador debe tener IMAP permitido.",
            ["trayOpen"] = "Abrir", ["trayCheck"] = "Comprobar ahora", ["trayQuit"] = "Salir",
            ["newMail"] = "Correo nuevo", ["newMailMany"] = "correos nuevos",
            ["errAuth"] = "Usuario o contraseña de aplicación incorrectos", ["errGeneric"] = "Error de conexión",
            ["errEmail"] = "Dirección de correo no válida", ["errPass"] = "Hace falta la contraseña de aplicación",
            ["errDup"] = "Esta cuenta ya existe",
            ["authMethod"] = "Método de acceso", ["methodPassword"] = "Contraseña de aplicación", ["methodOAuth"] = "Iniciar sesión con Google (OAuth)", ["oauthHelp"] = "Se abrirá el navegador para que inicies sesión con esta cuenta de Google y des permiso a la app. Antes hay que configurar el cliente OAuth en Configuración.",
            ["oauthClient"] = "Cliente OAuth de Google", ["oauthClientHelp"] = "Para iniciar sesión con Google, crea un «ID de cliente OAuth» de tipo «Aplicación de escritorio» en console.cloud.google.com y pega aquí el ID y el secreto.", ["errOAuthMissing"] = "Hay que configurar el cliente OAuth en Configuración",
        },
        ["en"] = new()
        {
            ["accounts"] = "Accounts", ["settings"] = "Settings", ["addAccount"] = "Add account",
            ["checkNow"] = "Check now", ["openGmail"] = "Open Gmail", ["edit"] = "Edit", ["remove"] = "Remove",
            ["unread"] = "unread", ["noUnread"] = "No new mail", ["checking"] = "Checking…",
            ["lastCheck"] = "Last check", ["never"] = "never",
            ["noAccounts"] = "No accounts yet. Add one to get started.",
            ["email"] = "Email address", ["appPassword"] = "App password",
            ["appPasswordKeep"] = "Leave empty to keep the current one",
            ["appPasswordHelp"] = "2-Step Verification must be on. Create the password at myaccount.google.com/apppasswords",
            ["notifyType"] = "Notification type", ["notifyNone"] = "None",
            ["notifyBadge"] = "Counter on the app icon", ["notifyToast"] = "Notification in the notification center",
            ["save"] = "Save", ["cancel"] = "Cancel", ["verifying"] = "Verifying account…",
            ["confirmRemove"] = "Remove this account?",
            ["language"] = "Language", ["interval"] = "Check every", ["minutes"] = "minutes",
            ["autostart"] = "Open the app when I sign in", ["startMinimized"] = "Start minimized to the tray",
            ["minimizeToTray"] = "Minimize and close to the system tray (next to the clock)",
            ["imapHelp"] = "IMAP (imap.gmail.com) is used. On Google Workspace the administrator must allow IMAP.",
            ["trayOpen"] = "Open", ["trayCheck"] = "Check now", ["trayQuit"] = "Quit",
            ["newMail"] = "New mail", ["newMailMany"] = "new emails",
            ["errAuth"] = "Wrong username or app password", ["errGeneric"] = "Connection error",
            ["errEmail"] = "Invalid email address", ["errPass"] = "App password required",
            ["errDup"] = "This account already exists",
            ["authMethod"] = "Access method", ["methodPassword"] = "App password", ["methodOAuth"] = "Sign in with Google (OAuth)", ["oauthHelp"] = "Your browser will open so you can sign in with this Google account and grant access. Set up the OAuth client in Settings first.",
            ["oauthClient"] = "Google OAuth client", ["oauthClientHelp"] = "For Google sign-in, create an “OAuth client ID” of type “Desktop app” at console.cloud.google.com and paste the ID and secret here.", ["errOAuthMissing"] = "The OAuth client must be set up in Settings",
        },
    };
}
