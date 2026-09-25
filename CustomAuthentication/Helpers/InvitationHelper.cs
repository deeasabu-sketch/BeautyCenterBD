using System;
using System.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace CustomAuthentication.Helpers
{
    public static class InvitationHelper
    {
        public static int ExpiryHours
        {
            get
            {
                int hours;
                if (!int.TryParse(ConfigurationManager.AppSettings["PasswordSetupExpiryHours"], out hours)) hours = 24;
                return Math.Max(1, hours);
            }
        }

        public static string CreateToken()
        {
            byte[] bytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create()) rng.GetBytes(bytes);
            return Base64Url(bytes);
        }

        public static string HashToken(string token)
        {
            using (var sha = SHA256.Create())
            {
                return Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(token ?? string.Empty)));
            }
        }

        public static string BuildSetupUrl(System.Web.Mvc.UrlHelper url, string token)
        {
            var baseUrl = ConfigurationManager.AppSettings["AppBaseUrl"];
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                var request = System.Web.HttpContext.Current?.Request;
                baseUrl = request?.Url?.GetLeftPart(UriPartial.Authority);
            }
            if (string.IsNullOrWhiteSpace(baseUrl)) throw new InvalidOperationException("AppBaseUrl is not configured and the current request URL is unavailable.");
            return baseUrl.TrimEnd('/') + url.Action("SetPassword", "Account", new { token = token });
        }

        public static string BuildSetupEmail(string fullName, string roleName, string setupUrl, DateTime expiresAt)
        {
            return $@"<!doctype html><html><body style='font-family:Arial,sans-serif;background:#f5f7fb;padding:30px;color:#24324A'>
<div style='max-width:620px;margin:auto;background:#fff;border-radius:18px;padding:32px;box-shadow:0 8px 30px rgba(124,58,237,.12)'>
<h2 style='margin-top:0;color:#4C1D95'>Welcome to BeautyCenterBD</h2>
<p>Hello <strong>{System.Web.HttpUtility.HtmlEncode(fullName)}</strong>,</p>
<p>An administrator created an account for you as <strong>{System.Web.HttpUtility.HtmlEncode(roleName)}</strong>.</p>
<p>Use the button below to set your own password. You will then be able to log in with this email address.</p>
<p><a href='{System.Web.HttpUtility.HtmlAttributeEncode(setupUrl)}' style='display:inline-block;padding:12px 22px;background:linear-gradient(90deg,#3B82F6,#7C3AED);color:#fff;text-decoration:none;border-radius:10px;font-weight:700'>Set My Password</a></p>
<p style='font-size:13px;color:#64748B'>This link expires on <strong>{expiresAt:dd MMM yyyy, hh:mm tt}</strong>. If you did not expect this invitation, you can ignore this email.</p>
<p style='font-size:12px;color:#94A3B8'>BeautyCenterBD Account Management</p></div></body></html>";
        }

        public static string BuildAdminExpiryEmail(string userName, string userEmail, string roleName, DateTime expiredAt)
        {
            return $@"<!doctype html><html><body style='font-family:Arial,sans-serif;color:#24324A'>
<h3 style='color:#7C3AED'>Password setup reminder</h3>
<p>The invited user <strong>{System.Web.HttpUtility.HtmlEncode(userName)}</strong> ({System.Web.HttpUtility.HtmlEncode(userEmail)}) has not set a password yet.</p>
<p>Role: <strong>{System.Web.HttpUtility.HtmlEncode(roleName)}</strong></p>
<p>The previous password setup link expired on <strong>{expiredAt:dd MMM yyyy, hh:mm tt}</strong>.</p>
<p>Please open the Users page and use <strong>Resend Invite</strong> to send a fresh password setup link.</p>
</body></html>";
        }

        private static string Base64Url(byte[] bytes)
        {
            return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }
    }
}