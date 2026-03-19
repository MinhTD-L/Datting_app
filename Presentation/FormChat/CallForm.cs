using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using BusinessLogic;

namespace Presentation.FormChat
{
    public sealed class CallForm : Form
    {
        private readonly ChatBLL _chatBll;
        private readonly string _userId;
        private readonly string _userName;
        private readonly string _avatar;
        private readonly bool _incoming;
        private readonly string _callType;
        private string _messageId;
        private string _offer;

        private Label _lblStatus;
        private Button _btnAccept;
        private Button _btnReject;
        private Button _btnEnd;
        private PictureBox _pbAvatar;

        private Timer _timer;
        private int _seconds = 0;
        private bool _isCallActive = false;

        private const string BaseUrl = "https://litmatchclone-production.up.railway.app";

        public CallForm(ChatBLL chatBll, string userId, string userName, string avatar, bool incoming, string callType, string messageId = null, string offer = null)
        {
            _chatBll = chatBll;
            _userId = userId;
            _userName = userName;
            _avatar = avatar;
            _incoming = incoming;
            _callType = callType;
            _messageId = messageId;
            _offer = offer;

            Text = string.Equals(_callType, "video", StringComparison.OrdinalIgnoreCase) ? "Video Call" : "Voice Call";
            Size = new Size(360, 520);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(40, 40, 40);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            BuildUI();
            Wire();

            Shown += CallForm_Shown;
            FormClosing += CallForm_FormClosing;
        }

        private void BuildUI()
        {
            _pbAvatar = new PictureBox
            {
                Size = new Size(120, 120),
                Location = new Point((ClientSize.Width - 120) / 2, 60),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(60, 60, 60)
            };
            ApplyRound(_pbAvatar);
            Controls.Add(_pbAvatar);

            var lblName = new Label
            {
                Text = _userName,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Width = ClientSize.Width,
                Location = new Point(0, _pbAvatar.Bottom + 20)
            };
            Controls.Add(lblName);

            _lblStatus = new Label
            {
                Text = _incoming ? "Đang gọi đến..." : "Đang kết nối...",
                ForeColor = Color.LightGray,
                Font = new Font("Segoe UI", 11),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Width = ClientSize.Width,
                Location = new Point(0, lblName.Bottom + 10)
            };
            Controls.Add(_lblStatus);

            _btnAccept = new Button
            {
                Text = "Trả lời",
                BackColor = Color.FromArgb(30, 200, 90),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(100, 44),
                Location = new Point(ClientSize.Width / 2 - 110, ClientSize.Height - 80),
                Visible = _incoming,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            _btnAccept.FlatAppearance.BorderSize = 0;
            _btnAccept.Click += BtnAccept_Click;
            Controls.Add(_btnAccept);

            _btnReject = new Button
            {
                Text = "Từ chối",
                BackColor = Color.FromArgb(255, 60, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(100, 44),
                Location = new Point(ClientSize.Width / 2 + 10, ClientSize.Height - 80),
                Visible = _incoming,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            _btnReject.FlatAppearance.BorderSize = 0;
            _btnReject.Click += BtnReject_Click;
            Controls.Add(_btnReject);

            _btnEnd = new Button
            {
                Text = "Kết thúc",
                BackColor = Color.FromArgb(255, 60, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(140, 44),
                Location = new Point((ClientSize.Width - 140) / 2, ClientSize.Height - 80),
                Visible = !_incoming,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            _btnEnd.FlatAppearance.BorderSize = 0;
            _btnEnd.Click += BtnEnd_Click;
            Controls.Add(_btnEnd);

            _timer = new Timer { Interval = 1000 };
            _timer.Tick += (_, __) =>
            {
                _seconds++;
                var span = TimeSpan.FromSeconds(_seconds);
                _lblStatus.Text = span.ToString(@"mm\:ss");
            };
        }

        private async void CallForm_Shown(object sender, EventArgs e)
        {
            _ = Task.Run(async () =>
            {
                var img = await TryLoadImageAsync(_avatar);
                if (img != null && !_pbAvatar.IsDisposed)
                {
                    try { _pbAvatar.Invoke(new Action(() => _pbAvatar.Image = img)); } catch { }
                }
            });

            if (!_incoming)
            {
                var dummyOffer = "dummy_offer_sdp_for_webrtc";
                await _chatBll.SendCallOfferAsync(_userId, _callType, dummyOffer);
            }
        }

        private void Wire()
        {
            _chatBll.CallCreated += OnCallCreated;
            _chatBll.CallAnswerReceived += OnCallAnswerReceived;
            _chatBll.CallEnded += OnCallEnded;
        }

        private void Unwire()
        {
            _chatBll.CallCreated -= OnCallCreated;
            _chatBll.CallAnswerReceived -= OnCallAnswerReceived;
            _chatBll.CallEnded -= OnCallEnded;
        }

        private void OnCallCreated(string msgId, string toUserId)
        {
            if (IsDisposed) return;
            if (!string.Equals(toUserId, _userId, StringComparison.Ordinal)) return;
            BeginInvoke(new Action(() =>
            {
                _messageId = msgId;
                _lblStatus.Text = "Đang đổ chuông...";
            }));
        }

        private void OnCallAnswerReceived(string from, string msgId, string answer)
        {
            if (IsDisposed) return;
            if (!string.Equals(from, _userId, StringComparison.Ordinal)) return;
            BeginInvoke(new Action(() =>
            {
                _isCallActive = true;
                _lblStatus.Text = "00:00";
                _timer.Start();
            }));
        }

        private void OnCallEnded(string msgId, int duration)
        {
            if (IsDisposed) return;
            if (!string.Equals(msgId, _messageId, StringComparison.Ordinal)) return;
            BeginInvoke(new Action(() =>
            {
                _timer.Stop();
                _isCallActive = false;
                _lblStatus.Text = "Cuộc gọi đã kết thúc";
                _btnEnd.Enabled = false;
                Task.Delay(1500).ContinueWith(_ => BeginInvoke(new Action(Close)));
            }));
        }

        private async void BtnAccept_Click(object sender, EventArgs e)
        {
            _btnAccept.Visible = false;
            _btnReject.Visible = false;
            _btnEnd.Visible = true;
            _isCallActive = true;

            var dummyAnswer = "dummy_answer_sdp_for_webrtc";
            await _chatBll.SendCallAnswerAsync(_userId, _messageId, dummyAnswer);
            
            _lblStatus.Text = "00:00";
            _timer.Start();
        }

        private async void BtnReject_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(_messageId))
                await _chatBll.EndCallAsync(_messageId);
            Close();
        }

        private async void BtnEnd_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(_messageId))
                await _chatBll.EndCallAsync(_messageId);
            Close();
        }

        private void CallForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _timer?.Stop();
            Unwire();
            if (_isCallActive || _incoming) 
            {
                if (!string.IsNullOrWhiteSpace(_messageId))
                    _ = _chatBll.EndCallAsync(_messageId);
            }
        }

        private static void ApplyRound(Control c)
        {
            void Update()
            {
                var path = new System.Drawing.Drawing2D.GraphicsPath();
                path.AddEllipse(0, 0, c.Width, c.Height);
                c.Region = new Region(path);
            }
            c.SizeChanged += (_, __) => Update();
            Update();
        }

        private static async Task<Image> TryLoadImageAsync(string relativeOrFullUrl)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(relativeOrFullUrl)) return null;
                var fullUrl = relativeOrFullUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                    ? relativeOrFullUrl
                    : $"{BaseUrl}{relativeOrFullUrl}";

                using var http = new System.Net.Http.HttpClient();
                var bytes = await http.GetByteArrayAsync(fullUrl);
                using var ms = new System.IO.MemoryStream(bytes);
                return Image.FromStream(ms);
            }
            catch
            {
                return null;
            }
        }
    }
}