// <copyright file="culture-metadata-api.ts" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import axios from "./axios-decorator";
import Constants from "../constants/constants";

/**
* Get default culture from API.
*/
export const getDefaultCultureAsync = async (): Promise<any> => {
    let url = `${Constants.apiBaseURL}/cultureMetadata`;

    return await axios.get(url);
}

/**
* Get supported cultures from API.
*/
export const getSupportedCulturesAsync = async (): Promise<any> => {
    let url = `${Constants.apiBaseURL}/cultureMetadata/supportedcultures`;

    return await axios.get(url);
}