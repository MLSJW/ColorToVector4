using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ColorToVector4
{
    internal static class Program
    {


        public class ColorToVector4 : Form
        {
            private readonly TrackBar rBar = new TrackBar { Minimum = 0, Maximum = 255, Value = 255, TickStyle = TickStyle.None, Width = 280 };
            private readonly TrackBar gBar = new TrackBar { Minimum = 0, Maximum = 255, Value = 128, TickStyle = TickStyle.None, Width = 280 };
            private readonly TrackBar bBar = new TrackBar { Minimum = 0, Maximum = 255, Value = 0, TickStyle = TickStyle.None, Width = 280 };
            private readonly TrackBar aBar = new TrackBar { Minimum = 0, Maximum = 100, Value = 100, TickStyle = TickStyle.None, Width = 280 };

            private readonly Label rVal = new Label { Width = 50, TextAlign = ContentAlignment.MiddleRight, ForeColor = Color.Gainsboro };
            private readonly Label gVal = new Label { Width = 50, TextAlign = ContentAlignment.MiddleRight, ForeColor = Color.Gainsboro };
            private readonly Label bVal = new Label { Width = 50, TextAlign = ContentAlignment.MiddleRight, ForeColor = Color.Gainsboro };
            private readonly Label aVal = new Label { Width = 50, TextAlign = ContentAlignment.MiddleRight, ForeColor = Color.Gainsboro };

            private readonly Panel preview = new Panel { Height = 100, Dock = DockStyle.Top };
            private readonly Label hexLabel = new Label { Dock = DockStyle.Bottom, Height = 20, TextAlign = ContentAlignment.MiddleCenter, ForeColor = Color.Black };
            private readonly TextBox output = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                Height = 70,
                Dock = DockStyle.Bottom,
                BackColor = Color.FromArgb(17, 19, 24),
                ForeColor = Color.FromArgb(126, 231, 135),
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Consolas", 10f)
            };
            private readonly Button copyBtn = new Button { Text = "Copy", Dock = DockStyle.Bottom, Height = 30 };

            public ColorToVector4()
            {
                Text = "Color To Vector4";
                ClientSize = new Size(360, 470);
                BackColor = Color.FromArgb(28, 31, 38);
                ForeColor = Color.Gainsboro;
                Font = new Font("Segoe UI", 9f);
                FormBorderStyle = FormBorderStyle.FixedSingle;
                MaximizeBox = false;

                preview.Paint += Preview_Paint;
                preview.Controls.Add(hexLabel);
                hexLabel.BackColor = Color.Transparent;

                var layout = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 3,
                    RowCount = 5,
                    Padding = new Padding(12)
                };
                layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 24));
                layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 54));
                for (int i = 0; i < 5; i++) layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));

                AddRow(layout, 0, "R", rBar, rVal, Color.Red);
                AddRow(layout, 1, "G", gBar, gVal, Color.Lime);
                AddRow(layout, 2, "B", bBar, bVal, Color.Blue);
                AddRow(layout, 3, "A", aBar, aVal, Color.White);

                var outPanel = new Panel { Dock = DockStyle.Bottom, Height = 110 };
                outPanel.Controls.Add(output);
                outPanel.Controls.Add(copyBtn);
                output.Dock = DockStyle.Fill;

                Controls.Add(layout);
                Controls.Add(outPanel);
                Controls.Add(preview);

                rBar.ValueChanged += (_, __) => UpdateAll();
                gBar.ValueChanged += (_, __) => UpdateAll();
                bBar.ValueChanged += (_, __) => UpdateAll();
                aBar.ValueChanged += (_, __) => UpdateAll();

                copyBtn.Click += (_, __) =>
                {
                    if (output.Text.Length > 0)
                    {
                        Clipboard.SetText(output.Text);
                        copyBtn.Text = "Copied";
                        var t = new Timer { Interval = 900 };
                        t.Tick += (s, e) => { copyBtn.Text = "Copy"; t.Stop(); t.Dispose(); };
                        t.Start();
                    }
                };

                UpdateAll();
            }

            private void AddRow(TableLayoutPanel layout, int row, string name, TrackBar bar, Label val, Color tint)
            {
                var lbl = new Label { Text = name, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, ForeColor = Color.FromArgb(124, 135, 152), Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
                bar.Dock = DockStyle.Fill;
                bar.BackColor = Blend(Color.FromArgb(44, 49, 60), tint, 0.15f);
                val.Text = bar.Value.ToString();
                layout.Controls.Add(lbl, 0, row);
                layout.Controls.Add(bar, 1, row);
                layout.Controls.Add(val, 2, row);
            }

            private static Color Blend(Color a, Color b, float t) =>
                Color.FromArgb(
                    (int)(a.R + (b.R - a.R) * t),
                    (int)(a.G + (b.G - a.G) * t),
                    (int)(a.B + (b.B - a.B) * t));

            private void UpdateAll()
            {
                int r = rBar.Value, g = gBar.Value, b = bBar.Value;
                float a = aBar.Value / 100f;

                rVal.Text = r.ToString();
                gVal.Text = g.ToString();
                bVal.Text = b.ToString();
                aVal.Text = a.ToString("0.00");

                float vr = r / 255f, vg = g / 255f, vb = b / 255f;

                string vec = $"Vector4({Trim(vr)}f, {Trim(vg)}f, {Trim(vb)}f, {Trim(a)}f)";
                int aByte = (int)Math.Round(a * 255);
                string hex = string.Format(
                    System.Globalization.CultureInfo.InvariantCulture,
                    "#{0:X2}{1:X2}{2:X2}{3:X2}", r, g, b, aByte);
                output.Text = vec + Environment.NewLine + hex;

                preview.Invalidate();
                hexLabel.Text = hex;

                double lum = (0.299 * r + 0.587 * g + 0.114 * b) / 255.0;
                hexLabel.ForeColor = lum > 0.6 ? Color.Black : Color.White;
            }

            private static string Trim(float v)
            {
                string s = v.ToString("F3", System.Globalization.CultureInfo.InvariantCulture);
                s = s.TrimEnd('0');
                if (s.EndsWith(".")) s += "0";
                return s;
            }

            private void Preview_Paint(object sender, PaintEventArgs e)
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                var rect = new Rectangle(4, 4, preview.Width - 8, preview.Height - 8);

                using (var path = RoundedRect(rect, 8))
                {
                    using (var brush = new SolidBrush(Color.FromArgb(aBar.Value * 255 / 100, rBar.Value, gBar.Value, bBar.Value)))
                        g.FillPath(brush, path);
                    using (var pen = new Pen(Color.FromArgb(44, 49, 60)))
                        g.DrawPath(pen, path);
                }
            }

            private static GraphicsPath RoundedRect(Rectangle r, int radius)
            {
                var p = new GraphicsPath();
                int d = radius * 2;
                p.AddArc(r.X, r.Y, d, d, 180, 90);
                p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
                p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
                p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
                p.CloseFigure();
                return p;
            }
            [STAThread]
            static void Main() => Application.Run(new ColorToVector4());
        }
    }
}
