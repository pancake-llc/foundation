using System;

namespace Pancake.Feedback
{
    /// <summary>
    /// Board data returned from Trello API
    /// </summary>
    [Serializable]
    public struct Board
    {
        public string id, name, desc;
        public object descData;
        public bool closed;
        public string idOrganization;
        public bool pinned;
        public string url, shortUrl;
        public Prefs prefs;
        public LabelNames labelNames;
    }

    [Serializable]
    public struct BoardCollection
    {
        public Board[] boards;
    }

    /// <summary>
    /// Board preferences
    /// </summary>
    [Serializable]
    public struct Prefs
    {
        public string background;
        public object backgroundImage, backgroundImageScaled;
        public string backgroundBrightness, backgroundColor;
        public bool? backgroundTile;
        public bool? calendarFeedEnabled;
        public bool? canBePublic, canBeOrg, canBePrivate, canInvite;
        public CardAgeMode? cardAging;
        public Invitations? invitations;
        public PermissionLevel? permissionLevel;
        public bool? selfJoin, cardCovers;
        public AccessibilityLevel? voting, comments;
    }

    [Serializable]
    public struct LabelNames
    {
        public string green, yellow, orange, red, purple, blue, sky, lime;
    }

    [Serializable]
    public struct Label
    {
        public string id, idBoard, name, color;
        public int uses, order;

        public Label(string id = null, string idBoard = null, string name = null, string color = null, int uses = 0, int order = 0)
        {
            this.id = id;
            this.idBoard = idBoard;
            this.name = name;
            this.color = color;
            this.uses = uses;
            this.order = order;
        }
    }

    [Serializable]
    public struct LabelCollection
    {
        public Label[] labels;
    }

    [Serializable]
    public struct List
    {
        public string id, name;
        public bool closed;
        public string idBoard;
        public float pos;
        public bool subscribed;
    }

    /// <summary>
    /// Object for GETting the subscribed value
    /// Trello has an underscore on value here, annoying
    /// </summary>
    [Serializable]
    public struct Subscribed
    {
        public bool _value;
    }


    [Serializable]
    public struct ListCollection
    {
        public List[] lists;
    }

    public enum PermissionLevel
    {
        org,
        @private,
        @public
    }

    public enum Invitations
    {
        admins,
        members
    }

    public enum AccessibilityLevel
    {
        disabled,
        members,
        observers,
        org,
        @public
    }

    public enum CardAgeMode
    {
        regular,
        pirate
    }

    [Serializable]
    public class AddCardResponse
    {
        public string id;
        public Badges badges;
        public bool[] checkItemStates;
        public bool closed;
        public DateTime dateLastActivity;
        public string desc;
        public Descdata descData;
        public string due;
        public bool dueComplete;
        public string email;
        public string idBoard;
        public string[] idChecklists;
        public string[] idLabels;
        public string idList;
        public string[] idMembers;
        public int idShort;
        public string idAttachmentCover;
        public bool manualCoverAttachment;
        public CardLabel[] labels;
        public string name;
        public int pos;
        public string shortUrl;
        public string url;
        public string[] stickers;
    }

    [Serializable]
    public class Badges
    {
        public int votes;
        public bool viewingMemberVoted;
        public bool subscribed;
        public string fogbugz;
        public int checkItems;
        public int checkItemsChecked;
        public int comments;
        public int attachments;
        public bool description;
        public string due;
        public bool dueComplete;
    }

    [Serializable]
    public class Descdata
    {
        public Emoji emoji;
    }

    [Serializable]
    public class Emoji
    {
    }

    [Serializable]
    public class CardLabel
    {
        public string id;
        public string idBoard;
        public string name;
        public string color;
        public int uses;
    }
}