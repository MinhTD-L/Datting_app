using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

public class RoundedPanel : Panel
{
    private int borderRadius = 18;

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);
        GraphicsPath path = GetRoundedPath(rect, borderRadius);

        // Bo góc cho Panel
        this.Region = new Region(path);

        // Vẽ đường viền đồng bộ với RoundedTextBox (độ dày 2f, màu 180)
        using (Pen pen = new Pen(Color.FromArgb(180, 180, 180), 2f))
        {
            g.DrawPath(pen, path);
        }
    }

    private GraphicsPath GetRoundedPath(Rectangle rect, int radius)
    {
        GraphicsPath path = new GraphicsPath();
        path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
        path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
        path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
        path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
        path.CloseFigure();
        return path;
    }
}