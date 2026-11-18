using System.Windows;

namespace MailNotification;

public partial class App : Application
{
    Mutex? _mutex;
    EventWaitHandle? _showEvent;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        ShutdownMode = ShutdownMode.OnExplicitShutdown;

        _mutex = new Mutex(true, "MailNotification.SingleInstance", out var first);
        _showEvent = new EventWaitHandle(false, EventResetMode.AutoReset, "MailNotification.Show");
        if (!first)
        {
            _showEvent.Set(); // avisa la instància que ja s'executa perquè es mostri
            Shutdown();
            return;
        }

        var controller = new Controller();
        controller.Start(hidden: e.Args.Contains("--hidden"));

        new Thread(() =>
        {
            while (true) { _showEvent.WaitOne(); Dispatcher.Invoke(controller.ShowWindow); }
        }) { IsBackground = true }.Start();
    }
}
