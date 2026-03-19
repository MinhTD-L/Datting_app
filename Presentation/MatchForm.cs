using BusinessLogic;
using DataTransferObject;
using Presentation.FormChat;
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Presentation
{
    public sealed class MatchForm : Form
    {
        private readonly ChatBLL _chatBll;

        private ComboBox _cboLookingFor;
        private Label _lblStatus;
        private Button _btnStart;
        private Button _btnLeave;

        private bool _searching;
        private bool _handledMatched;

        public MatchForm(ChatBLL chatBll)
        {
            _chatBll = chatBll ?? throw new ArgumentNullException(nameof(chatBll));

            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.White;
            MinimumSize = new Size(560, 340);
            Text = "Ghép đôi";

            BuildUi();
            Wire();

            FormClosing += MatchForm_FormClosing;
        }

        private void BuildUi()
        {
            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 64,
                BackColor = Color.White,
                Padding = new Padding(16, 12, 16, 10)
            };

            var title = new Label
            {
                Text = "Ghép đôi",
                AutoSize = true,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 30, 30),
                Location = new Point(0, 6)
            };

            header.Controls.Add(title);
            header.Controls.Add(new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 1,
                BackColor = Color.FromArgb(235, 235, 235)
            });

            var body = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(16, 14, 16, 14),
                BackColor = Color.White
            };

            var lblLookingFor = new Label
            {
                Text = "Bạn muốn ghép với:",
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 30, 30),
                Location = new Point(0, 0)
            };

            _cboLookingFor = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10),
                Width = 240,
                Location = new Point(0, lblLookingFor.Bottom + 8)
            };
            _cboLookingFor.Items.AddRange(new object[] { "male", "female", "other" });
            _cboLookingFor.SelectedIndex = 1; // default: female

            _lblStatus = new Label
            {
                AutoSize = false,
                Width = body.ClientSize.Width - 32,
                Height = 90,
                Location = new Point(0, _cboLookingFor.Bottom + 20),
                TextAlign = ContentAlignment.TopLeft,
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.Gray,
                Text = "Chọn giới tính và bấm “Ghép đôi”."
            };

            var footer = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 70,
                BackColor = Color.White,
                Padding = new Padding(16, 12, 16, 12)
            };

            _btnStart = new Button
            {
                Text = "Ghép đôi",
                Width = 180,
                Height = 42,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(255, 30, 100),
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            _btnStart.FlatAppearance.BorderSize = 0;

            _btnLeave = new Button
            {
                Text = "Rời hàng chờ",
                Width = 170,
                Height = 42,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(245, 246, 250),
                ForeColor = Color.FromArgb(45, 45, 45),
                Cursor = Cursors.Hand,
                Enabled = false
            };
            _btnLeave.FlatAppearance.BorderColor = Color.FromArgb(230, 230, 230);

            footer.Controls.Add(_btnStart);
            footer.Controls.Add(_btnLeave);

            void Layout()
            {
                _lblStatus.Width = body.ClientSize.Width - 32;
                _btnStart.Left = (footer.ClientSize.Width - (_btnStart.Width + 12 + _btnLeave.Width));
                _btnStart.Top = 12;
                _btnLeave.Left = _btnStart.Right + 12;
                _btnLeave.Top = 12;
            }

            body.SizeChanged += (_, __) => Layout();
            footer.SizeChanged += (_, __) => Layout();
            Layout();

            _btnStart.Click += async (_, __) => await StartAsync();
            _btnLeave.Click += async (_, __) => await LeaveAndCloseAsync();

            body.Controls.Add(lblLookingFor);
            body.Controls.Add(_cboLookingFor);
            body.Controls.Add(_lblStatus);

            Controls.Add(body);
            Controls.Add(footer);
            Controls.Add(header);
        }

        private void Wire()
        {
            _chatBll.WaitingReceived += OnWaitingReceived;
            _chatBll.MatchedReceived += OnMatchedReceived;
            _chatBll.LeftQueueReceived += OnLeftQueueReceived;
            _chatBll.Error += OnChatError;
        }

        private void Unwire()
        {
            _chatBll.WaitingReceived -= OnWaitingReceived;
            _chatBll.MatchedReceived -= OnMatchedReceived;
            _chatBll.LeftQueueReceived -= OnLeftQueueReceived;
            _chatBll.Error -= OnChatError;
        }

        private async void MatchForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                Unwire();

                // Nếu user vẫn đang tìm, gửi leave_match để backend dừng việc ghép.
                if (_searching && !_handledMatched)
                    await _chatBll.LeaveMatchAsync();
            }
            catch
            {
                // ignore
            }
            finally
            {
                _searching = false;
            }
        }

        private void SetSearchingState(bool searching)
        {
            _searching = searching;
            _btnStart.Enabled = !searching;
            _btnLeave.Enabled = searching;
            _cboLookingFor.Enabled = !searching;
        }

        private async Task StartAsync()
        {
            if (_handledMatched) return;
            if (_searching) return;

            var lookingFor = _cboLookingFor.SelectedItem?.ToString();
            if (string.IsNullOrWhiteSpace(lookingFor))
            {
                MessageBox.Show(this, "Vui lòng chọn giới tính.", "Thiếu thông tin",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            SetSearchingState(true);
            _lblStatus.Text = "Đang kết nối...";

            try
            {
                await _chatBll.EnsureConnectedAsync(SessionManager.Token);
                await _chatBll.JoinMatchAsync(lookingFor);
                // Backend sẽ đẩy event 'waiting' hoặc 'matched'
                _lblStatus.Text = "Đang tìm đối tác...";
            }
            catch (Exception ex)
            {
                SetSearchingState(false);
                _lblStatus.Text = "Không thể ghép đôi.";
                MessageBox.Show(this, ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task LeaveAndCloseAsync()
        {
            if (_handledMatched) return;
            if (!_searching) { Close(); return; }

            SetSearchingState(false);
            _lblStatus.Text = "Đang rời hàng chờ...";

            try
            {
                await _chatBll.LeaveMatchAsync();
            }
            catch
            {
                // ignore (đóng form vẫn được)
            }
            Close();
        }

        private void OnWaitingReceived()
        {
            if (IsDisposed) return;
            BeginInvoke(new Action(() =>
            {
                if (IsDisposed) return;
                if (!_searching || _handledMatched) return;
                _lblStatus.Text = "Đang chờ đối tác...";
            }));
        }

        private void OnMatchedReceived(string withUserId, string sessionId)
        {
            if (IsDisposed) return;
            BeginInvoke(new Action(() =>
            {
                if (IsDisposed) return;
                if (string.IsNullOrWhiteSpace(withUserId)) return;
                if (_handledMatched) return;
                _handledMatched = true;

                _searching = false;
                _btnStart.Enabled = false;
                _btnLeave.Enabled = false;
                _cboLookingFor.Enabled = false;
                _lblStatus.Text = "Đã ghép đôi. Đang mở chat...";

                try
                {
                    using var wnd = new ChatWindow(_chatBll, withUserId, withUserId, null, sessionId);
                    wnd.ShowDialog(this);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "Không thể mở chat: " + ex.Message, "Lỗi",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    Close();
                }
            }));
        }

        private void OnLeftQueueReceived(string withUserId, string sessionId)
        {
            if (IsDisposed) return;
            BeginInvoke(new Action(() =>
            {
                if (IsDisposed) return;
                if (_handledMatched) return;
                _lblStatus.Text = "Bạn đã rời hàng chờ.";
            }));
        }

        private void OnChatError(string err)
        {
            if (IsDisposed) return;
            BeginInvoke(new Action(() =>
            {
                if (IsDisposed) return;
                if (string.IsNullOrWhiteSpace(err)) return;
                // Không show MessageBox liên tục; ưu tiên hiển thị vào status.
                _lblStatus.Text = err;
            }));
        }
    }
}

