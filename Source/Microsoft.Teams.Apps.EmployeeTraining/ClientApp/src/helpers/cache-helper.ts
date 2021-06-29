// <copyright file="cache-helper.ts" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

const ONE_DAY = 1000 * 60 * 60 * 24;

/**
 * Gets the data from local storage cache.
 * @param cacheName Name of the cache to get.
 */
export const getDataFromCache = (cacheName: string) => {
    let supportedCultureCache = {
        data: {},
        nextCleanup: new Date().getTime() + ONE_DAY
    }

    try {
        const data = localStorage.getItem(cacheName)
        if (data) {
            supportedCultureCache = JSON.parse(data)

            // Remove cache if expired.
            if (new Date().getTime() > supportedCultureCache.nextCleanup) {
                localStorage.removeItem(cacheName);
                return {
                    data: {},
                    nextCleanup: new Date().getTime() + ONE_DAY
                }
            }
        }
    }
    catch (e) {
        console.error(e.message)
    }

    return supportedCultureCache
}

/**
 * Set the cache in local storage.
 * @param cacheName Name of the cache.
 * @param key Cache key.
 * @param value The value that need to stored.
 */
export const setDataToCache = (cacheName: string, key: string, value: any) => {
    const supportedCultureCache = getDataFromCache(cacheName);
    const data = supportedCultureCache.data;
    data[key] = value;
    try {
        localStorage.setItem(cacheName, JSON.stringify(supportedCultureCache))
    }
    catch (e) {
        console.error(e.message)
        localStorage.removeItem(cacheName);
    }
}