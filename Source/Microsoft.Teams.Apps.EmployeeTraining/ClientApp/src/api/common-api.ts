// <copyright file="common-api.ts" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import axios from "./axios-decorator";
import { AxiosRequestConfig } from "axios";
import Constants from "../constants/constants";
import { getAPIRequestConfigParams } from "../helpers/api-helper";

/**
 * Gets event details
 * @param eventId The event ID of which details need to be retrieved
 * @param teamId The LnD team ID
 */
export const getEventAsync = async (eventId: string, teamId: string) => {
    let url = `${Constants.apiBaseURL}/Event`;
    let config: AxiosRequestConfig = getAPIRequestConfigParams({ eventId: eventId, teamId: teamId });

    return await axios.get(url, config);
}