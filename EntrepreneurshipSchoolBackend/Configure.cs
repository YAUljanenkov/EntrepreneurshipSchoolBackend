namespace EntrepreneurshipSchoolBackend;

/// <summary>
/// This struct contains all properties of the app.
/// </summary>
public struct Properties
{
    /// <summary>
    /// Login of an admin to the system itself.
    /// </summary>
    public static readonly string AdminLogin = "admin";
    
    /// <summary>
    /// Password of an admin to the system.
    /// </summary>
    public static readonly string AdminPassword = "password";
    
    /// <summary>
    /// An address of an smtp server to send messagess from. It is Yandex by default because HSE uses it.
    /// </summary>
    public static readonly string SmtpAddress = "smtp.yandex.ru";
    
    /// <summary>
    /// A port of an smtp server. By default it is Yandex's mail.
    /// </summary>
    public static readonly int SmtpPort = 465;
    
    /// <summary>
    /// True if an smtp server uses SSL to auth.
    /// </summary>
    public static readonly bool SmtpUseSsl = true;

    /// <summary>
    /// A name of an email address.
    /// </summary>
    public static readonly string EmailName = "Школа предпринимательства";
    
    /// <summary>
    /// An email to send messages from.
    /// </summary>
    public static readonly string EmailAddress = "yaaulyanenkov@edu.hse.ru";
    
    /// <summary>
    /// A password for email to send messages from. If you use yandex mail, it should be a specially generated
    /// password for applications. 
    /// </summary>
    public static readonly string EmailPassword = "othhbvjqhsmtfmxh";
}
