import { ResponseStatus } from "../../constants/constants";
// <copyright file="manage-categories-api.ts" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import { ICategory } from "../../models/ICategory";

/**
 * Gets event categories
 * @param teamId The LnD team ID
 */
export const getCategoriesAsync = async (teamId: string) => {
    let categories: Array<ICategory> = [
        { categoryId: "1", description: "description1", isInUse: false, isSelected: false, name: "category name1", createdBy: "test", createdOn: new Date(), updatedBy: "test", updatedOn: new Date() },
        { categoryId: "2", description: "description2", isInUse: false, isSelected: false, name: "category name2", createdBy: "test", createdOn: new Date(), updatedBy: "test", updatedOn: new Date() },
        { categoryId: "3", description: "description3", isInUse: true, isSelected: false, name: "category name3", createdBy: "test", createdOn: new Date(), updatedBy: "test", updatedOn: new Date() }
    ]
    return Promise.resolve({
        data: categories,
        status: ResponseStatus.OK
    });
}

/**
 * Gets event categories
 * @param teamId The LnD team ID
 */
export const getEventCategoriesAsync = async () => {
    let categories: Array<ICategory> = [
        { categoryId: "1", description: "description1", isInUse: false, isSelected: false, name: "category name1", createdBy: "test", createdOn: new Date(), updatedBy: "test", updatedOn: new Date() },
        { categoryId: "2", description: "description2", isInUse: false, isSelected: false, name: "category name2", createdBy: "test", createdOn: new Date(), updatedBy: "test", updatedOn: new Date() },
        { categoryId: "3", description: "description3", isInUse: true, isSelected: false, name: "category name3", createdBy: "test", createdOn: new Date(), updatedBy: "test", updatedOn: new Date() }
    ]
    return Promise.resolve({
        data: categories,
        status: ResponseStatus.OK
    });
}

/**
* Creates a new category
* @param teamId The LnD team ID
* @param category The category details that needs to be created
*/
export const createCategoryAsync = async (teamId: string, category: ICategory) => {
    return Promise.resolve({
        data: true,
        status: ResponseStatus.OK
    });
}

/**
* Updates category details
* @param teamId The LnD team ID
* @param category The category details that needs to be updated
*/
export const updateCategoryAsync = async (teamId: string, category: ICategory) => {
    return Promise.resolve({
        data: true,
        status: ResponseStatus.OK
    });
}

/**
* Deletes categories
* @param teamId The LnD team ID
* @param categoryIds The comma separated category IDs that needs to be deleted
*/
export const deleteCategoriesAsync = async (teamId: string, categoryIds: string) => {
    return Promise.resolve({
        status: ResponseStatus.OK,
        data: false
    });
}