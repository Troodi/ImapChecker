using MailKit;
using MailKit.Net.Imap;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

class ImapChecker
{
    static private string[] mailData { get; set; }
    static private ImapClient client;
    private int maxErrors;
    private bool validated = false;

    public ImapChecker(int errorLimit = 50)
    {
        maxErrors = errorLimit;
        client = new ImapClient();
        client.ServerCertificateValidationCallback = (s, c, h, e) => true;
    }

    public void SetMail(string data)
    {
        mailData = data.Split(':');
    }

    public bool ValidateMail(string mailWithPassword = null)
    {
        string[] mailData;
        ImapClient client;

        if (mailWithPassword != null)
        {
            mailData = mailWithPassword.Split(':');
            client = new ImapClient();
            client.ServerCertificateValidationCallback = (s, c, h, e) => true;
        } else
        {
            mailData = ImapChecker.mailData;
            client = ImapChecker.client;
        }

        if (mailData == null)
        {
            Console.WriteLine("Не введена почта для проверки!");
            return false;
        }
        Console.WriteLine("Подключение к почтовому сервису");
        try
        {
            if (mailData[0].Contains("@mail.ru") || mailData[0].Contains("@inbox.ru") || mailData[0].Contains("@list.ru") || mailData[0].Contains("@bk.ru"))
            {
                Console.WriteLine("Подключаемся к imap.mail.ru");
                client.Connect("imap.mail.ru", 993, true);
            }
            else if (mailData[0].Contains("@hotmail.com") || mailData[0].Contains("@outlook."))
            {
                Console.WriteLine("Подключаемся к imap-mail.outlook.com");
                client.Connect("imap-mail.outlook.com", 993, true);
            }
            else if (mailData[0].Contains("@yahoo.com"))
            {
                Console.WriteLine("Подключаемся к imap.mail.yahoo.com");
                client.Connect("imap.mail.yahoo.com", 993, true);
            }
            else if (mailData[0].Contains("@yandex.") || mailData[0].Contains("@ya.ru"))
            {
                Console.WriteLine("Подключаемся к imap.yandex.ru");
                client.Connect("imap.yandex.ru", 993, true);
            }
            else if (mailData[0].Contains("@gmail.com"))
            {
                Console.WriteLine("Подключаемся к imap.gmail.com");
                client.Connect("imap.gmail.com", 993, true);
            }
            else if (mailData[0].Contains("@aol.com"))
            {
                Console.WriteLine("Подключаемся к imap.aol.com");
                client.Connect("imap.aol.com", 993, true);
            }
            else if (mailData[0].Contains("@mail.com"))
            {
                Console.WriteLine("Подключаемся к imap.mail.com");
                client.Connect("imap.mail.com", 993, true);
            }
            else if (mailData[0].Contains("@gmx."))
            {
                Console.WriteLine("Подключаемся к imap.gmx.com");
                client.Connect("imap.gmx.com", 993, true);
            }
            else if (mailData[0].Contains("@o2.pl"))
            {
                Console.WriteLine("Подключаемся к poczta.o2.pl");
                client.Connect("poczta.o2.pl", 993, true);
            }
            else if (mailData[0].Contains("@wp.pl"))
            {
                Console.WriteLine("Подключаемся к smtp.wp.pl");
                client.Connect("smtp.wp.pl", 993, true);
            }
            else if (mailData[0].Contains("@onet.pl"))
            {
                Console.WriteLine("Подключаемся к imap.poczta.onet.pl");
                client.Connect("imap.poczta.onet.pl", 993, true);
            }
            else if (mailData[0].Contains("@rambler.ru"))
            {
                Console.WriteLine("Подключаемся к imap.rambler.ru");
                client.Connect("imap.rambler.ru", 993, true);
            }
            else
            {
                Console.WriteLine("Данный сервис невозможно проверить с помощью данного класса");
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Произошла неизвестная ошибка при подключении к imap серверу");
            Console.WriteLine(ex.Message);
            return false;
        }

        try
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("Попытка авторизации!");
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            client.Authenticate(mailData[0], mailData[1]);
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("Почта успешно авторизовалась!");
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            validated = true;
            //var personal = client.GetFolder(client.PersonalNamespaces[0]);
            //foreach (var folder in personal.GetSubfolders(false))
            //    Console.WriteLine("[folder] {0}", folder.Name);
            //Console.ReadLine();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            Console.WriteLine("Невозможно авторизоваться!");
            return false;
        }

        try
        {
            Console.WriteLine("Попытка получить список сообщений!");
            var mail = client.GetFolder("inbox");
            mail.Open(FolderAccess.ReadOnly);
            Console.WriteLine("Список сообщений получен успешно!");
        }
        catch(Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine("Невозможно получить список сообщений!");
            Console.WriteLine(ex.Message);
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            return false;
        }
        finally
        {
            if (mailWithPassword != null)
            {
                client.Disconnect(true);
            }
        }
        return true;
    }

    public bool ValidateFromFileToFile(string pathFrom, string pathToSuccess, string pathToFailed = null)
    {
        string[] toCheckMails;
        try
        {
            toCheckMails = File.ReadAllLines(pathFrom);
        }
        catch
        {
            Console.WriteLine("Не удалось открыть файл с почтами");
            return false;
        }
        var listValidated = new List<string>();
        var listInvalidated = new List<string>();
        foreach (var mail in toCheckMails)
        {
            Console.WriteLine("Проверка почты: " + mail);
            SetMail(mail);
            if (ValidateMail())
            {
                Console.WriteLine("Почта валидна");
                listValidated.Add(mail);
            }
            else
            {
                Console.WriteLine("Почта невалидна");
                listInvalidated.Add(mail);
            }
            client.Disconnect(true);
        }
        try
        {
            File.WriteAllLines(pathToSuccess, listValidated.ToArray());
        }
        catch
        {
            Console.WriteLine("Не удалось записать файл с успешными почтами");
            return false;
        }
        if (pathToFailed != null)
        {
            try
            {
                File.WriteAllLines(pathToFailed, listInvalidated.ToArray());
            }
            catch
            {
                Console.WriteLine("Не удалось записать файл с невалидными почтами");
                return false;
            }
        }
        return true;
    }

    public void Disconnect()
    {
        try
        {
            client.Disconnect(true);
        }
        catch { }
    }

    public void checkFileMails(string path, string saveValid, string saveBroken = null)
    {
        var validEmailsToWrite = new List<string>(); 
        Parallel.ForEach(File.ReadLines(path), (line, _, lineNumber) =>
        {
            if (ValidateMail(line))
            {
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine(lineNumber + " - " + line);
                validEmailsToWrite.Add(line);
                Console.ForegroundColor = ConsoleColor.DarkBlue;
            }
        });
        File.WriteAllLines(saveValid, validEmailsToWrite.ToArray());
    }

    public bool FindCode(string codeRegex, string mailFrom, ref string link)
    {
        if (mailData == null)
        {
            Console.WriteLine("Не введена почта для проверки!");
            return false;
        }
        if (!validated)
        {
            Console.WriteLine("Невозможен поиск в невалидной почте!");
            return false;
        }
        IMailFolder mail;
        var regex = new Regex(codeRegex);
        var flag = true;
        var errors = 0;
        while (true)
        {
            if (flag)
            {
                flag = false;
                mail = client.GetFolder("inbox");
                mail.Open(FolderAccess.ReadWrite);
                Console.WriteLine("Поиск письма во входящих");
            }
            else
            {
                try
                {
                    flag = true;
                    if (mailData[0].Contains("@hotmail.com") || mailData[0].Contains("@outlook."))
                    {
                        mail = client.GetFolder("Junk");
                    }
                    else if (mailData[0].Contains("@yahoo.com"))
                    {
                        mail = client.GetFolder("Bulk Mail");
                    }
                    else
                    {
                        mail = client.GetFolder(SpecialFolder.Junk);
                    }
                    mail.Open(FolderAccess.ReadWrite);
                }
                catch
                {
                    mail = null;
                }
                Console.WriteLine("Поиск письма в спам (если есть такая папка)");
            }
            if (mail != null)
            {
                for (int i = mail.Count - 1; i > 0; i--)
                {
                    var msg = mail.GetMessage(i);
                    if (msg.From.ToString().Contains(mailFrom))
                    {
                        Console.WriteLine("Письмо найдено!");
                        Match match;
                        try
                        {
                            match = regex.Match(WebUtility.HtmlDecode(msg.HtmlBody));
                            link = match.Groups[0].ToString();
                            mail.AddFlags(i, MessageFlags.Seen, true);
                            Console.WriteLine("Ссылка успешно разобрана!");
                            Console.WriteLine("Ссылка: " + link);
                            try
                            {
                                client.Disconnect(true);
                            }
                            catch { }
                            return true;
                        }
                        catch
                        {
                            Console.WriteLine("Не удалось найти ссылку!");
                        }
                    }
                }
                errors++;
            }
            if (maxErrors != 0)
            {
                if (errors > maxErrors)
                {
                    Console.WriteLine("Ничего не пришло!");
                    return false;
                }
            }
            Thread.Sleep(2000);
        }
    }
}
