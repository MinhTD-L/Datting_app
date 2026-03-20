using System;
using System.Drawing;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using System.Windows.Forms;
using BusinessLogic;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace Presentation.FormChat
{
    public sealed class CallForm : Form
    {
        private readonly ChatBLL _chatBll;
        private readonly SynchronizationContext _uiContext;
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

        private System.Windows.Forms.Timer _timer;
        private int _seconds = 0;
        private bool _isCallActive = false;

        private WebView2 _webView;
        private bool _webViewReady = false;
        private bool _rtcInitStarted = false;
        private bool _incomingStartRequested = false;
        private string _pendingRemoteAnswer;
        private readonly List<string> _pendingIceCandidates = new();

        private const string BaseUrl = "https://litmatchclone-production.up.railway.app";

        public CallForm(ChatBLL chatBll, string userId, string userName, string avatar, bool incoming, string callType, string messageId = null, string offer = null)
        {
            _chatBll = chatBll;
            // Captures the UI thread context so we can safely marshal back to the UI even when
            // the form handle might not be available anymore.
            _uiContext = SynchronizationContext.Current;
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
            _webView = new WebView2
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Black
            };
            Controls.Add(_webView);

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

            _timer = new System.Windows.Forms.Timer { Interval = 1000 };
            _timer.Tick += (_, __) =>
            {
                _seconds++;
                var span = TimeSpan.FromSeconds(_seconds);
                _lblStatus.Text = span.ToString(@"mm\:ss");
            };

            // Ensure UI overlays appear above the media surface.
            _webView.SendToBack();
            _pbAvatar.BringToFront();
            _lblStatus.BringToFront();
            _btnAccept.BringToFront();
            _btnReject.BringToFront();
            _btnEnd.BringToFront();
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

            await EnsureWebRtcInitializedAsync();
        }

        private void Wire()
        {
            _chatBll.CallCreated += OnCallCreated;
            _chatBll.CallAnswerReceived += OnCallAnswerReceived;
            _chatBll.IceCandidateReceived += OnIceCandidateReceived;
            _chatBll.CallEnded += OnCallEnded;
        }

        private void Unwire()
        {
            _chatBll.CallCreated -= OnCallCreated;
            _chatBll.CallAnswerReceived -= OnCallAnswerReceived;
            _chatBll.IceCandidateReceived -= OnIceCandidateReceived;
            _chatBll.CallEnded -= OnCallEnded;
        }

        private void OnIceCandidateReceived(string from, string candidate)
        {
            if (IsDisposed) return;
            if (!string.Equals(from, _userId, StringComparison.Ordinal)) return;
            if (string.IsNullOrWhiteSpace(candidate)) return;

            // Wait until WebView2/JS is ready; ICE may arrive early.
            if (!_webViewReady)
            {
                _pendingIceCandidates.Add(candidate);
                return;
            }

            _ = PostToJsAsync(new
            {
                type = "remote-ice-candidate",
                candidate
            });
        }

        private void PostToUi(Action action)
        {
            if (action == null) return;
            if (IsDisposed) return;

            var ctx = _uiContext;
            if (ctx != null)
            {
                try
                {
                    ctx.Post(_ =>
                    {
                        if (IsDisposed) return;
                        try { action(); } catch { }
                    }, null);
                }
                catch
                {
                }
                return;
            }

            if (IsHandleCreated)
            {
                try { BeginInvoke(action); } catch { }
            }
        }

        private Task PostToJsAsync(object payload)
        {
            if (payload == null) return Task.CompletedTask;
            if (!_webViewReady) return Task.CompletedTask;
            if (_webView?.CoreWebView2 == null) return Task.CompletedTask;

            try
            {
                var json = JsonSerializer.Serialize(payload);
                _webView.CoreWebView2.PostWebMessageAsJson(json);
            }
            catch
            {
            }

            return Task.CompletedTask;
        }

        private void WebView_CoreWebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            if (IsDisposed) return;

            // WebView2 gives JSON text (because we use PostWebMessageAsJson).
            var raw = e.WebMessageAsJson;
            if (string.IsNullOrWhiteSpace(raw)) return;

            JsonDocument doc;
            try
            {
                doc = JsonDocument.Parse(raw);
                if (doc.RootElement.ValueKind == JsonValueKind.String)
                {
                    var inner = doc.RootElement.GetString();
                    if (!string.IsNullOrWhiteSpace(inner))
                    {
                        doc.Dispose();
                        doc = JsonDocument.Parse(inner);
                    }
                }
            }
            catch
            {
                return;
            }

            if (!doc.RootElement.TryGetProperty("type", out var typeEl)) return;
            var type = typeEl.GetString() ?? "";

            switch (type)
            {
                case "offer":
                {
                    if (!doc.RootElement.TryGetProperty("offer", out var offerEl)) return;
                    var offer = offerEl.GetString();
                    if (string.IsNullOrWhiteSpace(offer)) return;

                    _ = _chatBll.SendCallOfferAsync(_userId, _callType, offer);
                    break;
                }
                case "answer":
                {
                    if (!doc.RootElement.TryGetProperty("answer", out var answerEl)) return;
                    var answer = answerEl.GetString();
                    if (string.IsNullOrWhiteSpace(answer)) return;

                    if (string.IsNullOrWhiteSpace(_messageId)) return;
                    _ = _chatBll.SendCallAnswerAsync(_userId, _messageId, answer);
                    break;
                }
                case "ice-candidate":
                {
                    if (!doc.RootElement.TryGetProperty("candidate", out var candEl)) return;
                    var candidate = candEl.GetString();
                    if (string.IsNullOrWhiteSpace(candidate)) return;

                    _ = _chatBll.SendIceCandidateAsync(_userId, candidate);
                    break;
                }
                case "connected":
                {
                    PostToUi(() =>
                    {
                        if (_isCallActive) return;
                        _seconds = 0;
                        _timer.Stop();
                        _timer.Start();
                        _isCallActive = true;
                        _lblStatus.Text = "00:00";
                        _btnEnd.Enabled = true;
                        _btnEnd.Visible = true;
                    });
                    break;
                }
                case "error":
                {
                    if (doc.RootElement.TryGetProperty("message", out var msgEl))
                    {
                        var msg = msgEl.GetString();
                        PostToUi(() => _lblStatus.Text = "Loi WebRTC: " + (msg ?? ""));
                    }
                    break;
                }
            }
        }

        private async Task EnsureWebRtcInitializedAsync()
        {
            if (_rtcInitStarted) return;
            _rtcInitStarted = true;

            if (_webView == null) return;

            await _webView.EnsureCoreWebView2Async();

            _webView.CoreWebView2.PermissionRequested += (_, e) =>
            {
                try { e.State = CoreWebView2PermissionState.Allow; } catch { }
            };

            _webView.CoreWebView2.WebMessageReceived -= WebView_CoreWebMessageReceived;
            _webView.CoreWebView2.WebMessageReceived += WebView_CoreWebMessageReceived;

            var tcs = new TaskCompletionSource<bool>();

            void NavigationCompleted(object s, CoreWebView2NavigationCompletedEventArgs ev)
            {
                if (!ev.IsSuccess)
                {
                    tcs.TrySetResult(false);
                    return;
                }
                tcs.TrySetResult(true);
            }

            _webView.NavigationCompleted -= NavigationCompleted;
            _webView.NavigationCompleted += NavigationCompleted;

            _webView.CoreWebView2.NavigateToString(WebRtcHtml);

            await tcs.Task;

            // JS can receive messages only after the page is loaded.
            _webViewReady = true;

            _ = PostToJsAsync(new
            {
                type = "init",
                incoming = _incoming,
                callType = _callType,
                remoteOffer = _incoming ? _offer : null
            });

            if (_incomingStartRequested)
            {
                _incomingStartRequested = false;
                _ = PostToJsAsync(new { type = "accept" });
            }

            // Flush pending ICE candidates.
            if (_pendingIceCandidates.Count > 0)
            {
                foreach (var c in _pendingIceCandidates)
                    _ = PostToJsAsync(new { type = "remote-ice-candidate", candidate = c });
                _pendingIceCandidates.Clear();
            }

            // Flush pending answer (outgoing side should receive it from backend).
            if (!string.IsNullOrWhiteSpace(_pendingRemoteAnswer))
            {
                var ans = _pendingRemoteAnswer;
                _pendingRemoteAnswer = null;
                _ = PostToJsAsync(new { type = "remote-answer", answer = ans });
            }
        }

        private static string WebRtcHtml => """
<!doctype html>
<html>
<head>
  <meta charset="utf-8" />
  <meta name="viewport" content="width=device-width, initial-scale=1" />
  <style>
    html, body { margin:0; padding:0; background:#000; overflow:hidden; width:100%; height:100%; }
    #remoteVideo { width:100%; height:100%; object-fit:cover; background:#000; }
    #remoteAudio { display:none; }
    #localVideo {
      position:absolute; right:12px; bottom:12px;
      width:120px; height:80px; object-fit:cover;
      background:#111; border-radius:8px; display:none;
      transform: translateZ(0);
    }
  </style>
</head>
<body>
  <video id="remoteVideo" autoplay playsinline></video>
  <audio id="remoteAudio" autoplay></audio>
  <video id="localVideo" autoplay playsinline muted></video>

  <script>
    // Send plain object so C# can read it via WebMessageAsJson.
    const post = (msg) => {
      try { window.chrome.webview.postMessage(msg); } catch (e) { }
    };

    function parseDescription(text, defaultType){
      if (!text) return null;
      try {
        const obj = JSON.parse(text);
        if (obj && obj.sdp) return obj;
      } catch (e) {}
      return { type: defaultType, sdp: text };
    }

    function parseCandidate(text){
      if (!text) return null;
      try { return JSON.parse(text); } catch (e) {}
      return null;
    }

    let pc = null;
    let localStream = null;
    let remoteStream = null;
    let incoming = false;
    let callType = "voice";
    let remoteOfferText = null;
    let connected = false;
    let pendingRemoteIce = [];
    let pendingRemoteAnswer = null;

    const remoteVideo = document.getElementById('remoteVideo');
    const remoteAudio = document.getElementById('remoteAudio');
    const localVideo = document.getElementById('localVideo');

    function ensurePc(){
      if (pc) return pc;
      pc = new RTCPeerConnection({
        iceServers: [
          { urls: 'stun:stun.l.google.com:19302' },
          { urls: 'stun:stun1.l.google.com:19302' }
        ]
      });

      pc.onicecandidate = (ev) => {
        if (!ev || !ev.candidate) return;
        const cObj = ev.candidate.toJSON ? ev.candidate.toJSON() : ev.candidate;
        post({ type: 'ice-candidate', candidate: JSON.stringify(cObj) });
      };

      pc.ontrack = (ev) => {
        if (!remoteStream) remoteStream = new MediaStream();
        remoteStream.addTrack(ev.track);

        // Attach to both video and audio for better voice support.
        remoteVideo.srcObject = remoteStream;
        remoteAudio.srcObject = remoteStream;
        remoteVideo.play().catch(() => { });
        remoteAudio.play().catch(() => { });

        if (!connected){
          connected = true;
          post({ type: 'connected' });
        }
      };

      // Apply any ICE/answer received before the peer connection existed.
      flushPending().catch(() => { });

      return pc;
    }

    async function getLocalMedia(){
      const wantVideo = callType === 'video';
      return await navigator.mediaDevices.getUserMedia({ audio: true, video: wantVideo });
    }

    async function startOutgoing(){
      ensurePc();
      localStream = await getLocalMedia();
      for (const track of localStream.getTracks()) pc.addTrack(track, localStream);

      if (callType === 'video'){
        localVideo.style.display = 'block';
        localVideo.srcObject = localStream;
        localVideo.play().catch(() => { });
      }

      const offer = await pc.createOffer();
      await pc.setLocalDescription(offer);
      post({ type: 'offer', offer: JSON.stringify(pc.localDescription) });
    }

    async function startIncoming(){
      ensurePc();
      localStream = await getLocalMedia();
      for (const track of localStream.getTracks()) pc.addTrack(track, localStream);

      if (callType === 'video'){
        localVideo.style.display = 'block';
        localVideo.srcObject = localStream;
        localVideo.play().catch(() => { });
      }

      const remoteDesc = parseDescription(remoteOfferText, 'offer');
      await pc.setRemoteDescription(remoteDesc);
      await flushPending();
      const answer = await pc.createAnswer();
      await pc.setLocalDescription(answer);
      post({ type: 'answer', answer: JSON.stringify(pc.localDescription) });
    }

    async function applyRemoteAnswer(answerText){
      if (!pc){
        pendingRemoteAnswer = answerText;
        return;
      }
      const desc = parseDescription(answerText, 'answer');
      await pc.setRemoteDescription(desc);
      await flushPending();
    }

    async function addRemoteIce(candidateText){
      if (!candidateText) return;
      if (!pc){
        pendingRemoteIce.push(candidateText);
        return;
      }
      const cand = parseCandidate(candidateText);
      if (!cand) return;
      await pc.addIceCandidate(cand);
    }

    async function flushPending(){
      if (!pc) return;

      if (pendingRemoteAnswer){
        const ans = pendingRemoteAnswer;
        pendingRemoteAnswer = null;
        try { await applyRemoteAnswer(ans); } catch (e) {}
      }

      if (!pc.remoteDescription) return;

      const ices = pendingRemoteIce || [];
      pendingRemoteIce = [];
      for (const c of ices){
        try { await addRemoteIce(c); } catch (e) {}
      }
    }

    function endCall(){
      try { if (pc){ pc.close(); pc = null; } } catch (e) {}
      try {
        if (localStream){
          localStream.getTracks().forEach(t => t.stop());
          localStream = null;
        }
      } catch (e) {}
      try { remoteStream = null; remoteVideo.srcObject = null; } catch (e) {}
      try { localVideo.srcObject = null; localVideo.style.display = 'none'; } catch (e) {}
      connected = false;
      pendingRemoteIce = [];
      pendingRemoteAnswer = null;
    }

    window.chrome.webview.addEventListener('message', async (event) => {
      let data = event.data;
      if (typeof data === 'string'){
        try { data = JSON.parse(data); } catch(e) {}
      }
      if (!data || !data.type) return;

      switch (data.type){
        case 'init':
          incoming = !!data.incoming;
          callType = data.callType || 'voice';
          remoteOfferText = data.remoteOffer || null;
          connected = false;
          pendingRemoteIce = [];
          pendingRemoteAnswer = null;
          if (!incoming){
            startOutgoing().catch(err => post({ type:'error', message: String(err) }));
          }
          break;
        case 'accept':
          if (incoming){
            startIncoming().catch(err => post({ type:'error', message: String(err) }));
          }
          break;
        case 'remote-answer':
          try { await applyRemoteAnswer(data.answer); } catch (err) { }
          break;
        case 'remote-ice-candidate':
          try { await addRemoteIce(data.candidate); } catch (err) { }
          break;
        case 'end':
          endCall();
          break;
      }
    });
  </script>
</body>
</html>
""";

        private void OnCallCreated(string msgId, string toUserId)
        {
            if (IsDisposed) return;
            if (!string.Equals(toUserId, _userId, StringComparison.Ordinal)) return;
            _messageId = msgId;
            PostToUi(() =>
            {
                _lblStatus.Text = "Đang đổ chuông...";
            });
        }

        private void OnCallAnswerReceived(string from, string msgId, string answer)
        {
            if (IsDisposed) return;
            if (!string.Equals(from, _userId, StringComparison.Ordinal)) return;
            if (!string.IsNullOrWhiteSpace(_messageId) &&
                !string.Equals(msgId, _messageId, StringComparison.Ordinal))
                return;

            if (!_webViewReady)
            {
                _pendingRemoteAnswer = answer;
                return;
            }

            _ = PostToJsAsync(new
            {
                type = "remote-answer",
                answer
            });

            PostToUi(() =>
            {
                _lblStatus.Text = "Đang kết nối...";
            });
        }

        private void OnCallEnded(string msgId, int duration)
        {
            if (IsDisposed) return;
            if (!string.Equals(msgId, _messageId, StringComparison.Ordinal)) return;

            _ = PostToJsAsync(new { type = "end" });

            void PostToUi(Action action)
            {
                if (action == null) return;
                if (IsDisposed) return;

                var ctx = _uiContext;
                if (ctx != null)
                {
                    try
                    {
                        ctx.Post(_ =>
                        {
                            if (IsDisposed) return;
                            try { action(); } catch { }
                        }, null);
                    }
                    catch
                    {
                        // Ignore: form may be tearing down.
                    }
                }
                else
                {
                    // Fallback: works only when the form has a handle.
                    if (IsHandleCreated)
                    {
                        try { BeginInvoke(action); } catch { }
                    }
                }
            }

            PostToUi(() =>
            {
                _timer.Stop();
                _isCallActive = false;
                _lblStatus.Text = "Cuộc gọi đã kết thúc";
                _btnEnd.Enabled = false;
            });

            _ = Task.Delay(1500).ContinueWith(_ => PostToUi(Close));
        }

        private async void BtnAccept_Click(object sender, EventArgs e)
        {
            _btnAccept.Visible = false;
            _btnReject.Visible = false;
            _btnEnd.Visible = true;
            _btnEnd.Enabled = true;

            _seconds = 0;
            _timer.Stop();
            _lblStatus.Text = "Đang kết nối...";

            if (!_webViewReady)
            {
                _incomingStartRequested = true;
                return;
            }

            _ = PostToJsAsync(new { type = "accept" });
        }

        private async void BtnReject_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(_messageId))
                await _chatBll.EndCallAsync(_messageId);

            _ = PostToJsAsync(new { type = "end" });
            Close();
        }

        private async void BtnEnd_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(_messageId))
                await _chatBll.EndCallAsync(_messageId);

            _ = PostToJsAsync(new { type = "end" });
            Close();
        }

        private void CallForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _timer?.Stop();
            _ = PostToJsAsync(new { type = "end" });
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