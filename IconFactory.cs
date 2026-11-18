using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using WM = System.Windows.Media;

namespace MailNotification;

/// <summary>Dibuixa la icona de la safata i la insígnia de la barra de tasques amb el comptador.</summary>
public static class IconFactory
{
    [DllImport("user32.dll")] static extern bool DestroyIcon(IntPtr h);

    /// <summary>Icona per a la safata del sistema (32x32). Amb n &gt; 0 hi afegeix un cercle vermell amb el número.</summary>
    public static Icon Tray(int n)
    {
        using var bmp = new Bitmap(32, 32);
        using (var g = Graphics.FromImage(bmp))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            using var bg = new SolidBrush(System.Drawing.Color.FromArgb(37, 99, 235));
            g.FillPath(bg, Round(1, 1, 30, 30, 7));
            g.FillPath(System.Drawing.Brushes.White, Round(5, 9, 22, 15, 2));
            using var pen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(37, 99, 235), 1.6f);
            g.DrawLines(pen, new[] { new PointF(6, 11), new PointF(16, 18), new PointF(26, 11) });
            if (n > 0)
            {
                g.FillEllipse(System.Drawing.Brushes.Red, 13, -1, 20, 20);
                var txt = n > 99 ? "99" : n.ToString();
                using var f = new Font("Segoe UI", txt.Length > 1 ? 8f : 10f, System.Drawing.FontStyle.Bold, GraphicsUnit.Pixel);
                var sz = g.MeasureString(txt, f);
                g.DrawString(txt, f, System.Drawing.Brushes.White, 23 - sz.Width / 2, 9 - sz.Height / 2);
            }
        }
        var h = bmp.GetHicon();
        try { return (Icon)Icon.FromHandle(h).Clone(); }
        finally { DestroyIcon(h); }
    }

    static GraphicsPath Round(float x, float y, float w, float h, float r)
    {
        var p = new GraphicsPath(); float d = r * 2;
        p.AddArc(x, y, d, d, 180, 90); p.AddArc(x + w - d, y, d, d, 270, 90);
        p.AddArc(x + w - d, y + h - d, d, d, 0, 90); p.AddArc(x, y + h - d, d, d, 90, 90);
        p.CloseFigure(); return p;
    }

    /// <summary>Insígnia (overlay) per a la icona de la barra de tasques.</summary>
    public static WM.ImageSource? Overlay(int n)
    {
        if (n <= 0) return null;
        var dv = new WM.DrawingVisual();
        using (var dc = dv.RenderOpen())
        {
            dc.DrawEllipse(WM.Brushes.Red, null, new System.Windows.Point(16, 16), 16, 16);
            var txt = n > 99 ? "99+" : n.ToString();
            var ft = new WM.FormattedText(txt, System.Globalization.CultureInfo.InvariantCulture, System.Windows.FlowDirection.LeftToRight,
                new WM.Typeface("Segoe UI Semibold"), txt.Length > 2 ? 14 : txt.Length > 1 ? 18 : 22,
                WM.Brushes.White, 1.0);
            dc.DrawText(ft, new System.Windows.Point(16 - ft.Width / 2, 16 - ft.Height / 2));
        }
        var rtb = new RenderTargetBitmap(32, 32, 96, 96, WM.PixelFormats.Pbgra32);
        rtb.Render(dv);
        rtb.Freeze();
        return rtb;
    }
}
