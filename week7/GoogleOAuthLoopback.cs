using System;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class GoogleTokenResponse
{
    public string access_token;
    public int expires_in;
    public string refresh_token;
    public string scope;
    public string token_type;
    public string id_token;
}

public class GoogleOAuthLoopback
{
    private readonly string clientId;
    private readonly string clientSecret;
    private readonly string[] scopes;

    public GoogleOAuthLoopback(string clientId,string clientSecret, params string[] scopes)
    {
        this.clientId = clientId;
        this.clientSecret = clientSecret;
        this.scopes = (scopes != null && scopes.Length > 0) ? scopes : new[] { "openid", "email", "profile" };
    }

    public async Task<GoogleTokenResponse> SignInAsync(CancellationToken ct = default)
    {
        string codeVerifier = GenerateCodeVerifier();
        string codeChallenge = GenerateCodeChallenge(codeVerifier);
        string state = Base64Url(RandomBytes(16));

        int port = GetRandomUnusedPort();
        string redirectUri = $"http://127.0.0.1:{port}/";
        string scopeParam = Uri.EscapeDataString(string.Join(" ", scopes));

        string authUrl =
            "https://accounts.google.com/o/oauth2/v2/auth" +
            "?response_type=code" +
            "&client_id=" + Uri.EscapeDataString(clientId) +
            "&redirect_uri=" + Uri.EscapeDataString(redirectUri) +
            "&scope=" + scopeParam +
            "&code_challenge=" + codeChallenge +
            "&code_challenge_method=S256" +
            "&access_type=offline" + // optional: gets refresh_token on first consent
            "&prompt=select_account" + // optional: force account chooser
            "&state=" + state;

        using (var listener = new HttpListener())
        {
            listener.Prefixes.Add(redirectUri);
            try { listener.Start(); }
            catch (HttpListenerException ex)
            {
                Debug.LogError("HttpListener failed to start. On Windows you may need URL ACL.\n" + ex);
                throw;
            }

            Application.OpenURL(authUrl);

            // Wait for the browser to hit our redirect URI
            var contextTask = listener.GetContextAsync();
            using (var reg = ct.Register(() => { try { listener.Stop(); } catch { } }))
            {
                var context = await contextTask; // throws if canceled

                // Respond with a simple page that auto‑closes the tab
                string html = "<html><body><script>window.close && window.close();</script>" +
                              "You may close this window and return to the game." +
                              "</body></html>";
                byte[] buf = Encoding.UTF8.GetBytes(html);
                context.Response.ContentLength64 = buf.Length;
                context.Response.OutputStream.Write(buf, 0, buf.Length);
                context.Response.OutputStream.Close();

                string query = context.Request.Url.Query; // like ?code=...&state=...
                var nvc = System.Web.HttpUtility.ParseQueryString(query);
                string code = nvc.Get("code");
                string st = nvc.Get("state");
                string error = nvc.Get("error");

                if (!string.IsNullOrEmpty(error))
                    throw new Exception("OAuth error: " + error);
                if (st != state)
                    throw new Exception("State mismatch. Potential CSRF.");
                if (string.IsNullOrEmpty(code))
                    throw new Exception("No authorization code returned.");

                listener.Stop();

                // Exchange code for tokens
                using (var http = new HttpClient())
                {
                    var form = new FormUrlEncodedContent(new System.Collections.Generic.Dictionary<string, string>
                    {
                        { "client_id", clientId },
                        { "code", code },
                        { "code_verifier", codeVerifier },
                        { "redirect_uri", redirectUri },
                        { "client_secret", clientSecret },
                        { "grant_type", "authorization_code" }
                    });

                    HttpResponseMessage resp = await http.PostAsync("https://oauth2.googleapis.com/token", form);
                    string json = await resp.Content.ReadAsStringAsync();

                    if (!resp.IsSuccessStatusCode)
                        throw new Exception($"Token endpoint error ({(int)resp.StatusCode}): {json}");

                    // UnityEngine.JsonUtility expects field names to match exactly
                    var tokens = JsonUtility.FromJson<GoogleTokenResponse>(SanitizeJson(json));
                    if (tokens == null || string.IsNullOrEmpty(tokens.id_token))
                        throw new Exception("Token response missing id_token. Check scopes and client type.");

                    return tokens;
                }
            }
        }
    }

    // --- Helpers ---
    private static string GenerateCodeVerifier()
    {
        // 43-128 chars URL-safe
        return Base64Url(RandomBytes(32));
    }

    private static string GenerateCodeChallenge(string codeVerifier)
    {
        using (var sha = SHA256.Create())
        {
            byte[] bytes = Encoding.ASCII.GetBytes(codeVerifier);
            byte[] hash = sha.ComputeHash(bytes);
            return Base64Url(hash);
        }
    }

    private static byte[] RandomBytes(int len)
    {
        byte[] data = new byte[len];
        RandomNumberGenerator.Fill(data);
        return data;
    }

    private static string Base64Url(byte[] data)
    {
        return Convert.ToBase64String(data)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private static int GetRandomUnusedPort()
    {
        var l = new TcpListener(System.Net.IPAddress.Loopback, 0);
        l.Start();
        int port = ((System.Net.IPEndPoint)l.LocalEndpoint).Port;
        l.Stop();
        return port;
    }

    // Some token fields may be absent; ensure a valid JSON for JsonUtility
    private static string SanitizeJson(string json)
    {
        // JsonUtility can parse standard JSON fine; kept for future tweaks if needed.
        return json;
    }
}