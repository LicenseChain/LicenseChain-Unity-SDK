using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace LicenseChain
{
    [Serializable]
    public class LicenseChainResponse
    {
        public bool success;
        public string message;
        public string sessionid;
        public object info;
        public object data;
        public object users;
        public object messages;
        public string contents;
    }

    [Serializable]
    public class LicenseChainUser
    {
        public string username;
        public string email;
        public string license;
        public string expiry;
        public string hwid;
        public List<string> subscriptions;
        public Dictionary<string, string> variables;
        public Dictionary<string, object> data;
    }

    [Serializable]
    public class LicenseChainAppStats
    {
        public int totalUsers;
        public int onlineUsers;
        public int totalLicenses;
        public int activeLicenses;
        public int expiredLicenses;
    }

    [Serializable]
    public class LicenseChainChatMessage
    {
        public string username;
        public string message;
        public string timestamp;
        public string channel;
    }
}
