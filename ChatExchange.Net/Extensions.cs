﻿/*
 * ChatExchange.Net. A .Net (4.0) API for interacting with Stack Exchange chat.
 * Copyright © 2015, ArcticEcho.
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */





using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Linq;
using System.Text.RegularExpressions;
using CsQuery;
using System.IO;

namespace ChatExchangeDotNet
{
    internal static class Extensions
    {
        private static readonly Regex isReply = new Regex(@"^:\d+\s", RegexOpts);

        internal static RegexOptions RegexOpts { get; } = RegexOptions.Compiled | RegexOptions.CultureInvariant;



        internal static string GetContent(this HttpWebResponse response)
        {
            if (response == null) { throw new ArgumentNullException("response"); }

            using (var strm = response.GetResponseStream())
            using (var reader = new StreamReader(strm))
                return reader.ReadToEnd();
        }

        internal static List<Cookie> GetCookies(this CookieContainer container)
        {
            var cookies = new List<Cookie>();
            var table = (Hashtable)container.GetType().InvokeMember("m_domainTable", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance, null, container, new object[] { });

            foreach (var key in table.Keys)
            {
                Uri uri;
                var domain = key as string;

                if (domain == null) continue;
                if (domain.StartsWith("."))
                    domain = domain.Substring(1);

                var address = $"http://{domain}/";

                if (Uri.TryCreate(address, UriKind.RelativeOrAbsolute, out uri) == false) continue;

                foreach (Cookie cookie in container.GetCookies(uri))
                    cookies.Add(cookie);
            }

            return cookies;
        }

        internal static string GetInputValue(this CQ input, string elementName)
        {
            var fkeyE = input["input"].FirstOrDefault(e => e.Attributes["name"] == elementName);
            return fkeyE?.Attributes["value"];
        }

        public static List<Message> GetMessagesByUser(this IEnumerable<Message> messages, User user)
        {
            if (user == null) throw new ArgumentNullException("user");
            return messages.GetMessagesByUser(user.ID);
        }

        public static List<Message> GetMessagesByUser(this IEnumerable<Message> messages, int userID)
        {
            if (messages == null) throw new ArgumentNullException("messages");

            var userMessages = new List<Message>();

            foreach (var m in messages)
                if (m.Author.ID == userID)
                    userMessages.Add(m);

            return userMessages;
        }

        public static bool IsReply(this string message)
        {
            return !string.IsNullOrEmpty(message) && isReply.IsMatch(message);
        }
    }
}
