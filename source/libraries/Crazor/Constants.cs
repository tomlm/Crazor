﻿// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

namespace Crazor
{
    public class Constants
    {
        // action.data keys
        public const string SESSION_KEY = "_session";
        public const string EDITSESSION_KEY = "_editsessionid";
        public const string ROUTE_KEY = "_route";
        public const string IDDATA_KEY = "_id";
        public const string SUBMIT_VERB = "_verb";

        // well known verbs
        public const string CANCEL_VERB = "OnCancel";
        public const string OK_VERB = "OnOK";
        public const string LOADROUTE_VERB = "OnLoadRoute";
        public const string SHOWVIEW_VERB = "OnShowView";
        public const string ONEDIT_VERB = "OnEdit";

        // well known screens
        public const string DEFAULT_VIEW = "Default";
        public const string ABOUT_VIEW = "About";
        public const string SETTINGS_VIEW = "Settings";
    }
}
