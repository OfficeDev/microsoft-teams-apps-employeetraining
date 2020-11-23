// <copyright file="manage-categories-api.ts" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import axios from "./axios-decorator";
import { AxiosRequestConfig } from "axios";
import { ICategory } from "../models/ICategory";
import Constants from "../constants/constants";
import { getAPIRequestConfigParams } from "../helpers/api-helper";

/**
 * Gets event categories
 * @param teamId The LnD team Id
 */
export const getCategoriesAsync = async (teamId: string) => {
    let url = `${Constants.apiBaseURL}/category`;
    let config: AxiosRequestConfig = getAPIRequestConfigParams({ teamId: teamId });

    return await axios.get(url, config);
}

/**
 * Gets event categories
 * @param teamId The LnD team Id
 */
export const getEventCategoriesAsync = async () => {
    let url = `${Constants.apiBaseURL}/category/get-categories-for-event`;
    return await axios.get(url);
}

/**
* Creates a new category
* @param teamId The LnD team Id
* @param category The category details that needs to be created
*/
export const createCategoryAsync = async (teamId: string, category: ICategory) => {
    let url = `${Constants.apiBaseURL}/category`;
    let config: AxiosRequestConfig = getAPIRequestConfigParams({ teamId: teamId });

    return await axios.post(url, category, config);
}

/**
* Updates category details
* @param teamId The LnD team Id
* @param category The category details that needs to be updated
*/
export const updateCategoryAsync = async (teamId: string, category: ICategory) => {
    let url = `${Constants.apiBaseURL}/category`;
    let config: AxiosRequestConfig = getAPIRequestConfigParams({ teamId: teamId });

    return await axios.patch(url, category, config);
}

/**
* Deletes categories
* @param teamId The LnD team Id
* @param categoryIds The comma separated category IDs that needs to be deleted
*/
export const deleteCategoriesAsync = async (teamId: string, categoryIds: string) => {
    let url = `${Constants.apiBaseURL}/category`;
    let config: AxiosRequestConfig = getAPIRequestConfigParams({ teamId: teamId, categoryIds: categoryIds });

    return await axios.delete(url, config);
}