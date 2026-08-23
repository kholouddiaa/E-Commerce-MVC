using System.Net;

namespace ECommerce.BLL.Emails;

public static class EmailTemplate
{
    public static string Create(string title, string heading, string content)
    {
        return $"""
            <!DOCTYPE html>
            <html lang="en">
            <head><title>{WebUtility.HtmlEncode(title)}</title></head>
            <body style="margin:0;padding:24px;background:#f4f6f8;font-family:Arial,sans-serif;color:#343a40;">
                <div style="max-width:640px;margin:auto;background:#ffffff;border:1px solid #e1e5e8;">
                    <div style="padding:24px;background:#ffd333;color:#212529;">
                        <h1 style="margin:0;font-size:24px;">{WebUtility.HtmlEncode(heading)}</h1>
                    </div>
                    <div style="padding:24px;line-height:1.6;">
                        {content}
                    </div>
                    <div style="padding:16px 24px;background:#f4f6f8;color:#6c757d;font-size:13px;">
                        E-Commerce MVC
                    </div>
                </div>
            </body>
            </html>
            """;
    }

    public static string Encode(string value)
    {
        return WebUtility.HtmlEncode(value);
    }
}
