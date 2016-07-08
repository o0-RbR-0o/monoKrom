using System;
using System.Collections.Generic;
using System.Xml;

/*

	Simple quick and dirty parser for messages going from Partymeister to the screen server.
	Version: 0.4
	Author : 2016 by Kim Oliver "RbR" Schweikert
	License: CC-by-nc

*/


namespace monoKrom.PartyParser
{
    ///<summary>
    ///A small piece of code for parsing Partymeister messages to the screen server. This is not finished yet!
    ///</summary>
    class PartyParser
    {
        ///<summary>
        ///Request types: Partymeister messages have one of those types
        ///</summary>
        public enum RequestType { Unknown, Next, Previous, NextHard, PreviousHard, Playlist, GetPlaylists, Seek, SeekHard }

        XmlDocument xmld = new XmlDocument();
		
        ///<summary>
        ///A Playlist entry (a slide in most cases)
        ///</summary>
        public class PlaylistElement
        {
            ///<summary>
            ///Either image or video
            ///</summary>
            public enum PlaylistElementType { Image, Video }

            private string name;
            private string path;
            private PlaylistElementType type;
            private int duration;
            private int midi;
            private string slidetype;
            private int transitiontype;
            private int transitionduration;
            private bool manualadvance;
            private bool mute;
			
            ///<summary>
            ///The name of the element
            ///</summary>
            public string Name
            {
                get { return name; }
                set { name = value; }
            }

            ///<summary>
            ///The path of the element to download from
            ///</summary>
            public string Path
            {
                get { return path; }
                set { path = value; }
            }

            ///<summary>
            ///The type of this element
            ///</summary>
            public PlaylistElementType Type
            {
                get { return type; }
                set { type = value; }
            }

            ///<summary>
            ///The duration of this slides in milliseconds (!Note: Partymeister is using seconds by default.)
            ///</summary>
            public int Duration
            {
                get { return duration; }
                set { duration = value; }
            }
        
            ///<summary>
            ///?! Midi command to send with this slide?! Some more reversing work needed here
            ///</summary>
            public int Midi
            {
                get { return midi; }
                set { midi = value; }
            }

            ///<summary>
            ///The type of the slide template
            ///</summary>
            public string Slidetype
            {
                get { return slidetype; }
                set { slidetype = value; }
            }
            
            ///<summary>
            ///The id of the transition used by this slide
            ///</summary>
            public int Transitiontype
            {
                get { return transitiontype; }
                set { transitiontype = value; }
            }
            
            ///<summary>
            ///The duration in milliseconds for the transition of this slide
            ///</summary>
            public int Transitionduration
            {
                get { return transitionduration; }
                set { transitionduration = value; }
            }
            
            ///<summary>
            ///Manual advance and no timer if true
            ///</summary>
            public bool Manualadvance
            {
                get { return manualadvance; }
                set { manualadvance = value; }
            }
            
            ///<summary>
            ///Mute this slide?
            ///</summary>
            public bool Mute
            {
                get { return mute; }
                set { mute = value; }
            }
        }


        ///<summary>
        ///Just load in the xml coming from partymeister and you will be fine. In most cases.
        ///</summary>
        ////// <param name="xml">The xml string coming from Partymeister</param>
        public PartyParser(string xml)
        {
            xmld.LoadXml(xml);
        }


        ///<summary>
        ///Get a PlaylistElement with specified name
        ///</summary>
        ///<param name="name">The name of the element to get</param>
        ///<returns>A Playlist element</returns>
        public PlaylistElement parsePlaylistItem(string name)
        {
            //Select the playlist item node with given name
            XmlNode itemnode = xmld.SelectSingleNode(string.Format("/xml/data/item[@name='{0}']", name));

            //Create a new playlist element
            PlaylistElement p = new PlaylistElement();

            //Set the name of the playlist element
            p.Name = name;

            //Get the type of the element.
            switch (itemnode.Attributes["type"].Value)
            {
                case "image": p.Type = PlaylistElement.PlaylistElementType.Image; break;
                case "video": p.Type = PlaylistElement.PlaylistElementType.Video; break;
            }

            //Fill playlist element with values from xml
            p.Path = itemnode.SelectSingleNode("path").InnerText;
            p.Duration = Convert.ToInt32(itemnode.SelectSingleNode("duration").InnerText)*1000;
            p.Midi = Convert.ToInt32(itemnode.SelectSingleNode("midi").InnerText);
            p.Slidetype = itemnode.SelectSingleNode("slide_type").InnerText;
            p.Transitiontype = Convert.ToInt32(itemnode.SelectSingleNode("transition").InnerText);
            p.Transitionduration = Convert.ToInt32(itemnode.SelectSingleNode("transition").Attributes["duration"].Value);
            p.Manualadvance = itemnode.SelectSingleNode("manual_advance").InnerText == "1";
            p.Mute = itemnode.SelectSingleNode("mute").InnerText == "1";

            //And return it
            return p;
        }


        ///<summary>
        ///Get all playlist item names
        ///</summary>
        ///<returns>A list with playlist item names</returns>
        public List<string> getPlaylistItemNames()
        {
            List<string> names = new List<string>();

            XmlNodeList itemlist = xmld.SelectNodes("/xml/data/item");

            foreach (XmlNode n in itemlist)
            {
                names.Add(n.Attributes["name"].Value);
            }
            return names;
        }

		
        ///<summary>
        ///Get the name of the playlist
        ///</summary>
        ///<returns>The name of the playlist</returns>
        public string getPlaylistName()
        {
            return getParameter("name");
        }


        ///<summary>
        ///Get a parameter with specified name
        ///</summary>
        ///<returns>The value of the specified parameter</returns>
        public string getParameter(string name)
        {
            return xmld.SelectSingleNode(String.Format("/xml/parameter[@name='{0}']", name)).InnerText;
        }


        ///<summary>
        ///Get the type of the request. See Enum RequestType for more information about request types
        ///</summary>
        ///<returns>The request type</returns>
        public RequestType getRequestType()
        {
            XmlNode item = xmld.SelectSingleNode("/xml/rpc");
            
            switch (item.Attributes["name"].Value)
            {
                case "playlist": return RequestType.Playlist;
                case "next":  return getParameter("hard")=="0"?RequestType.Next:RequestType.NextHard;
                case "previous": return getParameter("hard") == "0" ? RequestType.Previous : RequestType.PreviousHard;
                case "seek": return getParameter("hard") == "0" ? RequestType.Seek : RequestType.SeekHard;
                case "get_playlists": return RequestType.GetPlaylists;
                default: return RequestType.Unknown;
            }
        }

         
        ///<summary>
        ///Get all playlist elements with their values in a list. See class PlaylistElement for mor information
        ///</summary>
        ///<returns>All playlist elements found in the xml</returns>
        public List<PlaylistElement> getAllPlaylistElements()
        {
            List<PartyParser.PlaylistElement> ple = new List<PartyParser.PlaylistElement>();
            foreach (string name in getPlaylistItemNames())
            {
                ple.Add(parsePlaylistItem(name));
            }
            return ple;
        }
    }
}
