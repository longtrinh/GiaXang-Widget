using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using Microsoft.Win32;
using System.Data;
using System.Diagnostics;

namespace PetrolimexWidget
{
    public partial class Form1 : Form
    {
        // For making the borderless form draggable
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        // --- Windows API to forcefully flush unused RAM to disk ---
        [System.Runtime.InteropServices.DllImport("psapi.dll")]
        static extern int EmptyWorkingSet(IntPtr hwProc);

        static readonly IntPtr HWND_BOTTOM = new IntPtr(1);
        const uint SWP_NOMOVE = 0x0002;
        const uint SWP_NOSIZE = 0x0001;
        const uint SWP_NOACTIVATE = 0x0010;

        // The icon in the system tray
        private NotifyIcon trayIcon;
        // The right-click menu for the icon
        private ContextMenuStrip trayMenu;

        // The invisible Edge browser engine
        private WebView2 webView;
        private System.Windows.Forms.Timer scheduleTimer;
        private DateTime lastScheduledUpdate = DateTime.MinValue;

        public Form1()
        {
            InitializeComponent();

            timerRefresh.Start();

            SetupSystemTray();

            this.Opacity = 0.8;

            CheckAndSetStartup();

            this.ShowInTaskbar = false;

            this.StartPosition = FormStartPosition.Manual;
            if (Properties.Settings.Default.WidgetLocation != new System.Drawing.Point(0, 0))
            {
                this.Location = Properties.Settings.Default.WidgetLocation;
            }

            dgvPrices.AllowUserToResizeColumns = false;
            dgvPrices.AllowUserToResizeRows = false;
            dgvPrices.SelectionChanged += (s, e) => dgvPrices.ClearSelection();

            this.MouseDown += Form1_MouseDown;
            lblTitle.MouseDown += LblTitle_MouseDown;

            // Event subscriptions
            this.DoubleClick += CloseWidget_DoubleClick;
            dgvPrices.DoubleClick += CloseWidget_DoubleClick;

            // Push to the back immediately after starting
            this.Shown += (s, e) => SetWindowPos(this.Handle, HWND_BOTTOM, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);

            // --- Start the schedule checker ---
            SetupScheduleTimer();

            // Initial browser load
            InitializeBrowser();

            // Instead of resuming, we check if the browser is null and rebuild it
            timerRefresh.Tick += (s, e) =>
            {
                if (webView == null)
                {
                    InitializeBrowser();
                }
            };
        }

        private void LblTitle_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (e.Clicks == 2)
                {
                    // Double-click: trigger force update instead of dragging
                    ForceUpdate_DoubleClick(sender, e);
                }
                else
                {
                    // Single click: drag the window as before
                    ReleaseCapture();
                    SendMessage(Handle, WM_NCLBUTTONDOWN, (IntPtr)HT_CAPTION, IntPtr.Zero);
                }
            }
        }

        // --- New Method: Force Update when Double Clicking Title ---
        private void ForceUpdate_DoubleClick(object sender, EventArgs e)
        {
            lblTitle.Text = "Manual update requested...";

            if (webView != null)
            {
                this.Controls.Remove(webView);
                webView.Dispose();
                webView = null;
            }

            InitializeBrowser();
        }

        private void SetupScheduleTimer()
        {
            scheduleTimer = new System.Windows.Forms.Timer();
            scheduleTimer.Interval = 30000; // Check the time every 30 seconds
            scheduleTimer.Tick += ScheduleTimer_Tick;
            scheduleTimer.Start();
        }

        private void ScheduleTimer_Tick(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;

            // Check if current time matches target times
            bool isTargetTime = (now.Hour == 0 && now.Minute == 1) ||
                                (now.Hour == 15 && now.Minute == 1) ||
                                (now.Hour == 15 && now.Minute == 31);

            // If it's a target time AND we haven't already updated in the last 60 seconds
            if (isTargetTime && (now - lastScheduledUpdate).TotalMinutes >= 1)
            {
                lastScheduledUpdate = now;

                // Ensure the previous browser instance was cleaned up before starting a new one
                if (webView == null)
                {
                    InitializeBrowser();
                }
                else
                {
                    // Force cleanup if it's stuck from a previous failed load
                    this.Controls.Remove(webView);
                    webView.Dispose();
                    webView = null;

                    InitializeBrowser();
                }
            }
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            SetWindowPos(this.Handle, HWND_BOTTOM, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
        }

        private async void InitializeBrowser()
        {
            lblTitle.Text = "Initializing browser engine...";

            webView = new WebView2 { Visible = false };
            this.Controls.Add(webView);

            await webView.EnsureCoreWebView2Async(null);

            await webView.CoreWebView2.Profile.ClearBrowsingDataAsync();

            // Block YouTube subframe process entirely
            webView.CoreWebView2.FrameCreated += (sender, args) =>
            {
                args.Frame.NavigationStarting += (frameSender, frameArgs) =>
                {
                    if (frameArgs.Uri.Contains("youtube.com") ||
                        frameArgs.Uri.Contains("ytimg.com") ||
                        frameArgs.Uri.Contains("youtu.be"))
                    {
                        frameArgs.Cancel = true;
                    }
                };
            };

            // Block Unnecessary Resources to speed up load time and save RAM
            webView.CoreWebView2.AddWebResourceRequestedFilter("*youtube.com*", CoreWebView2WebResourceContext.All);
            webView.CoreWebView2.AddWebResourceRequestedFilter("*ytimg.com*", CoreWebView2WebResourceContext.All);
            webView.CoreWebView2.AddWebResourceRequestedFilter("*", CoreWebView2WebResourceContext.Image);
            webView.CoreWebView2.AddWebResourceRequestedFilter("*", CoreWebView2WebResourceContext.Media);
            webView.CoreWebView2.AddWebResourceRequestedFilter("*", CoreWebView2WebResourceContext.Stylesheet); // Added
            webView.CoreWebView2.AddWebResourceRequestedFilter("*", CoreWebView2WebResourceContext.Font);       // Added

            webView.CoreWebView2.WebResourceRequested += (sender, args) =>
            {
                args.Response = webView.CoreWebView2.Environment.CreateWebResourceResponse(null, 403, "Blocked", "");
            };

            webView.CoreWebView2.NavigationCompleted += WebView_NavigationCompleted;

            lblTitle.Text = "Connecting to Petrolimex...";
            // Append a timestamp to the URL to force a fresh fetch and bypass the cache
            webView.Source = new Uri($"https://www.petrolimex.com.vn/?nocache={DateTime.Now.Ticks}");
        }

        private async void WebView_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            try
            {
                if (e.IsSuccess)
                {
                    lblTitle.Text = "Extracting live prices...";

                    string js = @"
                (function() {
                    let results = [];
                    let seenProducts = new Set();
                    let rows = document.querySelectorAll('tr');
                    rows.forEach(row => {
                        let cells = row.querySelectorAll('td, th');
                        if(cells.length >= 3) {
                            let product = cells[0].innerText.trim();
                            let v1 = cells[1].innerText.trim();
                            let v2 = cells[2].innerText.trim();
                            
                            // Only add if it's a fuel product AND we haven't seen it yet
                            if((product.includes('RON') || product.includes('DO') || product.includes('E5') || product.includes('Dầu')) && !seenProducts.has(product)) {
                                 seenProducts.add(product);
                                 results.push(product + '|' + v1 + '|' + v2);
                            }
                        }
                    });
                    return results.join('^');
                })();
            ";

                    string rawResult = "";

                    // --- FIX: Smart Wait / Retry Loop ---
                    // Wait up to 8 seconds for Petrolimex's dynamic price table to render via AJAX.
                    for (int i = 0; i < 8; i++)
                    {
                        await Task.Delay(1000); // Wait 1 second before checking

                        rawResult = await webView.CoreWebView2.ExecuteScriptAsync(js);
                        rawResult = rawResult?.Trim('"');

                        // If we successfully found data, break out of the waiting loop immediately
                        if (!string.IsNullOrEmpty(rawResult) && rawResult != "null")
                        {
                            break;
                        }
                    }

                    if (!string.IsNullOrEmpty(rawResult) && rawResult != "null")
                    {
                        DataTable dt = new DataTable();
                        dt.Columns.Add("Sản phẩm");
                        dt.Columns.Add("Vùng 1");
                        dt.Columns.Add("Vùng 2");

                        string[] rows = rawResult.Split('^');
                        foreach (string row in rows)
                        {
                            string[] cols = row.Split('|');
                            if (cols.Length == 3)
                            {
                                dt.Rows.Add(cols[0], cols[1], cols[2]);
                            }
                        }

                        dgvPrices.DataSource = null;
                        dgvPrices.Columns.Clear();
                        dgvPrices.Rows.Clear();

                        dgvPrices.DataSource = dt;
                        dgvPrices.RowHeadersVisible = false;
                        dgvPrices.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                        dgvPrices.BackgroundColor = this.BackColor;
                        dgvPrices.BorderStyle = BorderStyle.None;

                        foreach (DataGridViewColumn col in dgvPrices.Columns)
                        {
                            col.SortMode = DataGridViewColumnSortMode.NotSortable;
                        }

                        dgvPrices.Columns[0].FillWeight = 50;
                        dgvPrices.Columns[1].FillWeight = 25;
                        dgvPrices.Columns[2].FillWeight = 25;

                        dgvPrices.Columns[0].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                        dgvPrices.Columns[1].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                        dgvPrices.Columns[1].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                        dgvPrices.Columns[2].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                        dgvPrices.Columns[2].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;

                        dgvPrices.ClearSelection();
                        dgvPrices.EnableHeadersVisualStyles = false;
                        dgvPrices.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
                        dgvPrices.ColumnHeadersHeight = 25;
                        dgvPrices.ColumnHeadersDefaultCellStyle.Font = new System.Drawing.Font(dgvPrices.Font, System.Drawing.FontStyle.Bold);
                        dgvPrices.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(220, 235, 245);
                        dgvPrices.RowsDefaultCellStyle.BackColor = System.Drawing.Color.White;
                        dgvPrices.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(245, 245, 245);

                        lblTitle.Text = $"Widget giá xăng dầu, by ttlong - Cập nhật: {DateTime.Now:dd/MM/yyyy HH:mm}";
                    }
                    else
                    {
                        lblTitle.Text = "Prices hidden or page loading delayed.";
                    }
                }
                else
                {
                    lblTitle.Text = "Failed to connect to website.";
                }
            }
            catch (Exception)
            {
                lblTitle.Text = "Data extraction failed.";
            }
            finally
            {
                this.BeginInvoke(new Action(() =>
                {
                    if (webView != null)
                    {
                        this.Controls.Remove(webView);
                        webView.Dispose();
                        webView = null;
                    }

                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    EmptyWorkingSet(Process.GetCurrentProcess().Handle);
                }));
            }
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, (IntPtr)HT_CAPTION, IntPtr.Zero);
            }
        }

        private void CloseWidget_DoubleClick(object sender, EventArgs e)
        {
            Properties.Settings.Default.WidgetLocation = this.Location;
            Properties.Settings.Default.ShowTrayIcon = true;
            Properties.Settings.Default.Save();
            RemoveFromStartup();
            Application.Exit();
        }

        private void CheckAndSetStartup()
        {
            try
            {
                string appName = "PetrolimexWidget";
                string appPath = Application.ExecutablePath;

                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
                {
                    if (key != null)
                    {
                        object currentValue = key.GetValue(appName);
                        if (currentValue == null || currentValue.ToString() != appPath)
                        {
                            key.SetValue(appName, appPath);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not set startup: " + ex.Message);
            }
        }

        private void SetupSystemTray()
        {
            trayMenu = new ContextMenuStrip();

            ToolStripMenuItem hideOption = new ToolStripMenuItem("Ẩn biểu tượng khay hệ thống");
            hideOption.Click += (s, e) =>
            {
                trayIcon.Visible = false;
                Properties.Settings.Default.ShowTrayIcon = false;
                Properties.Settings.Default.Save();
            };
            trayMenu.Items.Add(hideOption);
            trayMenu.Items.Add(new ToolStripSeparator());

            ToolStripMenuItem closeOption = new ToolStripMenuItem("Đóng widget");
            closeOption.Click += CloseWidget_DoubleClick;
            trayMenu.Items.Add(closeOption);

            trayIcon = new NotifyIcon();
            trayIcon.Text = "Petrolimex Widget";
            trayIcon.Icon = this.Icon;
            trayIcon.ContextMenuStrip = trayMenu;

            if (Properties.Settings.Default.ShowTrayIcon == true)
            {
                trayIcon.Visible = true;
            }
        }
        private void RemoveFromStartup()
        {
            try
            {
                string appName = "PetrolimexWidget";

                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
                {
                    if (key != null)
                    {
                        if (key.GetValue(appName) != null)
                        {
                            key.DeleteValue(appName, false);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not remove startup: " + ex.Message);
            }
        }
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            if (trayIcon != null)
            {
                trayIcon.Visible = false;
                trayIcon.Dispose();
            }

            base.OnFormClosed(e);
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                // Add the WS_EX_TOOLWINDOW extended window style (0x80)
                cp.ExStyle |= 0x80;
                return cp;
            }
        }

        private const int WM_NCLBUTTONDBLCLK = 0x00A3;

        protected override void WndProc(ref Message m)
        {
            // Intercept the OS-level double-click on the "draggable" area
            if (m.Msg == WM_NCLBUTTONDBLCLK && m.WParam.ToInt32() == HT_CAPTION)
            {
                // Trigger the manual update
                ForceUpdate_DoubleClick(this, EventArgs.Empty);

                // Consume the message by setting Result to zero so Windows doesn't 
                // try to maximize or restore the borderless form
                m.Result = IntPtr.Zero;
                return;
            }

            base.WndProc(ref m);
        }
    }
}