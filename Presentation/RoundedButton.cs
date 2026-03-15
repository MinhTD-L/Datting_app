using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

public class RoundedButton : Button
{
    public int BorderRadius = 20;

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        GraphicsPath path = new GraphicsPath();
        int r = BorderRadius;

        path.AddArc(0, 0, r, r, 180, 90);
        path.AddArc(Width - r, 0, r, r, 270, 90);
        path.AddArc(Width - r, Height - r, r, r, 0, 90);
        path.AddArc(0, Height - r, r, r, 90, 90);
        path.CloseAllFigures();

        this.Region = new Region(path);
    }
}
