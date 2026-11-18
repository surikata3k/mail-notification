using System.ComponentModel;

namespace MailNotification;

public class AccountVm : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    public Account Model { get; }
    public HashSet<uint> Known { get; set; } = new();
    public bool Initialized { get; set; }

    public AccountVm(Account model)
    {
        Model = model;
        Loc.I.PropertyChanged += (_, _) => Refresh();
    }

    int _count; string? _error; DateTime? _last; bool _checking;
    public int Count { get => _count; set { _count = value; Refresh(); } }
    public string? Error { get => _error; set { _error = value; Refresh(); } }
    public DateTime? LastCheck { get => _last; set { _last = value; Refresh(); } }
    public bool Checking { get => _checking; set { _checking = value; Refresh(); } }

    public string Email => Model.Email;
    public bool HasUnread => Error == null && Count > 0;
    public string CountText => Error != null ? "!" : Count.ToString();
    public bool HasError => Error != null;
    public string StatusText
    {
        get
        {
            if (Error != null) return Loc.T(Error);
            if (Checking && LastCheck == null) return Loc.T("checking");
            var main = Count > 0 ? $"{Count} {Loc.T("unread")}" : Loc.T("noUnread");
            var when = LastCheck?.ToString("T") ?? Loc.T("never");
            return $"{main} · {Loc.T("lastCheck")}: {when}";
        }
    }

    public void Refresh() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
}
