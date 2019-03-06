using System;
using System.Collections.Generic;
using System.Text;

namespace OpenDKPShared.ClientModels
{
    public class CognitoUser
    {
        public string Username { get; set; }
        public string Nickname { get; set; }
        public string Email { get; set; }
        public List<string> Groups { get; set; }
    }
}
