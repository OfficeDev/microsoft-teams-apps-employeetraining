// <copyright file="user-events-api.ts" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import axios from "./axios-decorator";
import baseAxios, { AxiosRequestConfig } from "axios";

/** The base URL for API */
const baseURL = window.location.origin + "/api";

/**
 * Register to an event
 * @param eventId The event Id in which registration need to be done
 */
export const getEventsAsync = async (
    searchString: string, pageCount: number, eventSearchType: number, createdByFilter: string, categoryFilter: string, sortByFilter: number) => {
    let url = `${baseURL}/event/UserEvents`;
    let config: AxiosRequestConfig = baseAxios.defaults;
    config.params = {
        searchString: encodeURIComponent(searchString),
        pageCount: pageCount,
        eventSearchType: eventSearchType,
        createdByFilter: createdByFilter,
        categoryFilter: categoryFilter,
        sortBy: sortByFilter
    };

    return axios.get(url, config);
}

/**
 * Registers user to an event
 * @param teamId The LnD team ID who created the event
 * @param eventId The event Id in which registration need to be done
 */
export const registerToEventAsync = (teamId: string, eventId: string) => {
    let url = `${baseURL}/event/RegisterToEvent?teamId=${teamId}&eventId=${eventId}`;
    let config: AxiosRequestConfig = baseAxios.defaults;
    config.params = {
        teamId: teamId,
        eventId: eventId
    };

    return axios.post(url, null, config);
}

/**
 * Un-register user to an event
 * @param teamId The LnD team ID who created the event
 * @param eventId The event Id in which registration need to be cancelled
 */
export const removeEventAsync = (teamId: string, eventId: string) => {
    let url = `${baseURL}/event/UnregisterToEvent`;
    let config: AxiosRequestConfig = baseAxios.defaults;
    config.params = {
        teamId: teamId,
        eventId: eventId
    };

    return axios.post(url, null, config);
}