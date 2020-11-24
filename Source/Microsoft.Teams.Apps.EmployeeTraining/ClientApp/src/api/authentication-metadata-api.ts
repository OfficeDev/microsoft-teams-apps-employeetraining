// <copyright file="authentication-metadata-api.ts" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import axios from "./axios-decorator";
import { AxiosRequestConfig } from "axios";
import Constants from "../constants/constants";
import { getAPIRequestConfigParams } from "../helpers/api-helper";

/**
* Get authentication metadata from API
* @param  {String} windowLocationOriginDomain Application base URL
* @param  {String} login_hint Login hint for SSO
*/
export const getAuthenticationConsentMetadata = async (windowLocationOriginDomain: string, login_hint: string): Promise<any> => {
    let url = `${Constants.apiBaseURL}/authenticationMetadata/consentUrl`;
    let config: AxiosRequestConfig = getAPIRequestConfigParams({
        windowLocationOriginDomain: windowLocationOriginDomain,
        loginhint: login_hint
    });

    return await axios.get(url, config, false);
}