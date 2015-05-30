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
using System.Net;
using CsQuery;

namespace ChatExchangeDotNet
{
    public class Message
    {
        private bool stripMention;
        private string content;

        public int ID { get; private set; }
        public string AuthorName { get; private set; }
        public int AuthorID { get; private set; }
        public int ParentID { get; private set; }
        public string Host { get; private set; }
        public int RoomID { get; private set; }

        public string Content
        {
            get
            {
                return content;
            }
        }

        public int StarCount
        {
            get
            {
                var resContent = RequestManager.SendGETRequest("", "http://chat." + Host + "/messages/" + ID + "/history");

                if (string.IsNullOrEmpty(resContent)) { return -1; }

                var dom = CQ.Create(resContent)[".stars"];
                var count = 0;

                if (dom != null && dom.Length != 0)
                {
                    if (dom[".times"] != null && !string.IsNullOrEmpty(dom[".times"].First().Text()))
                    {
                        count = int.Parse(dom[".times"].First().Text());
                    }
                    else
                    {
                        count = 1;
                    }
                }

                return count;
            }
        }

        public int PinCount
        {
            get
            {
                var resContent = RequestManager.SendGETRequest("", "http://chat." + Host + "/messages/" + ID + "/history");

                if (string.IsNullOrEmpty(resContent)) { return -1; }

                var dom = CQ.Create(resContent)[".owner-star"];
                var count = 0;

                if (dom != null && dom.Length != 0)
                {
                    if (dom[".times"] != null && !string.IsNullOrEmpty(dom[".times"].First().Text()))
                    {
                        count = int.Parse(dom[".times"].First().Text());
                    }
                    else
                    {
                        count = 1;
                    }
                }

                return count;
            }
        }



        public Message(string host, int roomID, int messageID, string authorName, int authorID, bool stripMention = true, int parentID = -1)
        {
            EventManager temp = null;
            var ex = Initialise(host, roomID, messageID, authorName, authorID, stripMention, parentID, ref temp);

            if (ex != null)
            {
                throw ex;
            }
        }

        public Message(ref EventManager evMan, string host, int roomID, int messageID, string authorName, int authorID, bool stripMention = true, int parentID = -1)
        {
            var ex = Initialise(host, roomID, messageID, authorName, authorID, stripMention, parentID, ref evMan);

            if (ex != null)
            {
                throw ex;
            }
        }



        public static string GetMessageContent(string host, int messageID, bool stripMention = true)
        {
            using (var res = RequestManager.SendGETRequestRaw("", "http://chat." + host + "/message/" + messageID + "?plain=true"))
            {
                if (res == null || res.StatusCode != HttpStatusCode.OK) { return null; }

                var content = RequestManager.GetResponseContent(res);

                return string.IsNullOrEmpty(content) ? null : WebUtility.HtmlDecode(stripMention ? content.StripMention() : content);
            }
        }



        private Exception Initialise(string host, int roomID, int messageID, string authorName, int authorID, bool stripMention, int parentID, ref EventManager evMan)
        {
            if (string.IsNullOrEmpty(host)) { return new ArgumentException("'host' can not be null or empty.", "host"); }
            if (messageID < 0) { return new ArgumentOutOfRangeException("messageID", "'ID' can not be less than 0."); }
            if (string.IsNullOrEmpty(authorName)) { return new ArgumentException("'authorName' can not be null or empty.", "authorName"); }
            if (authorID < -1) { return new ArgumentOutOfRangeException("authorID", "'authorID' can not be less than -1."); }

            this.stripMention = stripMention;
            content = GetMessageContent(host, messageID, stripMention);
            Host = host;
            RoomID = roomID;
            ID = messageID;
            AuthorName = authorName;
            AuthorID = authorID;
            ParentID = parentID;

            if (evMan != null)
            {
                evMan.ConnectListener(EventType.MessageEdited, new Action<Message, Message>((oldMessage, newMessage) =>
                {
                    if (oldMessage.ID == messageID)
                    {
                        content = newMessage.Content;
                    }
                }));
            }

            return null;
        }
    }
}
