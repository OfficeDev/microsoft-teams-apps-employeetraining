// <copyright file="i18n.ts" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import i18n from "i18next";
import { initReactI18next } from "react-i18next";
import moment from 'moment';
import 'moment/min/locales.min';
import Backend from 'i18next-xhr-backend';
import * as microsoftTeams from "@microsoft/teams-js";

let locale = "en-US";
microsoftTeams.initialize();
microsoftTeams.getContext((context: microsoftTeams.Context) => {
    i18n.changeLanguage(context.locale!);
    moment.locale(context.locale!);
});

i18n
.use(Backend)
.use(initReactI18next) // passes i18n down to react-i18next
.init({
    lng: locale,
    fallbackLng: locale,
    keySeparator: false, // we do not use keys in form messages.welcome
    interpolation: {
        escapeValue: false // react already safes from xss
    },
    react: {
        useSuspense: true
    }
});

export const updateLocale = () => {
    const search = window.location.search;
    const params = new URLSearchParams(search);
    const loc = params.get("locale") || locale;    
    i18n.changeLanguage(loc);
    moment.locale(loc);
};

export const formatDate = (date: string) => {
    if (!date) return date;
    return moment(date).format('l LT');
}

export default i18n;