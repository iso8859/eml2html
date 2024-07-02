using System;
using System.Text.RegularExpressions;
using System.Text;
using System.IO;
using MimeKit;
using Org.BouncyCastle.Tls;

public class eml2html
{
    static string _formatHtmlTag(string src)
    {
        src = src.Replace(">", "&gt;");
        src = src.Replace("<", "&lt;");
        return src;
    }

    static string _formatAddresses(InternetAddressList addresses, string prefix)
    {
        StringBuilder buffer = new StringBuilder();
        buffer.Append(string.Format("<b>{0}:</b> ", prefix));

        string sep = "";
        foreach (MailboxAddress address in addresses)
        {
            buffer.Append(sep + _formatHtmlTag(address.ToString()));
            sep = ";";
        }

        buffer.Append("<br>");
        return buffer.ToString();
    }

    static string ConvertTextToHTML(string txt)
    {
        StringReader sr = new StringReader(txt);
        string result;
        using (StringWriter sw = new StringWriter())
        {
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                sw.Write(_formatHtmlTag(line));
                sw.WriteLine("<br>");
            }
            result = sw.ToString();
        }
        return result;
    }

    static public void Convert(Stream eml, Stream htmlOut)
    {
        MimeMessage mail = MimeMessage.Load(eml);

        string html = mail.HtmlBody;
        if (html == null)
            html = ConvertTextToHTML(mail.TextBody);
        StringBuilder header = new StringBuilder();

        header.Append("<font face=\"Courier New,Arial\" size=2>");
        header.Append("<b>From:</b> " + _formatHtmlTag(mail.From.ToString()) + "<br>");

        header.Append(_formatAddresses(mail.To, "To"));
        header.Append(_formatAddresses(mail.Cc, "Cc"));
        header.Append("<b>Date:</b>" + mail.Date.ToString("yyyy-MM-dd HH:mm:ss") + "<br>");

        header.Append(string.Format("<b>Subject:</b>{0}<br>\r\n", _formatHtmlTag(mail.Subject)));

        // Change original meta header encoding to utf-8
        if (!string.IsNullOrEmpty(html))
        {
            Regex reg = new Regex("(<meta[^>]*charset[ \t]*=[ \t\"]*)([^<> \r\n\"]*)", RegexOptions.Multiline | RegexOptions.IgnoreCase);
            html = reg.Replace(html, "$1utf-8");
            if (!reg.IsMatch(html))
            {
                header.Insert(0, "<meta HTTP-EQUIV=\"Content-Type\" Content=\"text/html; charset=utf-8\">");
            }
        }
        html = header.ToString() + "<hr>" + html;
        StreamWriter writer = new StreamWriter(htmlOut, Encoding.UTF8);
        writer.Write(html);
        writer.Flush();
    }
}