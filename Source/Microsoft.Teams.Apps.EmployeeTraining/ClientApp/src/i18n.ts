// <copyright file="i18n.ts" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import i18n from "i18next";
import { initReactI18next } from "react-i18next";
import Backend from 'i18next-xhr-backend';
import * as microsoftTeams from "@microsoft/teams-js";
import { getDefaultCultureAsync, getSupportedCulturesAsync } from "./api/culture-metadata-api";
import { getDataFromCache, setDataToCache } from "./helpers/cache-helper";

const SUPPORTEDCULTURE_CACHE = "SUPPORTEDCULTURE_CACHE"
const SupportedCulturesKey = "SupportedCultures"
const DEFAULTCULTURE_CACHE = "DEFAULTCULTURE_CACHE"
const DefaultCultureKey = "DefaultCulture"

let locale = "en";
let defaultLocale = "en";
microsoftTeams.initialize();
microsoftTeams.getContext((context: microsoftTeams.Context) => {
    // Get suported cultures and default culture from cache.
    const supportedCulturesCache = getDataFromCache(SUPPORTEDCULTURE_CACHE);
    const defaultCultureCache = getDataFromCache(DEFAULTCULTURE_CACHE);

    // Check if cache is present.
    if (supportedCulturesCache.data[SupportedCulturesKey] && defaultCultureCache.data[DefaultCultureKey]) {
        if (supportedCulturesCache.data[SupportedCulturesKey].includes(context.locale.split("-")[0]) || supportedCulturesCache.data[SupportedCulturesKey].includes(context.locale)) {
            i18n.changeLanguage(context.locale!);
        }
        else {
            i18n.changeLanguage(defaultCultureCache.data[DefaultCultureKey]);
        }
    }
    else {
        getSupportedCulturesAsync().then((result: any) => {
            const supportedCultures = result.data;

            // Set supported culture to cache.
            setDataToCache(SUPPORTEDCULTURE_CACHE, SupportedCulturesKey, supportedCultures);
            if (supportedCultures.includes(context.locale.split("-")[0]) || supportedCultures.includes(context.locale)) {
                i18n.changeLanguage(context.locale!);
            }
            else {
                getDefaultCultureAsync().then((result: any) => {
                    defaultLocale = result.data;

                    // Set default culture to cache.
                    setDataToCache(DEFAULTCULTURE_CACHE, DefaultCultureKey, defaultLocale);
                    i18n.changeLanguage(defaultLocale);
                });
            }
        });
    }
});

i18n
.use(Backend)
.use(initReactI18next) // passes i18n down to react-i18next
.init({
    lng: locale,
    fallbackLng: defaultLocale,
    keySeparator: false, // we do not use keys in form messages.welcome
    interpolation: {
        escapeValue: false // react already safes from xss
    },
    react: {
        useSuspense: true
    }
});

export default i18n;