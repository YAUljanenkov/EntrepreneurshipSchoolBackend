namespace EntrepreneurshipSchoolBackend;

/// <summary>
/// This struct contains all properties of the app.
/// </summary>
public struct Properties
{
    /// <summary>
    /// Login of an admin to the system itself.
    /// </summary>
    public const string AdminLogin = "admin";

    /// <summary>
    /// Password of an admin to the system.
    /// </summary>
    public const string AdminPassword = "password";

    /// <summary>
    /// An address of an smtp server to send messagess from. It is Yandex by default because HSE uses it.
    /// </summary>
    public const string SmtpAddress = "smtp.yandex.ru";

    /// <summary>
    /// A port of an smtp server. By default it is Yandex's mail.
    /// </summary>
    public const int SmtpPort = 465;

    /// <summary>
    /// True if an smtp server uses SSL to auth.
    /// </summary>
    public const bool SmtpUseSsl = true;

    /// <summary>
    /// A name of an email address.
    /// </summary>
    public const string EmailName = "Школа предпринимательства";

    /// <summary>
    /// An email to send messages from.
    /// </summary>
    public const string EmailAddress = "ent.school@yandex.ru";

    /// <summary>
    /// A password for email to send messages from. If you use yandex mail, it should be a specially generated
    /// password for applications. 
    /// </summary>
    public const string EmailPassword = "sgwfgbmsdkgmvhwt";
    
    /// <summary>
    /// If you need a mailing list about deadlines, this constant should be set to true.
    /// </summary>
    public const bool NeedSendEmail = false;
    
    /// <summary>
    /// Returns title for a notification message about new task.
    /// </summary>
    public static readonly Func<string, string> NewTaskTitle = taskTitle => $"[Школа Предпринимательства] Новое задание: {taskTitle}";

    /// <summary>
    /// Returns notification message about new task.
    /// </summary>
    /// <param name="taskName">The name of the published task.</param>
    /// <param name="deadline">The deadline of the published task</param>
    /// <returns>Message</returns>
    public static string NewTaskMessage(string taskName, DateTime deadline)
    {
        return
            $"Здравствуйте!\n\nВ онлайн-системе Школы Gредпринимательства опубликовано новое задание: \"{taskName}\".\nУстановленный срок сдачи: {deadline}.\nУспехов в выполнении!\n\nС уважением\nШкола Предпринимательства.";
    }
    
    /// <summary>
    /// Returns title for a notification message about a task to check for trackers.
    /// </summary>
    public static readonly Func<string, string> NewTaskToCheck = taskTitle => $"[Школа Предпринимательства] Требуется оценка задания: {taskTitle}";
    
    /// <summary>
    /// Returns notification message about new task.
    /// </summary>
    /// <param name="taskName">The name of the published task.</param>
    /// <returns>Message</returns>
    public static string NewTaskToCheckMessage(string taskName)
    {
        return
            $"Здравствуйте!\n\nЗадание \"{taskName}\" требует оценивания.\n\nС уважением\nШкола Предпринимательства.";
    }
    
        
    /// <summary>
    /// Returns title for a notification message about an upcoming deadline.
    /// </summary>
    public static readonly Func<string, string> DeadlineIsSoonTitle = taskTitle => $"[Школа Предпринимательства] Cкоро дедлайн задания: {taskTitle}";
    
    /// <summary>
    /// Returns notification message about an upcoming deadline.
    /// </summary>
    /// <param name="taskName">The name of the task.</param>
    /// <returns>Message</returns>
    public static string DeadlineSoonMessage(string taskName)
    {
        return
            $"Здравствуйте!\n\nСрок сдачи задания \"{taskName}\" истекает завтра.\n\nС уважением\nШкола Предпринимательства.";
    }
}
