using System.Diagnostics;
using System.Runtime.InteropServices;

ApplicationConfiguration.Initialize();
Application.Run(new MainForm());

class MainForm : Form
{
    [DllImport("dwmapi.dll")]
    static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int value, int size);

    readonly Stopwatch watch = new();
    readonly System.Windows.Forms.Timer ticker = new() { Interval = 16 };
    readonly Label display = new();
    readonly Button toggle = new();
    readonly Button lap = new();
    readonly Button reset = new();
    readonly ListBox laps = new();

    static readonly Color Accent = Color.FromArgb(99, 102, 241);
    static readonly Color Danger = Color.FromArgb(220, 70, 80);
    static readonly Color Neutral = Color.FromArgb(44, 44, 50);

    public MainForm()
    {
        Text = "TimeKeeper";
        ClientSize = new Size(380, 520);
        MinimumSize = new Size(380, 520);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(18, 18, 20);
        ForeColor = Color.White;
        Font = new Font("Segoe UI", 10f);
        KeyPreview = true;
        DoubleBuffered = true;

        display.Text = Format(TimeSpan.Zero);
        display.Font = new Font("Segoe UI Light", 44f);
        display.Dock = DockStyle.Top;
        display.Height = 140;
        display.TextAlign = ContentAlignment.MiddleCenter;

        var row = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 72,
            ColumnCount = 3,
            RowCount = 1,
            Padding = new Padding(8, 0, 8, 0)
        };
        for (int i = 0; i < 3; i++) row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
        Style(toggle, "Start", Accent);
        Style(lap, "Lap", Neutral);
        Style(reset, "Reset", Neutral);
        row.Controls.Add(toggle, 0, 0);
        row.Controls.Add(lap, 1, 0);
        row.Controls.Add(reset, 2, 0);

        laps.Dock = DockStyle.Fill;
        laps.BorderStyle = BorderStyle.None;
        laps.BackColor = Color.FromArgb(24, 24, 28);
        laps.ForeColor = Color.FromArgb(210, 210, 220);
        laps.Font = new Font("Consolas", 12f);
        laps.IntegralHeight = false;

        Controls.Add(laps);
        Controls.Add(row);
        Controls.Add(display);

        ticker.Tick += (_, _) => display.Text = Format(watch.Elapsed);
        toggle.Click += (_, _) => Toggle();
        lap.Click += (_, _) => Lap();
        reset.Click += (_, _) => Reset();
        KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Space) Toggle();
            else if (e.KeyCode == Keys.L) Lap();
            else if (e.KeyCode == Keys.R) Reset();
            e.SuppressKeyPress = true;
        };
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        int on = 1;
        DwmSetWindowAttribute(Handle, 20, ref on, sizeof(int));
    }

    static void Style(Button b, string text, Color back)
    {
        b.Text = text;
        b.Dock = DockStyle.Fill;
        b.Margin = new Padding(6);
        b.FlatStyle = FlatStyle.Flat;
        b.FlatAppearance.BorderSize = 0;
        b.BackColor = back;
        b.ForeColor = Color.White;
        b.Cursor = Cursors.Hand;
        b.Font = new Font("Segoe UI Semibold", 11f);
    }

    static string Format(TimeSpan t) =>
        $"{(int)t.TotalMinutes:00}:{t.Seconds:00}.{t.Milliseconds / 10:00}";

    void Toggle()
    {
        if (watch.IsRunning)
        {
            watch.Stop();
            toggle.Text = "Resume";
            toggle.BackColor = Accent;
        }
        else
        {
            watch.Start();
            ticker.Start();
            toggle.Text = "Pause";
            toggle.BackColor = Danger;
        }
    }

    void Lap()
    {
        if (!watch.IsRunning) return;
        laps.Items.Insert(0, $"{laps.Items.Count + 1:00}    {Format(watch.Elapsed)}");
    }

    void Reset()
    {
        watch.Reset();
        laps.Items.Clear();
        display.Text = Format(TimeSpan.Zero);
        toggle.Text = "Start";
        toggle.BackColor = Accent;
    }
}
