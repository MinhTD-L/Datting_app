using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

public class RoundedTextBox : UserControl
{
    private TextBox textBox = new TextBox();

    private int borderRadius = 18;
    private Color borderColor = Color.Gray;

    public RoundedTextBox()
    {
        this.BackColor = Color.White;
        this.Size = new Size(280, 40);

        textBox.BorderStyle = BorderStyle.None;
        textBox.Font = new Font("Segoe UI", 10);
        textBox.Location = new Point(12, 10);
        textBox.Width = this.Width - 24;

        this.Controls.Add(textBox);

        this.Resize += RoundedTextBox_Resize;
    }

    private void RoundedTextBox_Resize(object? sender, EventArgs e)
    {
        textBox.Width = this.Width - 24;
        textBox.Location = new Point(12, (this.Height - textBox.Height) / 2);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);

        GraphicsPath path = GetRoundedPath(rect, borderRadius);
        this.Region = new Region(path);

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

    public override string Text
    {
        get { return textBox.Text; }
        set { textBox.Text = value; }
    }

    public bool UseSystemPasswordChar
    {
        get { return textBox.UseSystemPasswordChar; }
        set { textBox.UseSystemPasswordChar = value; }
    }
}
